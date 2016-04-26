// Copyright 2016 door Erik de Roos
using ILiturgieDatabase;
using ISlideBuilder;
using mppt.Connect;
using mppt.LiedPresentator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tools;

namespace mppt.RegelVerwerking
{
    class VerwerkerBijbeltekst : IVerwerkFactory
    {
        public IVerwerk Init(IMppApplication metApplicatie, IMppPresentatie toevoegenAanPresentatie, IMppFactory metFactory, ILiedFormatter gebruikLiedFormatter, IBuilderBuildSettings buildSettings,
                IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, IEnumerable<ILiturgieRegel> volledigeLiturgieOpVolgorde)
        {
            return new Verwerker(metApplicatie, toevoegenAanPresentatie, metFactory, gebruikLiedFormatter, buildSettings, buildDefaults, dependentFileList, volledigeLiturgieOpVolgorde);
        }

        private class Verwerker : VerwerkBase, IVerwerk
        {
            private IMppPresentatie _presentatie { get; }
            private IMppFactory _mppFactory { get; }
            private ILiedFormatter _liedFormatter { get; }
            private IBuilderBuildSettings _buildSettings { get; }
            private IBuilderBuildDefaults _buildDefaults { get; }
            private IBuilderDependendFiles _dependentFileList { get; }
            private IEnumerable<ILiturgieRegel> _liturgie { get; }
            private int _slidesGemist = 0;

            public Verwerker(IMppApplication metApplicatie, IMppPresentatie toevoegenAanPresentatie, IMppFactory metFactory, ILiedFormatter gebruikLiedFormatter, IBuilderBuildSettings buildSettings,
                IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, IEnumerable<ILiturgieRegel> volledigeLiturgieOpVolgorde) : base(metApplicatie)
            {
                _presentatie = toevoegenAanPresentatie;
                _mppFactory = metFactory;
                _liedFormatter = gebruikLiedFormatter;
                _buildSettings = buildSettings;
                _buildDefaults = buildDefaults;
                _dependentFileList = dependentFileList;
                _liturgie = volledigeLiturgieOpVolgorde;
            }

            public IVerwerkResultaat Verwerk(ILiturgieRegel regel, ILiturgieRegel volgende)
            {
                InvullenTekstOpTemplate(regel, volgende);

                return new VerwerkResultaat()
                {
                    SlidesGemist = _slidesGemist
                };
            }

            private void InvullenTekstOpTemplate(ILiturgieRegel regel, ILiturgieRegel volgende)
            {
                var lengteBerekenaar = new LengteBerekenaar(35, "Verdana", 32);
                var tekstPerSlide = OpdelenPerSlide(TekstOpknippen(regel.Content), _buildDefaults.RegelsPerBijbeltekstSlide, lengteBerekenaar);

                //zolang er nog iets is in te voegen in sheets
                foreach (var tekst in tekstPerSlide)
                {
                    //regel de template om de bijbeltekst op af te beelden
                    var presentatie = OpenPps(_dependentFileList.FullTemplateBijbeltekst);
                    var slide = presentatie.EersteSlide();  //alleen eerste slide gebruiken we
                                                            //voor elk object op de slides (we zoeken naar de tekst die vervangen moet worden in de template)
                    foreach (var shape in slide.Shapes().Where(s => s is IMppShapeTextbox).Cast<IMppShapeTextbox>())
                    {
                        var text = shape.Text;
                        //als de template de tekst bevat "Liturgieregel" moet daar de liturgieregel komen
                        if (text.Equals("<Liturgieregel>"))
                            shape.Text = _liedFormatter.Huidig(regel, null).Display;
                        //als de template de tekst bevat "Inhoud" moet daar de inhoud van het vers komen
                        else if (text.Equals("<Inhoud>"))
                            shape.Text = tekst.ToString();
                        //als de template de tekst bevat "Volgende" moet daar de Liturgieregel van de volgende sheet komen
                        else if (text.Equals("<Volgende>"))
                        {
                            //we moeten dan wel al op de laatste slide zitten ('InvullenVolgende' is wel al intelligent maar in het geval van 1
                            //lange tekst over meerdere dia's kan 'InvullenVolgende' niet de juiste keuze maken)
                            var display = tekstPerSlide.Last() == tekst ? _liedFormatter.Volgende(volgende) : null;
                            shape.Text = display != null ? $"{_buildDefaults.LabelVolgende} {display.Display}" : string.Empty;
                        }
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
                        if (CouldAdd(verzameldeRegels.Count, lengteBerekenaar.VerbruiktPercentageVanRegel(nogOver), regelWoorden, regelsPerSlide, lengteBerekenaar))
                        {
                            var result = DoAdd(nogOver, regelWoorden, lengteBerekenaar);
                            verzameldeRegels.AddRange(result.AddRows);
                            nogOver = result.Over;
                        }
                        else
                        {
                            if (nogOver.Length > 0)
                                verzameldeRegels.Add(nogOver);
                            yield return new SlideData() { Regels = verzameldeRegels.Select(r => r.Trim()).ToList() };
                            verzameldeRegels = new List<string>();
                            var result = DoAdd(string.Empty, regelWoorden, lengteBerekenaar);
                            verzameldeRegels.AddRange(result.AddRows);
                            nogOver = result.Over;
                        }
                    }
                    // Laatste restje van het blok nog toevoegen
                    if (nogOver.Length > 0)
                        verzameldeRegels.Add(nogOver);
                    
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
                var regelBuildPercentage = nogOverPercentage + lengteBerekenaar.VerbruiktPercentageVanRegel(" ");
                var verzameldeLengte = 0;
                foreach (var woord in regelWoorden)
                {
                    var woordVerbruikPercentage = lengteBerekenaar.VerbruiktPercentageVanRegel(woord);
                    if (regelBuildPercentage + woordVerbruikPercentage < 100)
                    {
                        regelBuildPercentage += woordVerbruikPercentage;
                        continue;
                    }
                    else if (regelBuildPercentage + lengteBerekenaar.VerbruiktPercentageVanRegel(woord.Trim()) < 100)
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
                var regelBuildPercentage = lengteBerekenaar.VerbruiktPercentageVanRegel(builder.ToString());
                var verzameldeRegels = new List<string>();
                foreach (var woord in regelWoorden)
                {
                    var woordVerbruikPercentage = lengteBerekenaar.VerbruiktPercentageVanRegel(woord);
                    if (regelBuildPercentage + woordVerbruikPercentage < 100)
                    {
                        regelBuildPercentage += woordVerbruikPercentage;
                        builder.Append(woord);
                        continue;
                    }
                    else if (regelBuildPercentage + lengteBerekenaar.VerbruiktPercentageVanRegel(woord.Trim()) < 100)
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
                public IEnumerable<string> Regels { get; set; }
                public override string ToString()
                {
                    return string.Join("\r\n", Regels);
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

            private interface ILengteBerekenaar
            {
                float VerbruiktPercentageVanRegel(string tekst);
            }
            private class LengteBerekenaar : ILengteBerekenaar
            {
                private System.Drawing.Font _font { get; }
                private System.Drawing.Size _baseSize { get; }

                public LengteBerekenaar(int lettersaOpEenRegel, string vanLettertype, float metPuntGrootte)
                {
                    var initString = new string(Enumerable.Repeat('a', lettersaOpEenRegel).ToArray());
                    _font = new System.Drawing.Font(vanLettertype, metPuntGrootte);
                    _baseSize = TextRenderer.MeasureText(initString, _font);
                }

                public float VerbruiktPercentageVanRegel(string tekst)
                {
                    var size = TextRenderer.MeasureText(tekst, _font);
                    if (size.IsEmpty)
                        return 0;
                    return (float)size.Width * 100 / _baseSize.Width;
                }
            }
        }
    }
}
