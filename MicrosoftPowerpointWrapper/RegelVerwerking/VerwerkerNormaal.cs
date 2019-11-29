﻿// Copyright 2019 Erik de Roos
// Van Remco Veurink is het idee van 'liturgie regels' die zich vertalen naar 'templates' met vervangteksten.
// Van Remco Veurink is het idee van een bestandsdatabase waarvan liedteksten op een template ingevuld worden.
using Generator.Database.Models;
using Generator.LiturgieInterpretator.Models;
using Generator.Tools;
using mppt.Connect;
using mppt.LiedPresentator;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace mppt.RegelVerwerking
{
    /// <summary>
    /// Deze klasse implementeert de logica die nodig is om de aangeleverde content (liturgie) voor een
    /// niet-bijbeltekst slide te combineren met slides. Dat kan een lied template slide zijn maar ook
    /// een normale slide waarop wat template teksten vervangen moeten worden.
    /// </summary>
    class VerwerkerNormaal : IVerwerkFactory
    {
        public IVerwerk Init(IMppApplication metApplicatie, IMppPresentatie toevoegenAanPresentatie, IMppFactory metFactory, ILiedFormatter gebruikLiedFormatter, IBuilderBuildSettings buildSettings,
                IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, IEnumerable<ISlideOpbouw> volledigeLiturgieOpVolgorde, ILengteBerekenaar lengteBerekenaar)
        {
            return new Verwerker(metApplicatie, toevoegenAanPresentatie, metFactory, gebruikLiedFormatter, buildSettings, buildDefaults, dependentFileList, volledigeLiturgieOpVolgorde);
        }

        private class Verwerker : VerwerkBase, IVerwerk
        {
            private const string NieuweSlideAanduiding = "#";

            private int _slidesGemist = 0;

            public Verwerker(IMppApplication metApplicatie, IMppPresentatie toevoegenAanPresentatie, IMppFactory metFactory, ILiedFormatter gebruikLiedFormatter, IBuilderBuildSettings buildSettings,
                IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, IEnumerable<ISlideOpbouw> volledigeLiturgieOpVolgorde) 
                : base (metApplicatie, toevoegenAanPresentatie, metFactory, gebruikLiedFormatter, buildSettings, buildDefaults, dependentFileList, volledigeLiturgieOpVolgorde)
            {
            }

            public IVerwerkResultaat Verwerk(ISlideInhoud regel, IEnumerable<ISlideOpbouw> volgenden, CancellationToken token)
            {
                // Per onderdeel in de regel moet een sheet komen
                foreach (var inhoud in regel.Content)
                {
                    if (token.IsCancellationRequested)
                        break;
                    if (inhoud.InhoudType == InhoudType.Tekst)
                        InvullenTekstOpTemplate(regel, inhoud, volgenden, token);
                    else
                        ToevoegenSlides(inhoud, volgenden, token);
                }

                return new VerwerkResultaat()
                {
                    SlidesGemist = _slidesGemist
                };
            }

            /// <summary>
            /// Lied in template plaatsen
            /// </summary>
            private void InvullenTekstOpTemplate(ISlideInhoud regel, ILiturgieContent inhoud, IEnumerable<ISlideOpbouw> volgenden, CancellationToken token)
            {
                var tekstOmTeRenderen = inhoud.Inhoud;
                var tekstOmTeRenderenLijst = new List<string>();
                // knip de te renderen tekst in stukken (zodat we van tevoren het aantal weten)
                while (!string.IsNullOrWhiteSpace(tekstOmTeRenderen))
                {
                    if (token.IsCancellationRequested)
                        return;

                    // plaats zo veel mogelijk tekst op de slide totdat het niet meer past, krijg de restjes terug
                    var uitzoeken = InvullenLiedTekst(tekstOmTeRenderen);
                    tekstOmTeRenderenLijst.Add(uitzoeken.Invullen);
                    tekstOmTeRenderen = uitzoeken.Over;
                }

                //zolang er nog iets is in te voegen in sheets
                foreach (var tekst in tekstOmTeRenderenLijst)
                {
                    if (token.IsCancellationRequested)
                        return;

                    //regel de template om het lied op af te beelden
                    var presentatie = OpenPps(_dependentFileList.FullTemplateLied);
                    var slide = presentatie.EersteSlide();  //alleen eerste slide gebruiken we
                                                            //voor elk object op de slides (we zoeken naar de tekst die vervangen moet worden in de template)
                    foreach (var shape in slide.Shapes().Where(s => s is IMppShapeTextbox).Cast<IMppShapeTextbox>())
                    {
                        var tagReplacementResult = ProcessForTagReplacement(shape.Text, 
                            additionalSearchForTagReplacement: (s) => {
                                switch (s)
                                {
                                    case "liturgieregel":
                                        return new SearchForTagReplacementResult(_liedFormatter.Huidig(regel, inhoud).Display);
                                    case "inhoud":
                                        return new SearchForTagReplacementResult(tekst);
                                    case "volgende":
                                        //we moeten dan wel al op de laatste slide zitten ('InvullenVolgende' is wel al intelligent maar in het geval van 1
                                        //lange tekst over meerdere dia's kan 'InvullenVolgende' niet de juiste keuze maken)
                                        var display = IsLaatsteSlide(tekstOmTeRenderenLijst, tekst, regel, inhoud) ? _liedFormatter.Volgende(volgenden) : null;
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

            /// <summary>
            /// Algemene slide waarop we alleen template teksten moeten vervangen
            /// </summary>
            private void ToevoegenSlides(ILiturgieContent inhoud, IEnumerable<ISlideOpbouw> volgenden, CancellationToken token)
            {
                //open de presentatie met de sheets erin
                var presentatie = OpenPps(inhoud.Inhoud);
                //voor elke slide in de presentatie
                var slides = presentatie.AlleSlides().ToList();
                foreach (var shape in slides.SelectMany(s => s.Shapes()).ToList())
                {
                    if (token.IsCancellationRequested)
                        return;

                    var textbox = shape as IMppShapeTextbox;
                    var table = shape as IMppShapeTable;

                    if (textbox != null)
                    {
                        var tagReplacementResult = ProcessForTagReplacement(textbox.Text,
                            additionalSearchForTagReplacement: (s) => {
                                LiedFormatResult display;
                                switch (s)
                                {
                                    case "volgende":
                                        //als de template de tekst bevat "Volgende" moet daar de Liturgieregel van de volgende sheet komen
                                        display = _liedFormatter.Volgende(volgenden);
                                        return new SearchForTagReplacementResult(display != null ? $"{_buildDefaults.LabelVolgende} {display.Display}" : string.Empty);
                                    case "volgende_kort":
                                        //verkorte versie van "Volgende"
                                        display = _liedFormatter.Volgende(volgenden);
                                        return new SearchForTagReplacementResult(display != null ? display.Display : string.Empty);
                                }
                                if (s.StartsWith("volgende_over_"))
                                {
                                    var aantalOverslaanStr = s.Substring("volgende_over_".Length, s.Length - "volgende_over_".Length);
                                    int aantalOverslaan = 0;
                                    if (int.TryParse(aantalOverslaanStr, out aantalOverslaan))
                                    {
                                        display = _liedFormatter.Volgende(volgenden, aantalOverslaan);
                                        return new SearchForTagReplacementResult(display != null ? $"{_buildDefaults.LabelVolgende} {display.Display}" : string.Empty);
                                    }
                                    else
                                        return new SearchForTagReplacementResult(string.Empty);
                                }
                                return SearchForTagReplacementResult.Unresolved;
                            });
                        if (tagReplacementResult.TagsReplaced)
                            textbox.Text = tagReplacementResult.NewValue;
                    }
                    else if (table != null)
                    {
                        if (table.GetTitelFromFirstRowCell().Equals("<Liturgie>"))
                            VulLiturgieTabel(table, _mppFactory, _liedFormatter, _liturgie, _buildSettings.Lezen, _buildSettings.Tekst, _buildDefaults.LabelLiturgieLezen, _buildDefaults.LabelLiturgieTekst, _buildDefaults.LabelLiturgie);
                    }
                }
                //voeg de slides in in het grote geheel
                _slidesGemist += _presentatie.SlidesKopieNaarPresentatie(slides);
                //sluit de geopende presentatie weer af
                presentatie.Dispose();
            }

            private static void VulLiturgieTabel(IMppShapeTable inTabel, IMppFactory mppFactory, ILiedFormatter liedFormatter, IEnumerable<ISlideOpbouw> liturgie, string lezen, string tekst, string instellingenLezen, string instellingenTekst, string instellingLiturgie)
            {
                var toonLijst = new List<IMppShapeTableContent>();
                toonLijst.Add(mppFactory.GetMppShapeTableContent1Column(0, instellingLiturgie, false));
                foreach (var liturgieItem in liturgie.Where(l => l.TonenInOverzicht))
                {
                    var display = liedFormatter.Liturgie(liturgieItem);
                    var kolom1 = display.Naam;
                    if (liturgieItem.VerwerkenAlsType == VerwerkingType.bijbeltekst)
                        kolom1 = $"{instellingenLezen}{kolom1}";
                    var kolom2 = display.SubNaam;
                    var kolom3 = display.Verzen;
                    if (!string.IsNullOrWhiteSpace(kolom3))
                        kolom3 = $": {kolom3}";
                    if (!string.IsNullOrWhiteSpace(kolom2) || !string.IsNullOrWhiteSpace(kolom3))
                        toonLijst.Add(mppFactory.GetMppShapeTableContent3Column(toonLijst.Count, kolom1, kolom2, kolom3));
                    else
                        toonLijst.Add(mppFactory.GetMppShapeTableContent1Column(toonLijst.Count, kolom1, true));
                }
                if (!string.IsNullOrWhiteSpace(lezen))
                    toonLijst.Add(mppFactory.GetMppShapeTableContent1Column(toonLijst.Count, $"{instellingenLezen}{lezen}", true));
                if (!string.IsNullOrWhiteSpace(tekst))
                    toonLijst.Add(mppFactory.GetMppShapeTableContent1Column(toonLijst.Count, $"{instellingenTekst}{tekst}", true));
                inTabel.SetRowsContent(toonLijst);
                inTabel.TrimRows();
            }


            private SlideVuller InvullenLiedTekst(string tempinhoud)
            {
                var returnValue = new SlideVuller();
                var regels = SplitRegels.Split(tempinhoud);

                // We moeten goed opletten bij het invullen van een liedtekst op een slide:
                // -Het mogen niet te veel regels zijn (instellingen beperken dat)
                // -We willen niet beginregels verspillen aan witruimte
                // -Tussenwitregels willen we wel respecteren
                // -Als we afbreken in een aaneengesloten stuk tekst moeten we kijken of we toch niet
                //  naar een voorgaande witruimte kunnen afbreken

                // kijk waar we gaan beginnen. Sla begin witregels over
                var beginIndex = regels.Select((r, i) => new { Regel = r, Index = i })
                  .Where(r => !SkipRegel(r.Regel))
                  .Select(r => (int?)r.Index)  // nullable int zodat als we niets vinden we dat weten
                  .FirstOrDefault();
                if (!beginIndex.HasValue)
                    return returnValue;  // er is niets over

                // kijk waar we eindigen als we instellinge-aantal tellen vanaf ons startpunt
                var eindIndex = regels.Select((r, i) => new { Regel = r, Index = i })
                  .Where(r => r.Index >= beginIndex && (r.Index - beginIndex) < _buildDefaults.RegelsPerLiedSlide && r.Regel != NieuweSlideAanduiding)
                  .Select(r => r.Index)  // eindindex is er altijd als er een begin is
                  .LastOrDefault();

                var optimaliseerEindIndex = eindIndex;
                // Kijk of we niet beter op een eerdere witregel kunnen stoppen
                if (!SkipRegel(regels[optimaliseerEindIndex]) && regels.Length != optimaliseerEindIndex + 1)
                {
                    var tryOptimaliseerEindIndex = regels.Select((r, i) => new { Regel = r, Index = i })
                      .Skip(beginIndex.Value).Take(optimaliseerEindIndex + 1 - beginIndex.Value)
                      .OrderByDescending(r => r.Index)
                      .Where(r => SkipRegel(r.Regel))
                      .Select(r => (int?)r.Index)
                      .FirstOrDefault();
                    if (tryOptimaliseerEindIndex.HasValue && tryOptimaliseerEindIndex.Value > beginIndex.Value)
                        optimaliseerEindIndex = tryOptimaliseerEindIndex.Value;
                }

                // haal regels van het vers op
                var insertLines = regels
                  .Skip(beginIndex.Value).Take(optimaliseerEindIndex + 1 - beginIndex.Value)
                  .Select(r => (r ?? "").Trim()).ToList();

                // plaats de in te voegen regels in het tekstveld (geen enter aan het einde)
                returnValue.Invullen = string.Join("", insertLines.Select((l, i) => l + (i + 1 == insertLines.Count ? "" : "\r\n")));

                var overStart = optimaliseerEindIndex + 1;
                if (overStart >= regels.Length)
                    return returnValue;
                if (regels[overStart] == NieuweSlideAanduiding)
                    overStart++;
                var overLines = regels.Skip(overStart).ToList();

                // afbreek teken tonen alleen als een vers doormidden gebroken is
                if (!SkipRegel(insertLines.Last()) && overLines.Any() && !SkipRegel(overLines.First()))
                    returnValue.Invullen += "\r\n >>";

                // Geef de resterende regels terug
                returnValue.Over = string.Join("", overLines.Select((l, i) => l + (i + 1 == overLines.Count ? "" : "\r\n")));
                return returnValue;
            }
            private static bool SkipRegel(string regel)
            {
                return string.IsNullOrWhiteSpace(regel) || regel == NieuweSlideAanduiding;
            }

            /// Een 'volgende' tekst is alleen relevant om te tonen op de laatste pagina binnen een item voordat 
            /// een nieuw item komt.
            /// Je kunt er echter ook voor kiezen dat een volgende item gewoon niet aangekondigd wordt. Dat gaat
            /// via 'TonenInVolgende'.
            protected static bool IsLaatsteSlide(IEnumerable<string> tekstOmTeRenderen, string huidigeTekst, ISlideInhoud regel, ILiturgieContent deel)
            {
                return tekstOmTeRenderen.Last() == huidigeTekst && regel.Content.Last() == deel;
            }

            private class SlideVuller
            {
                public string Invullen { get; set; }
                public string Over { get; set; }
            }

            private class VerwerkResultaat : IVerwerkResultaat
            {
                public int SlidesGemist { get; set; }
            }
        }
    }
}
