using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ILiturgieDatabase;
using ISettings;
using ISlideBuilder;
using System.Text;
using NetOffice.PowerPointApi;
using NetOffice.OfficeApi.Enums;
using NetOffice.PowerPointApi.Enums;
using Clipboard = System.Windows.Forms.Clipboard;
using Tools;

namespace mppt
{
    public class PowerpointFunctions : IBuilder
    {
        private Application _applicatie;
        private _Presentation _presentatie;
        private CustomLayout _layout;
        private int _slideteller = 1;
        private int _slidesGemist = 0;
        private bool _stop;

        private const string NieuweSlideAanduiding = "#";

        private IEnumerable<ILiturgieRegel> _liturgie = new List<ILiturgieRegel>();
        private string _voorganger;
        private string _collecte1;
        private string _collecte2;
        private string _lezen;
        private string _tekst;
        private IInstellingen _instellingen;
        private string _opslaanAls;

        public Action<int, int, int> Voortgang { get; set; }
        public Action<Status, string, int?> StatusWijziging { get; set; }

        public void PreparePresentation(IEnumerable<ILiturgieRegel> liturgie, string voorganger, string collecte1, string collecte2, string lezen, string tekst, IInstellingen gebruikInstellingen, string opslaanAls)
        {
            _liturgie = liturgie;
            _voorganger = voorganger;
            _collecte1 = collecte1;
            _collecte2 = collecte2;
            _lezen = lezen;
            _tekst = tekst;
            _instellingen = gebruikInstellingen;
            _opslaanAls = opslaanAls;
            _slideteller = 1;
            // Hier GEEN COM calls want dit kan nog in n andere thread zijn
        }

        /// <summary>
        /// Genereer een presentatie aan de hand van meegegeven Liturgie en Template voor de Liederen
        /// </summary>
        public void GeneratePresentation()
        {
            StatusWijziging?.Invoke(Status.Gestart, null, null);

            //Creeer een nieuwe lege presentatie volgens een bepaald thema
            _applicatie = new Application {Visible = MsoTriState.msoTrue};
            var presSet = _applicatie.Presentations;
            _presentatie = presSet.Open(_instellingen.FullTemplatetheme, MsoTriState.msoFalse, MsoTriState.msoTrue);
            //sla het thema op, zodat dat in iedere nieuwe slide kan worden meegenomen
            _layout = _presentatie.SlideMaster.CustomLayouts[PpSlideLayout.ppLayoutTitle];
            //minimaliseer powerpoint
            _applicatie.WindowState = PpWindowState.ppWindowMinimized;

            try
            {
                // Voor elke regel in de liturgie moeten sheets worden gemaakt (als dat mag)
                // Gebruik een list zodat we de plek weten voor de progress
                var hardeLijst = _liturgie.Where(l => l.VerwerkenAlsSlide).ToList();
                foreach (var regel in hardeLijst)
                {
                    var volgende = Volgende(_liturgie, regel);

                    // Per onderdeel in de regel moet een sheet komen
                    foreach (var inhoud in regel.Content)
                    {
                        if (inhoud.InhoudType == InhoudType.Tekst)
                            InvullenTekstOpTemplate(regel, inhoud, volgende);
                        else
                            ToevoegenSlides(regel, inhoud, volgende);
                        if (_stop)
                            break;
                    }
                    Voortgang?.Invoke(0, _liturgie.Count(), hardeLijst.IndexOf(regel) + 1);
                    if (_stop)
                        break;
                }

                //sla de presentatie op
                _presentatie.SaveAs(_opslaanAls);
                SluitAlles();
                if (_stop)
                    StatusWijziging?.Invoke(Status.StopFout, "Tussentijds gestopt door de gebruiker.", null);
                else
                    StatusWijziging?.Invoke(Status.StopGoed, null, _slidesGemist);
            }
            catch (Exception ex)
            {
                FoutmeldingSchrijver.Log(ex.ToString());
                StatusWijziging?.Invoke(Status.StopFout, ex.ToString(), null);
                SluitAlles();
            }
        }

        /// <summary>
        /// Uitzoeken wat de volgende is
        /// </summary>
        private static ILiturgieRegel Volgende(IEnumerable<ILiturgieRegel> volledigeLiturgie, ILiturgieRegel huidig)
        {
            var lijst = volledigeLiturgie.ToList();
            var huidigeItemIndex = lijst.IndexOf(huidig);
            return lijst.Skip(huidigeItemIndex + 1).FirstOrDefault();
        }


        public void Stop()
        {
            _stop = true;
        }

        private void InvullenTekstOpTemplate(ILiturgieRegel regel, ILiturgieContent inhoud, ILiturgieRegel volgende)
        {
            var tekstOmTeRenderen = inhoud.Inhoud;
            var tekstOmTeRenderenLijst = new List<string>();
            // knip de te renderen tekst in stukken (zodat we van tevoren het aantal weten)
            while (!string.IsNullOrWhiteSpace(tekstOmTeRenderen))
            {
                // plaats zo veel mogelijk tekst op de slide totdat het niet meer past, krijg de restjes terug
                var uitzoeken = InvullenLiedTekst(tekstOmTeRenderen);
                tekstOmTeRenderenLijst.Add(uitzoeken.Invullen);
                tekstOmTeRenderen = uitzoeken.Over;
            }

            //zolang er nog iets is in te voegen in sheets
            foreach(var tekst in tekstOmTeRenderenLijst)
            {
                //regel de template om het lied op af te beelden
                var presentatie = OpenPps(_instellingen.FullTemplateliederen);
                var slide = presentatie.Slides.First() as Slide;  //alleen eerste slide gebruiken we
                //voor elk object op de slides (we zoeken naar de tekst die vervangen moet worden in de template)
                foreach (var shape in slide.Shapes.Cast<Shape>().Where(s => s.Type == MsoShapeType.msoTextBox).ToList())
                    // Specifieke casts en tolists om COM problemen te minimaliseren
                {
                    var text = shape.TextFrame.TextRange.Text;
                    //als de template de tekst bevat "Liturgieregel" moet daar de liturgieregel komen
                    if (text.Equals("<Liturgieregel>"))
                        shape.TextFrame.TextRange.Text = InvullenLiturgieRegel(regel, inhoud);
                    //als de template de tekst bevat "Inhoud" moet daar de inhoud van het vers komen
                    else if (text.Equals("<Inhoud>"))
                        shape.TextFrame.TextRange.Text = tekst;
                    //als de template de tekst bevat "Volgende" moet daar de Liturgieregel van de volgende sheet komen
                    else if (text.Equals("<Volgende>"))
                    {
                        //we moeten dan wel al op de laatste slide zitten ('InvullenVolgende' is wel al intelligent maar in het geval van 1
                        //lange tekst over meerdere dia's kan 'InvullenVolgende' niet de juiste keuze maken)
                        shape.TextFrame.TextRange.Text = tekstOmTeRenderenLijst.Last() == tekst ? InvullenVolgende(regel, inhoud, volgende) : string.Empty;
                    }
                }
                //voeg slide in in het grote geheel
                SlidesKopieNaarPresentatie(new List<Slide> { slide });
                //sluit de template weer af
                presentatie.Close();
                presentatie.Dispose();
            }
        }

        private void ToevoegenSlides(ILiturgieRegel regel, ILiturgieContent inhoud, ILiturgieRegel volgende)
        {
            //open de presentatie met de sheets erin
            var presentatie = OpenPps(inhoud.Inhoud);
            //voor elke slide in de presentatie
            var slides = presentatie.Slides.Cast<Slide>().ToList();
            foreach (var shape in slides.SelectMany(s => s.Shapes.Cast<Shape>()).ToList())  
                // Specifieke casts en tolists om COM problemen te minimaliseren
            {
                //als de shape gelijk is aan een textbox bevat het dus tekst
                switch (shape.Type)
                {
                    case MsoShapeType.msoTextBox:
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
                        break;
                    case MsoShapeType.msoTable:
                        if (shape.Table.Rows[1].Cells[1].Shape.TextFrame.TextRange.Text.Equals("<Liturgie>"))
                            VulLiturgieTabel(shape.Table, _liturgie, _lezen, _tekst, _instellingen.StandaardTeksten.Liturgie);
                        break;
                }
            }
            //voeg de slides in in het grote geheel
            SlidesKopieNaarPresentatie(slides);
            //sluit de geopende presentatie weer af
            presentatie.Close();
            presentatie.Dispose();
        }

        private static void VulLiturgieTabel(Table inTabel, IEnumerable<ILiturgieRegel> liturgie, string lezen, string tekst, string instellingLiturgie)
        {
            // Te tonen liturgie in lijst plaatsen zodat we de plek per index weten
            var liturgieIndex = 0;
            var teTonenLiturgie = liturgie.Where(l => l.TonenInOverzicht).ToList();

            var lezengehad = false;
            var tekstgehad = false;
            for (var index = 1; index <= inTabel.Rows.Count; index++)
            {
                if (!inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text.Equals("<Liturgie>"))
                {
                    var liturgiegevonden = liturgieIndex < teTonenLiturgie.Count;
                    if (liturgiegevonden)
                    {
                        var toonItem = teTonenLiturgie[liturgieIndex];
                        inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text = toonItem.Display.NaamOverzicht;
                        if (!string.IsNullOrWhiteSpace(toonItem.Display.SubNaam))
                        {
                            inTabel.Rows[index].Cells[2].Shape.TextFrame.TextRange.Text = toonItem.Display.SubNaam;
                            if (!string.IsNullOrWhiteSpace(toonItem.Display.VersenDefault))
                                inTabel.Rows[index].Cells[3].Shape.TextFrame.TextRange.Text = ":" + LiedFormattering.LiedVerzen(toonItem.Display, false, vanDelen: toonItem.Content);
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

        /// Een 'volgende' tekst is alleen relevant om te tonen op de laatste pagina binnen een item voordat 
        /// een nieuw item komt.
        /// Je kunt er echter ook voor kiezen dat een volgende item gewoon niet aangekondigd wordt. Dat gaat
        /// via 'TonenInVolgende'.
        private string InvullenVolgende(ILiturgieRegel regel, ILiturgieContent deel, ILiturgieRegel volgende)
        {
            // Alleen volgende tonen als we op het laatste item zitten en als volgende er is
            if (regel.Content.Last() == deel && volgende != null && volgende.TonenInVolgende)
                return $"{_instellingen.StandaardTeksten.Volgende} {LiedFormattering.LiedNaam(volgende)}";
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

        private static string InvullenLiturgieRegel(ILiturgieRegel regel, ILiturgieContent vanafDeel)
        {
            return LiedFormattering.LiedNaam(regel, vanafDeel);
        }

        /// <summary>
        /// Voeg een slide in in de hoofdpresentatie op de volgende positie (hoofdpresentatie werd aangemaakt bij het maken van deze klasse)
        /// </summary>
        /// <param name="slides">de slide die ingevoegd moet worden (voorwaarde is hierbij dat de presentatie waarvan de slide onderdeel is nog wel geopend is)</param>
        private void SlidesKopieNaarPresentatie(IEnumerable<Slide> slides, int retryCount = 3)
        {
            foreach (var slide in slides.ToList())
            {
                if (_slideteller == 1)
                    _presentatie.Slides[1].Delete();
                slide.Copy();
                var gelukt = false;
                for (int currentTry = 1; currentTry < retryCount && gelukt == false; currentTry++)
                {
                    try
                    {
                        gelukt = ExecuteWhen(HasClipboardPowerpointSlideContent, () =>
                        {
                            _presentatie.Slides.Paste();
                            _slideteller++;
                        });
                    }
                    catch (System.Runtime.InteropServices.COMException)
                    {

                    }
                }
                if (!gelukt)
                    _slidesGemist++;
            }
        }
        private static bool ExecuteWhen(Func<bool> isTrue, Action action, int minWaitTime = 5, int maxWaitTime = 250, int waitStep = 25, int waitBeforeExecute = 5)
        {
            var waited = 0;
            if (minWaitTime > 0)
                System.Threading.Thread.Sleep(minWaitTime);
            waited += minWaitTime;
            while (waited < maxWaitTime && waitStep > 0 && !isTrue.Invoke())
            {
                System.Threading.Thread.Sleep(waitStep);
                waited += waitStep;
            }
            if (isTrue.Invoke())
            {
                if (waitBeforeExecute > 0)
                    System.Threading.Thread.Sleep(waitBeforeExecute);
                action.Invoke();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Open een presentatie op het meegegeven pad
        /// </summary>
        /// <param name="path">het pad waar de powerpointpresentatie kan worden gevonden</param>
        /// <returns>de powerpoint presentatie</returns>
        private _Presentation OpenPps(string path)
        {
            //controleer voor het openen van de presentatie op het meegegeven path of de presentatie bestaat
            return File.Exists(path) ? _applicatie.Presentations.Open(path, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoFalse) : null;
        }

        private static bool HasClipboardPowerpointSlideContent()
        {
            var data = Clipboard.GetDataObject();
            if (data == null)
                return false;
            var formats = data.GetFormats();
            return formats.Any(f => f.StartsWith("PowerPoint"));
        }

        private void SluitAlles()
        {
            _layout = null;
            try {
                _presentatie?.Close();
                _presentatie?.Dispose();
            }
            finally
            {
                _presentatie = null;
            }
            try
            {
                _applicatie?.Quit();
            }
            finally
            {
                _applicatie = null;
            }
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