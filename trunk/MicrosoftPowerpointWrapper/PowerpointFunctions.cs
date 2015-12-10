﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Core;
using IDatabase;
using ISettings;
using ISlideBuilder;

namespace mppt
{
    class PowerpointFunctions : IBuilder
    {
        private Microsoft.Office.Interop.PowerPoint.Application _applicatie;
        private _Presentation _presentatie;
        private CustomLayout _layout;
        private int _slideteller = 1;
        private bool _stop = false;

        private const string NieuweSlideAanduiding = "#";

        private IEnumerable<ILiturgieZoekresultaat> _liturgie = new List<ILiturgieZoekresultaat>();
        private string _voorganger;
        private string _collecte1;
        private string _collecte2;
        private string _lezen;
        private string _tekst;
        private IInstellingen _instellingen;
        private string _opslaanAls;

        public Action<int, int, int> Voortgang { get; set; }
        public Action<Status, string> StatusWijziging { get; set; }

        public PowerpointFunctions() { }

        public void PreparePresentation(IEnumerable<ILiturgieZoekresultaat> liturgie, string Voorganger, string Collecte1, string Collecte2, string Lezen, string Tekst, IInstellingen gebruikInstellingen, string opslaanAls)
        {
            _liturgie = liturgie;
            _voorganger = Voorganger;
            _collecte1 = Collecte1;
            _collecte2 = Collecte2;
            _lezen = Lezen;
            _tekst = Tekst;
            _instellingen = gebruikInstellingen;
            _opslaanAls = opslaanAls;
            _slideteller = 1;
            // Hier GEEN COM calls want dit kan nog in n andere thread zijn
        }

        /// <summary>
        /// Genereer een presentatie aan de hand van meegegeven Liturgie en Template voor de Liederen
        /// </summary>
        /// <param name="Liturgie">Liturgie die de indeling en inhoud van de gegenereerde presentatie bepaald</param>
        public void GeneratePresentation()
        {
            if (StatusWijziging != null)
                StatusWijziging.Invoke(Status.Gestart, null);

            //Creeer een nieuwe lege presentatie volgens een bepaald thema
            _applicatie = new Microsoft.Office.Interop.PowerPoint.Application();
            _applicatie.Visible = MsoTriState.msoTrue;
            var presSet = _applicatie.Presentations;
            _presentatie = presSet.Open(_instellingen.FullTemplatetheme, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoTrue);
            //sla het thema op, zodat dat in iedere nieuwe slide kan worden meegenomen
            _layout = _presentatie.SlideMaster.CustomLayouts[PpSlideLayout.ppLayoutTitle];
            //minimaliseer powerpoint
            _applicatie.WindowState = PpWindowState.ppWindowMinimized;

            try
            {
                // Lijst maken zodat volgorde bekend is
                var lijst = _liturgie.ToList();

                //voor elke regel in de liturgie moeten sheets worden gemaakt
                foreach (var regel in lijst)
                {
                    var huidigeItemIndex = lijst.IndexOf(regel);
                    var volgendeIndex = huidigeItemIndex + 1;
                    var volgende = volgendeIndex < lijst.Count ? lijst[volgendeIndex] : null;

                    foreach (var inhoud in regel.Resultaten)
                    {
                        if (inhoud.InhoudType == InhoudType.Tekst)
                            InvullenTekst(regel, inhoud, volgende);
                        else
                            InvullenSlide(regel, inhoud, volgende);
                        if (_stop)
                            break;
                    }
                    if (Voortgang != null)
                        Voortgang.Invoke(0, _liturgie.Count(), volgendeIndex);
                    if (_stop)
                        break;
                }

                //sla de presentatie op
                _presentatie.SaveAs(_opslaanAls);
                if (StatusWijziging != null)
                    StatusWijziging.Invoke(Status.StopGoed, null);
            }
            catch (Exception ex)
            {
                //FoutmeldingSchrijver.Log(ex.ToString());
                if (StatusWijziging != null)
                    StatusWijziging.Invoke(Status.StopFout, ex.ToString());
            }
            SluitAlles();
        }

        public void Stop()
        {
            _stop = true;
        }

        private void InvullenTekst(ILiturgieZoekresultaat regel, ILiturgieZoekresultaatDeel inhoud, ILiturgieZoekresultaat volgende)
        {
            var tekstOmTeRenderen = inhoud.Inhoud;
            //zolang er nog iets is in te voegen in sheets
            while (!string.IsNullOrWhiteSpace(tekstOmTeRenderen))
            {
                //regel de template om het lied op af te beelden
                var presentatie = OpenPPS(_instellingen.FullTemplateliederen);
                //voor elke slide in de presentatie(in principe moet dit er 1 zijn)
                foreach (Slide slide in presentatie.Slides)
                {
                    //voor elk object op de slides (we zoeken naar de tekst die vervangen moet worden in de template)
                    foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                    {
                        //als de shape gelijk is aan een textbox bevat het dus tekst
                        if (shape.Type == MsoShapeType.msoTextBox)
                        {
                            var text = shape.TextFrame.TextRange.Text;
                            //als de template de tekst bevat "Liturgieregel" moet daar de liturgieregel komen
                            if (text.Equals("<Liturgieregel>"))
                                shape.TextFrame.TextRange.Text = InvullenLiturgieRegel(regel, inhoud);
                            //als de template de tekst bevat "Inhoud" moet daar de inhoud van het vers komen
                            else if (text.Equals("<Inhoud>"))
                            {
                                // plaats zo veel mogelijk tekst op de slide totdat het niet meer past, krijg de restjes terug
                                var uitzoeken = InvullenLiedTekst(tekstOmTeRenderen);
                                shape.TextFrame.TextRange.Text = uitzoeken.Invullen;
                                tekstOmTeRenderen = uitzoeken.Over;
                            }
                            //als de template de tekst bevat "Volgende" moet daar de Liturgieregel van de volgende sheet komen
                            else if (text.Equals("<Volgende>"))
                                shape.TextFrame.TextRange.Text = InvullenVolgende(regel, inhoud, volgende);
                        }
                    }
                }
                //voeg slide in in het grote geheel
                VoegSlideinPresentatiein(presentatie.Slides);
                //sluit de template weer af
                presentatie.Close();
            }
        }

        private void InvullenSlide(ILiturgieZoekresultaat regel, ILiturgieZoekresultaatDeel inhoud, ILiturgieZoekresultaat volgende)
        {
            //open de presentatie met de sheets erin
            var presentatie = OpenPPS(inhoud.Inhoud);
            //voor elke slide in de presentatie(in principe moet dit er 1 zijn)
            foreach (Slide slide in presentatie.Slides)
            {
                //voor elk shape in de slide (we zoeken naar de tekst of andere dingen die vervangen moet worden in de geopende sheet)
                foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                {
                    //als de shape gelijk is aan een textbox bevat het dus tekst
                    if (shape.Type == MsoShapeType.msoTextBox)
                    {
                        var text = shape.TextFrame.TextRange.Text;
                        //als de template de tekst bevat "Voorganger: " moet daar de Voorgangersnaam achter komen
                        if (text.Equals("<Voorganger:>"))
                            shape.TextFrame.TextRange.Text = _instellingen.StandaardTeksten.Voorganger + _voorganger;
                        //als de template de tekst bevat "Collecte: " moet daar de collectedoel achter komen
                        else if (text.Equals("<Collecte:>"))
                            shape.TextFrame.TextRange.Text = _instellingen.StandaardTeksten.Collecte + _collecte1;
                        //als de template de tekst bevat "1e Collecte: " moet daar de 1e collecte achter komen
                        else if (text.Equals("<1e Collecte:>"))
                            shape.TextFrame.TextRange.Text = _instellingen.StandaardTeksten.Collecte1 + _collecte1;
                        //als de template de tekst bevat "2e Collecte: " moet daar de 2e collecte achter komen
                        else if (text.Equals("<2e Collecte:>"))
                            shape.TextFrame.TextRange.Text = _instellingen.StandaardTeksten.Collecte2 + _collecte2;
                        //als de template de tekst bevat "Volgende" moet daar de Liturgieregel van de volgende sheet komen
                        else if (text.Equals("<Volgende>"))
                            shape.TextFrame.TextRange.Text = InvullenVolgende(regel, inhoud, volgende);
                        //als de template de tekst bevat "Volgende" moet daar de te lezen schriftgedeeltes komen
                        else if (text.Equals("<Lezen>"))
                            shape.TextFrame.TextRange.Text = _instellingen.StandaardTeksten.Lezen + _lezen;
                        else if (text.Equals("<Tekst>"))
                            shape.TextFrame.TextRange.Text = _instellingen.StandaardTeksten.Tekst + _tekst;
                        else if (text.Equals("<Tekst_Onder>"))
                            shape.TextFrame.TextRange.Text = _tekst;
                    }
                    else if (shape.Type == MsoShapeType.msoTable)
                    {
                        if (shape.Table.Rows[1].Cells[1].Shape.TextFrame.TextRange.Text.Equals("<Liturgie>"))
                            VulLiturgieTabel(shape.Table, _liturgie, _lezen, _tekst, _instellingen.StandaardTeksten.Liturgie);
                    }
                }
            }
            //voeg de slides in in het grote geheel
            VoegSlideinPresentatiein(presentatie.Slides);
            //sluit de geopende presentatie weer af
            presentatie.Close();
        }

        private static void VulLiturgieTabel(Table inTabel, IEnumerable<ILiturgieZoekresultaat> liturgie, string lezen, string tekst, string instellingLiturgie)
        {
            // Te tonen liturgie in lijst plaatsen zodat we de plek per index weten
            int liturgieIndex = 0;
            var teTonenLiturgie = liturgie.Where(l => l.Type != LiturgieType.EnkelZonderDeel).ToList();

            var lezengehad = false;
            var tekstgehad = false;
            var deleterows = new List<Row>();
            for (int index = 1; index <= inTabel.Rows.Count; index++)
            {
                if (!inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text.Equals("<Liturgie>"))
                {
                    var liturgiegevonden = liturgieIndex < teTonenLiturgie.Count;
                    if (liturgiegevonden)
                    {
                        var toonItem = teTonenLiturgie[liturgieIndex];
                        inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text = toonItem.VirtueleBenaming;
                        if (toonItem.Type != LiturgieType.EnkelZonderDeel)
                        {
                            inTabel.Rows[index].Cells[2].Shape.TextFrame.TextRange.Text = toonItem.DeelBenaming;
                            if (toonItem.Type == LiturgieType.MeerMetDeel)
                                inTabel.Rows[index].Cells[3].Shape.TextFrame.TextRange.Text = ":" + LiedVerzen(toonItem.Resultaten);
                        }
                        liturgieIndex++;
                    }
                    if (!liturgiegevonden)
                    {
                        inTabel.Rows[index].Cells[1].Merge(inTabel.Rows[index].Cells[2]);
                        if (inTabel.Rows[index].Cells.Count >= 3)
                            inTabel.Rows[index].Cells[2].Merge(inTabel.Rows[index].Cells[3]);

                        //volgorde voor het liturgiebord is
                        //liederen
                        //lezen
                        //tekst
                        if (!lezengehad)
                        {
                            if (!string.IsNullOrWhiteSpace(lezen))
                            {
                                inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text = "L ";
                                inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text += lezen;
                                inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.ParagraphFormat.Alignment = PpParagraphAlignment.ppAlignLeft;
                            }
                            else {
                                inTabel.Rows[index].Delete();
                                index--;
                            }
                            lezengehad = true;
                        }
                        else if (!tekstgehad)
                        {
                            if (!string.IsNullOrWhiteSpace(tekst))
                            {
                                inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text = "T ";
                                inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text += tekst;
                                inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.ParagraphFormat.Alignment = PpParagraphAlignment.ppAlignLeft;
                            }
                            else {
                                inTabel.Rows[index].Delete();
                                index--;
                            }
                            tekstgehad = true;
                        }
                        else {
                            inTabel.Rows[index].Delete();
                            index--;
                        }
                    }
                }
                else
                    inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text = instellingLiturgie;
            }
        }

        private string InvullenVolgende(ILiturgieZoekresultaat regel, ILiturgieZoekresultaatDeel deel, ILiturgieZoekresultaat volgende)
        {
            // Alleen volgende tonen als we op het laatste item zitten en als volgende er is
            var tonen = regel.Resultaten.Last() == deel && volgende != null;
            // Check of de volgende 'blanco' heet, want dan tonen we m niet
            tonen = tonen && string.Compare(volgende.VirtueleBenaming, "Blanco", true) != 0;
            if (tonen)
                return string.Format("{0} {1}", _instellingen.StandaardTeksten.Volgende, LiedNaam(volgende));
            return string.Empty;
        }

        private SlideVuller InvullenLiedTekst(string tempinhoud)
        {
            var returnValue = new SlideVuller();
            var regels = tempinhoud.Split(new[] { "\r\n" }, StringSplitOptions.None);

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
              .Where(r => r.Index >= beginIndex && (r.Index - beginIndex) < _instellingen.Regelsperslide && r.Regel != NieuweSlideAanduiding)
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

        private static string InvullenLiturgieRegel(ILiturgieZoekresultaat regel, ILiturgieZoekresultaatDeel vanafDeel)
        {
            return LiedNaam(regel, vanafDeelHint: vanafDeel);
        }

        private static string LiedNaam(ILiturgieZoekresultaat regel, ILiturgieZoekresultaatDeel vanafDeelHint = null)
        {
            if (regel.Type == LiturgieType.EnkelZonderDeel)
                return regel.VirtueleBenaming;
            else if (regel.Type == LiturgieType.EnkelMetDeel)
                return string.Format("{0} {1}", regel.VirtueleBenaming, regel.DeelBenaming);
            var vanafDeel = vanafDeelHint ?? regel.Resultaten.FirstOrDefault();  // Bij een deel hint tonen we alleen nog de huidige en komende versen
            var gebruikDeelRegels = regel.Resultaten.SkipWhile(r => r != vanafDeel);
            return string.Format("{0} {1}: {2}", regel.VirtueleBenaming, regel.DeelBenaming, LiedVerzen(gebruikDeelRegels));
        }
        private static string LiedVerzen(IEnumerable<ILiturgieZoekresultaatDeel> vanDelen)
        {
            return string.Join(",", vanDelen.Select(r => " " + r.Nummer)).TrimEnd(new char[] { ',' });
        }

        /// <summary>
        /// Voeg een slide in in de hoofdpresentatie op de volgende positie (hoofdpresentatie werd aangemaakt bij het maken van deze klasse)
        /// </summary>
        /// <param name="slides">de slide die ingevoegd moet worden (voorwaarde is hierbij dat de presentatie waarvan de slide onderdeel is nog wel geopend is)</param>
        private void VoegSlideinPresentatiein(Slides slides)
        {
            foreach (Slide slide in slides)
            {
                //dit gedeelte is om het probleem van de eerste slide die al bestaat op te lossen voor alle andere gevallen maken we gewoon een nieuwe slide aan
                Slide voeginslide;
                if (_slideteller == 1)
                    voeginslide = _presentatie.Slides[_slideteller];
                else
                    voeginslide = _presentatie.Slides.AddSlide(_slideteller, _layout);

                //verwijder alle standaard toegevoegde dingen
                while (voeginslide.Shapes.Count > 0)
                {
                    voeginslide.Shapes[1].Delete();
                }
                //voeg de dingen van de template toe
                foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                {
                    try
                    {
                        shape.Copy();
                        voeginslide.Shapes.Paste();
                    }
                    catch (Exception) { }
                }
                _slideteller++;
            }
        }

        /// <summary>
        /// Open een presentatie op het meegegeven pad
        /// </summary>
        /// <param name="path">het pad waar de powerpointpresentatie kan worden gevonden</param>
        /// <returns>de powerpoint presentatie</returns>
        private _Presentation OpenPPS(string path)
        {
            //controleer voor het openen van de presentatie op het meegegeven path of de presentatie bestaat
            if (File.Exists(path))
                return _applicatie.Presentations.Open(path, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoFalse);
            return null;
        }

        private void SluitAlles()
        {
            _layout = null;
            if (_presentatie != null)
                _presentatie.Close();
            _presentatie = null;
            if (_applicatie != null)
                _applicatie.Quit();
            _applicatie = null;
        }
        public void Dispose()
        {
            SluitAlles();
        }

        private class SlideVuller
        {
            public string Invullen { get; set; }
            public string Over { get; set; }
        }
    }
}