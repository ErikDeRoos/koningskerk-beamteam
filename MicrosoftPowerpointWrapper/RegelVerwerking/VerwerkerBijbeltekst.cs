// Copyright 2020 door Erik de Roos
using Generator.Database.Models;
using Generator.LiturgieInterpretator.Models;
using mppt.Connect;
using mppt.LiedPresentator;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static mppt.RegelVerwerking.TekstVerdelerBijbeltekst;

namespace mppt.RegelVerwerking
{
    /// <summary>
    /// Deze klasse implementeert de logica die nodig is om de aangeleverde content (liturgie) voor een
    /// bijbeltekst slide te combineren met de bijbeltekst template
    /// </summary>
    class VerwerkerBijbeltekst : IVerwerkFactory
    {
        public IVerwerk Init(IMppApplication metApplicatie, IMppPresentatie toevoegenAanPresentatie, IMppFactory metFactory, ILiedFormatter gebruikLiedFormatter, IBuilderBuildSettings buildSettings,
                IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, IEnumerable<ISlideOpbouw> volledigeLiturgieOpVolgorde, ILengteBerekenaar lengteBerekenaar)
        {
            return new Verwerker(metApplicatie, toevoegenAanPresentatie, metFactory, gebruikLiedFormatter, buildSettings, buildDefaults, dependentFileList, volledigeLiturgieOpVolgorde, lengteBerekenaar);
        }

        private class Verwerker : VerwerkBase, IVerwerk
        {
            private ILengteBerekenaar _lengteBerekenaar { get; }
            private int _slidesGemist = 0;

            public Verwerker(IMppApplication metApplicatie, IMppPresentatie toevoegenAanPresentatie, IMppFactory metFactory, ILiedFormatter gebruikLiedFormatter, IBuilderBuildSettings buildSettings,
                IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, IEnumerable<ISlideOpbouw> volledigeLiturgieOpVolgorde, ILengteBerekenaar lengteBerekenaar)
                : base(metApplicatie, toevoegenAanPresentatie, metFactory, gebruikLiedFormatter, buildSettings, buildDefaults, dependentFileList, volledigeLiturgieOpVolgorde)
            {
                _lengteBerekenaar = lengteBerekenaar;
            }

            public IVerwerkResultaat Verwerk(ISlideInhoud regel, IEnumerable<ISlideOpbouw> volgenden, CancellationToken token)
            {
                InvullenTekstOpTemplate(regel, volgenden, token);

                return new VerwerkResultaat()
                {
                    SlidesGemist = _slidesGemist
                };
            }

            private void InvullenTekstOpTemplate(ISlideInhoud regel, IEnumerable<ISlideOpbouw> volgenden, CancellationToken token)
            {
                var tekstPerSlide = TekstVerdelerBijbeltekst.OpdelenPerSlide(TekstOpknippen(regel.Content), _buildDefaults.RegelsPerBijbeltekstSlide, _lengteBerekenaar, _buildDefaults.VersOnderbrekingOverSlidesHeen);

                //zolang er nog iets is in te voegen in sheets
                foreach (var tekst in tekstPerSlide)
                {
                    if (token.IsCancellationRequested)
                        return;

                    //regel de template om de bijbeltekst op af te beelden
                    var presentatie = OpenPps(_dependentFileList.FullTemplateBijbeltekst);
                    var slide = presentatie.EersteSlide();  //alleen eerste slide gebruiken we
                                                            //voor elk object op de slides (we zoeken naar de tekst die vervangen moet worden in de template)
                    foreach (var shape in slide.Shapes().Where(s => s is IMppShapeTextbox).Cast<IMppShapeTextbox>())
                    {
                        var tagReplacementResult = ProcessForTagReplacement(shape.Text,
                            additionalSearchForTagReplacement: (s) => {
                                switch (s)
                                {
                                    case "liturgieregel":
                                        return new SearchForTagReplacementResult(_liedFormatter.Huidig(regel, null, _buildDefaults.VerkortVerzenBijVolledigeContent).Display);
                                    case "inhoud":
                                        return new SearchForTagReplacementResult(tekst.ToString());
                                    case "volgende":
                                        //we moeten dan wel al op de laatste slide zitten ('InvullenVolgende' is wel al intelligent maar in het geval van 1
                                        //lange tekst over meerdere dia's kan 'InvullenVolgende' niet de juiste keuze maken)
                                        var display = tekstPerSlide.Last() == tekst ? _liedFormatter.Volgende(volgenden, 0, _buildDefaults.VerkortVerzenBijVolledigeContent) : null;
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

            private class VerwerkResultaat : IVerwerkResultaat
            {
                public int SlidesGemist { get; set; }
            }
        }
    }
}
