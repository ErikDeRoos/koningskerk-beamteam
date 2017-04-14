// Copyright 2017 door Erik de Roos
using ILiturgieDatabase;
using ISlideBuilder;
using mppt.Connect;
using mppt.LiedPresentator;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;

namespace mppt.RegelVerwerking
{
    /// <summary>
    /// Deze klasse implementeert de logica die nodig is om de aangeleverde content (liturgie) voor een
    /// bijbeltekst slide te combineren met de bijbeltekst template
    /// </summary>
    class VerwerkerBijbeltekst : IVerwerkFactory
    {
        public IVerwerk Init(IMppApplication metApplicatie, IMppPresentatie toevoegenAanPresentatie, IMppFactory metFactory, ILiedFormatter gebruikLiedFormatter, IBuilderBuildSettings buildSettings,
                IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, IEnumerable<ILiturgieRegel> volledigeLiturgieOpVolgorde, ILengteBerekenaar lengteBerekenaar)
        {
            return new Verwerker(metApplicatie, toevoegenAanPresentatie, metFactory, gebruikLiedFormatter, buildSettings, buildDefaults, dependentFileList, volledigeLiturgieOpVolgorde, lengteBerekenaar);
        }

        private class Verwerker : VerwerkBase, IVerwerk
        {
            private ILengteBerekenaar _lengteBerekenaar { get; }
            private int _slidesGemist = 0;

            public Verwerker(IMppApplication metApplicatie, IMppPresentatie toevoegenAanPresentatie, IMppFactory metFactory, ILiedFormatter gebruikLiedFormatter, IBuilderBuildSettings buildSettings,
                IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, IEnumerable<ILiturgieRegel> volledigeLiturgieOpVolgorde, ILengteBerekenaar lengteBerekenaar)
                : base(metApplicatie, toevoegenAanPresentatie, metFactory, gebruikLiedFormatter, buildSettings, buildDefaults, dependentFileList, volledigeLiturgieOpVolgorde)
            {
                _lengteBerekenaar = lengteBerekenaar;
            }

            public IVerwerkResultaat Verwerk(ILiturgieRegel regel, IEnumerable<ILiturgieRegel> volgenden)
            {
                InvullenTekstOpTemplate(regel, volgenden);

                return new VerwerkResultaat()
                {
                    SlidesGemist = _slidesGemist
                };
            }

            private void InvullenTekstOpTemplate(ILiturgieRegel regel, IEnumerable<ILiturgieRegel> volgenden)
            {
                var tekstPerSlide = OpdelenPerSlide(TekstOpknippen(regel.Content), _buildDefaults.RegelsPerBijbeltekstSlide, _lengteBerekenaar);

                //zolang er nog iets is in te voegen in sheets
                foreach (var tekst in tekstPerSlide)
                {
                    //regel de template om de bijbeltekst op af te beelden
                    var presentatie = OpenPps(_dependentFileList.FullTemplateBijbeltekst);
                    var slide = presentatie.EersteSlide();  //alleen eerste slide gebruiken we
                                                            //voor elk object op de slides (we zoeken naar de tekst die vervangen moet worden in de template)
                    foreach (var shape in slide.Shapes().Where(s => s is IMppShapeTextbox).Cast<IMppShapeTextbox>())
                    {
                        var tagReplacementResult = ProcessForTagReplacement(shape.Text, regel,
                            additionalSearchForTagReplacement: (s) => {
                                switch (s)
                                {
                                    case "inhoud":
                                        return new SearchForTagReplacementResult(tekst.ToString());
                                    case "volgende":
                                        //we moeten dan wel al op de laatste slide zitten ('InvullenVolgende' is wel al intelligent maar in het geval van 1
                                        //lange tekst over meerdere dia's kan 'InvullenVolgende' niet de juiste keuze maken)
                                        var display = tekstPerSlide.Last() == tekst ? _liedFormatter.Volgende(volgenden) : null;
                                        return new SearchForTagReplacementResult(display != null ? $"{_buildDefaults.LabelVolgende} {display.Display}" : string.Empty);
                                }
                                return SearchForTagReplacementResult.Unresolved;
                            });
                        if (tagReplacementResult.TagsReplaced)
                            shape.Text = tagReplacementResult.NewValue;
                    }
                    //voeg slide in in het grote geheel
                    _slidesGemist += _presentatie.SlidesKopieNaarPresentatie(new List<IMppSlide> { slide });
                    //sluit de template weer af
                    presentatie.Dispose();
                }
            }

            private static IEnumerable<SlideData> OpdelenPerSlide(IEnumerable<TekstBlok> tekst, int regelsPerSlide, ILengteBerekenaar lengteBerekenaar)
            {
                // We moeten goed opletten bij het invullen van een liedtekst op een slide:
                // -Het mogen niet te veel regels zijn (instellingen beperken dat)
                // -Tussenwitregels willen we respecteren
                // -Als we afbreken binnen een bijbeltekst moeten we kijken of we toch niet
                //  naar een voorgaande witruimte kunnen afbreken
                var verzameldeRegels = new List<string>();
                var nogOver = string.Empty;
                foreach (var blok in tekst)
                {
                    // Voeg de tekst uit dit blok toe aan de slide en maak waar nodig nieuwe slides.
                    foreach (var regel in blok.Regels)
                    {
                        var regelWoorden = KnipInWoorden(regel).ToList();
                        if (CouldAdd(verzameldeRegels.Count, lengteBerekenaar.VerbruiktPercentageVanRegel(nogOver, true), regelWoorden, regelsPerSlide, lengteBerekenaar))
                        {
                            var result = DoAdd(nogOver, regelWoorden, lengteBerekenaar);
                            verzameldeRegels.AddRange(result.AddRows);
                            nogOver = result.Over;
                        }
                        else
                        {
                            if (nogOver.Length > 0)
                            {
                                verzameldeRegels.Add(nogOver);
                                nogOver = string.Empty;
                            }
                            yield return new SlideData() { Regels = verzameldeRegels.Select(r => r.Trim()).ToList() };
                            verzameldeRegels = new List<string>();
                            var result = DoAdd(string.Empty, regelWoorden, lengteBerekenaar);
                            verzameldeRegels.AddRange(result.AddRows);
                            nogOver = result.Over;
                        }
                    }
                    // Laatste restje van het blok nog toevoegen
                    if (nogOver.Length > 0)
                    {
                        verzameldeRegels.Add(nogOver);
                        nogOver = string.Empty;
                    }
                    
                    // Blok einde. Check of een witregel nog past.
                    if (verzameldeRegels.Count + 1 >= regelsPerSlide)
                    {
                        yield return new SlideData() { Regels = verzameldeRegels.Select(r => r.Trim()).ToList() };
                        verzameldeRegels = new List<string>();
                    }
                    else
                    {
                        verzameldeRegels.Add(string.Empty);
                    }
                }
                // Laatste restje verzamelde regels naar een nieuwe slide sturen
                if (verzameldeRegels.Any())
                    yield return new SlideData() { Regels = verzameldeRegels.Select(r => r.Trim()).ToList() };
            }

            private static bool CouldAdd(int slideRegelCount, float nogOverPercentage, IEnumerable<string> regelWoorden, int regelsPerSlide, ILengteBerekenaar lengteBerekenaar)
            {
                var regelBuildPercentage = nogOverPercentage + lengteBerekenaar.VerbruiktPercentageVanRegel(" ", false);
                var verzameldeLengte = 0;
                foreach (var woord in regelWoorden)
                {
                    var woordVerbruikPercentage = lengteBerekenaar.VerbruiktPercentageVanRegel(woord, false);
                    if (regelBuildPercentage + woordVerbruikPercentage < 100)
                    {
                        regelBuildPercentage += woordVerbruikPercentage;
                        continue;
                    }
                    else if (regelBuildPercentage + lengteBerekenaar.VerbruiktPercentageVanRegel(woord.Trim(), true) < 100)
                    {
                        regelBuildPercentage += woordVerbruikPercentage;
                        continue;
                    }
                    verzameldeLengte++;
                    regelBuildPercentage = woordVerbruikPercentage;
                    if (slideRegelCount + verzameldeLengte > regelsPerSlide)
                        return false;
                }
                if (regelBuildPercentage > 0 && slideRegelCount + verzameldeLengte + 1 > regelsPerSlide)
                    return false;
                return true;
            }
            private static AddReturnValue DoAdd(string nogOver, IEnumerable<string> regelWoorden, ILengteBerekenaar lengteBerekenaar)
            {
                var builder = new StringBuilder(nogOver);
                if (builder.Length > 0)
                    builder.Append(" ");
                var regelBuildPercentage = lengteBerekenaar.VerbruiktPercentageVanRegel(builder.ToString(), true);
                var verzameldeRegels = new List<string>();
                foreach (var woord in regelWoorden)
                {
                    var woordVerbruikPercentage = lengteBerekenaar.VerbruiktPercentageVanRegel(woord, false);
                    if (regelBuildPercentage + woordVerbruikPercentage < 100)
                    {
                        regelBuildPercentage += woordVerbruikPercentage;
                        builder.Append(woord);
                        continue;
                    }
                    else if (regelBuildPercentage + lengteBerekenaar.VerbruiktPercentageVanRegel(woord.Trim(), true) < 100)
                    {
                        regelBuildPercentage += woordVerbruikPercentage;
                        builder.Append(woord.Trim());
                        continue;
                    }
                    regelBuildPercentage = woordVerbruikPercentage;
                    verzameldeRegels.Add(builder.ToString());
                    builder = new StringBuilder(woord);
                }
                return new AddReturnValue()
                {
                    AddRows = verzameldeRegels,
                    Over = builder.ToString(),
                };
            }

            private static IEnumerable<TekstBlok> TekstOpknippen(IEnumerable<ILiturgieContent> content)
            {
                // Als input hebben we alleen maar losse verzen. De waarde die deze functie toevoegd is het
                // invoegen van onderbrekingen als de nummering onderbroken wordt.
                var verzameldeRegels = new List<Regel>();
                var laatsteNummer = (int?)null;
                foreach (var regel in content)
                {
                    if (regel.InhoudType != InhoudType.Tekst || !regel.Nummer.HasValue)
                        continue;
                    if (laatsteNummer.HasValue && laatsteNummer + 1 != regel.Nummer)
                    {
                        yield return new TekstBlok() { Regels = verzameldeRegels };
                        verzameldeRegels = new List<Regel>();
                        laatsteNummer = null;
                    }
                    laatsteNummer = regel.Nummer;
                    verzameldeRegels.Add(new Regel() { Nummer = regel.Nummer.Value, Tekst = regel.Inhoud.Replace("\n", "").Replace("\r", "") });
                }
                if (verzameldeRegels.Any())
                    yield return new TekstBlok() { Regels = verzameldeRegels };
            }

            private static IEnumerable<string> KnipInWoorden(Regel regel)
            {
                var firstMatch = true;
                foreach (var woord in SplitRegels.KnipInWoorden(regel.Tekst))
                {
                    if (firstMatch)
                        yield return $"{regel.Nummer} {woord}";
                    else
                        yield return woord;
                    firstMatch = false;
                }
            }

            private class SlideData
            {
                private const bool TekstForcerenPerRegel = false;

                public IEnumerable<string> Regels { get; set; }
                public override string ToString()
                {
                    return TekstForcerenPerRegel ? string.Join("\r\n", Regels) : string.Join(" ", Regels);
                }
            }

            private class VerwerkResultaat : IVerwerkResultaat
            {
                public int SlidesGemist { get; set; }
            }

            private class TekstBlok
            {
                public IEnumerable<Regel> Regels { get; set; }
            }
            private class Regel
            {
                public int Nummer { get; set; }
                public string Tekst { get; set; }
            }

            private class AddReturnValue
            {
                public IEnumerable<string> AddRows { get; set; }
                public string Over { get; set; }
            }
        }
    }
}
