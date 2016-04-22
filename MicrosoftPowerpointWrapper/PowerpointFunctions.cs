﻿// Copyright 2016 door Remco Veurink en Erik de Roos
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ILiturgieDatabase;
using ISlideBuilder;
using Tools;
using mppt.Connect;
using mppt.LiedPresentator;

namespace mppt
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Zit hard op het file systeem! (powerpoint heeft geen ondersteuning voor streams)</remarks>
    public class PowerpointFunctions : IBuilder
    {
        private IMppFactory _mppFactory;
        private ILiedFormatter _liedFormatter;

        private IMppApplication _applicatie;
        private IMppPresentatie _presentatie;
        private int _slidesGemist = 0;
        private bool _stop;

        private const string NieuweSlideAanduiding = "#";

        private IEnumerable<ILiturgieRegel> _liturgie = new List<ILiturgieRegel>();
        private string _voorganger;
        private string _collecte1;
        private string _collecte2;
        private string _lezen;
        private string _tekst;
        private IBuilderBuildDefaults _buildDefaults;
        private IBuilderDependendFiles _dependentFileList;
        private string _opslaanAls;

        public Action<int, int, int> Voortgang { get; set; }
        public Action<Status, string, int?> StatusWijziging { get; set; }

        public PowerpointFunctions(IMppFactory mppFactory, ILiedFormatter liedFormatter)
        {
            _mppFactory = mppFactory;
            _liedFormatter = liedFormatter;
        }

        public void PreparePresentation(IEnumerable<ILiturgieRegel> liturgie, IBuilderBuildSettings buildSettings, IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, string opslaanAls)
        {
            _liturgie = liturgie;
            _voorganger = buildSettings.Voorganger;
            _collecte1 = buildSettings.Collecte1;
            _collecte2 = buildSettings.Collecte2;
            _lezen = buildSettings.Lezen;
            _tekst = buildSettings.Tekst;
            _buildDefaults = buildDefaults;
            _dependentFileList = dependentFileList;
            _opslaanAls = opslaanAls;
        }

        /// <summary>
        /// Genereer een presentatie aan de hand van meegegeven Liturgie en Template voor de Liederen
        /// </summary>
        public void GeneratePresentation()
        {
            StatusWijziging?.Invoke(Status.Gestart, null, null);

            // Hier pas COM calls want dit is de juiste thread
            _applicatie = _mppFactory.GetApplication();
            //Creeer een nieuwe lege presentatie volgens de template thema (toon scherm zodat bij fout nog iets te zien is)
            _presentatie = _applicatie.Open(_dependentFileList.FullTemplateTheme, metWindow: true);
            //Minimaliseer scherm
            _applicatie.MinimizeInterface();

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
                        // TODO bij regel type bijbeltekst eigen template vuller die teksten met nummering plaatst

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
                _presentatie.OpslaanAls(_opslaanAls);
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
                var presentatie = OpenPps(_dependentFileList.FullTemplateLied);
                var slide = presentatie.EersteSlide();  //alleen eerste slide gebruiken we
                //voor elk object op de slides (we zoeken naar de tekst die vervangen moet worden in de template)
                foreach (var shape in slide.Shapes().Where(s => s is IMppShapeTextbox).Cast<IMppShapeTextbox>())
                {
                    var text = shape.Text;
                    //als de template de tekst bevat "Liturgieregel" moet daar de liturgieregel komen
                    if (text.Equals("<Liturgieregel>"))
                        shape.Text = _liedFormatter.Huidig(regel, inhoud).Display;
                    //als de template de tekst bevat "Inhoud" moet daar de inhoud van het vers komen
                    else if (text.Equals("<Inhoud>"))
                        shape.Text = tekst;
                    //als de template de tekst bevat "Volgende" moet daar de Liturgieregel van de volgende sheet komen
                    else if (text.Equals("<Volgende>"))
                    {
                        //we moeten dan wel al op de laatste slide zitten ('InvullenVolgende' is wel al intelligent maar in het geval van 1
                        //lange tekst over meerdere dia's kan 'InvullenVolgende' niet de juiste keuze maken)
                        var display = IsLaatsteSlide(tekstOmTeRenderenLijst, tekst, regel, inhoud) ? _liedFormatter.Volgende(volgende) : null;
                        shape.Text = display != null ? $"{_buildDefaults.LabelVolgende} {display.Display}" : string.Empty;
                    }
                }
                //voeg slide in in het grote geheel
                _slidesGemist += _presentatie.SlidesKopieNaarPresentatie(new List<IMppSlide> { slide });
                //sluit de template weer af
                presentatie.Dispose();
            }
        }

        private void ToevoegenSlides(ILiturgieRegel regel, ILiturgieContent inhoud, ILiturgieRegel volgende)
        {
            //open de presentatie met de sheets erin
            var presentatie = OpenPps(inhoud.Inhoud);
            //voor elke slide in de presentatie
            var slides = presentatie.AlleSlides().ToList();
            foreach (var shape in slides.SelectMany(s => s.Shapes()).ToList())  
            {
                var textbox = shape as IMppShapeTextbox;
                var table = shape as IMppShapeTable;

                if (textbox != null) {
                    var text = textbox.Text;
                    //als de template de tekst bevat "Voorganger: " moet daar de Voorgangersnaam achter komen
                    if (text.Equals("<Voorganger:>"))
                        textbox.Text = _buildDefaults.LabelVoorganger + _voorganger;
                    //als de template de tekst bevat "Collecte: " moet daar de collectedoel achter komen
                    else if (text.Equals("<Collecte:>"))
                        textbox.Text = _buildDefaults.LabelCollecte + _collecte1;
                    //als de template de tekst bevat "1e Collecte: " moet daar de 1e collecte achter komen
                    else if (text.Equals("<1e Collecte:>"))
                        textbox.Text = _buildDefaults.LabelCollecte1 + _collecte1;
                    //als de template de tekst bevat "2e Collecte: " moet daar de 2e collecte achter komen
                    else if (text.Equals("<2e Collecte:>"))
                        textbox.Text = _buildDefaults.LabelCollecte2 + _collecte2;
                    //als de template de tekst bevat "Volgende" moet daar _altijd_ de Liturgieregel van de volgende sheet komen
                    //(omdat het hier handmatig bepaald wordt door degene die de slides gemaakt heeft)
                    else if (text.Equals("<Volgende>"))
                    {
                        var display = _liedFormatter.Volgende(volgende);
                        textbox.Text = display != null ? $"{_buildDefaults.LabelVolgende} {display.Display}" : string.Empty;
                    }
                    //als de template de tekst bevat "Volgende" moet daar de te lezen schriftgedeeltes komen
                    else if (text.Equals("<Lezen>"))
                        textbox.Text = _buildDefaults.LabelLezen + _lezen;
                    else if (text.Equals("<Tekst>"))
                        textbox.Text = _buildDefaults.LabelTekst + _tekst;
                    else if (text.Equals("<Tekst_Onder>"))
                        textbox.Text = _tekst;
                }
                else if (table != null) { 
                    if (table.GetTitel().Equals("<Liturgie>"))
                        VulLiturgieTabel(table, _mppFactory, _liedFormatter, _liturgie, _lezen, _tekst, _buildDefaults.LabelLiturgieLezen, _buildDefaults.LabelLiturgieTekst, _buildDefaults.LabelLiturgie);
                }
            }
            //voeg de slides in in het grote geheel
            _slidesGemist += _presentatie.SlidesKopieNaarPresentatie(slides);
            //sluit de geopende presentatie weer af
            presentatie.Dispose();
        }

        private static void VulLiturgieTabel(IMppShapeTable inTabel, IMppFactory mppFactory, ILiedFormatter liedFormatter, IEnumerable<ILiturgieRegel> liturgie, string lezen, string tekst, string instellingenLezen, string instellingenTekst, string instellingLiturgie)
        {
            var toonLijst = new List<IMppShapeTableContent>();
            toonLijst.Add(mppFactory.GetMppShapeTableContent1Column(0, instellingLiturgie, false));
            foreach (var liturgieItem in liturgie.Where(l => l.TonenInOverzicht))
            {
                var display = liedFormatter.Liturgie(liturgieItem);
                var kolom1 = display.Naam;
                var kolom2 = display.SubNaam;
                var kolom3 = display.Verzen;
                if (!string.IsNullOrWhiteSpace(kolom3))
                    kolom3 = $": {kolom3}";
                toonLijst.Add(mppFactory.GetMppShapeTableContent3Column(toonLijst.Count, kolom1, kolom2, kolom3));
            }
            if (!string.IsNullOrWhiteSpace(lezen))
                toonLijst.Add(mppFactory.GetMppShapeTableContent1Column(toonLijst.Count, $"{instellingenLezen}{lezen}", true));
            if (!string.IsNullOrWhiteSpace(tekst))
                toonLijst.Add(mppFactory.GetMppShapeTableContent1Column(toonLijst.Count, $"{instellingenTekst}{tekst}", true));
            inTabel.InsertContent(toonLijst);
        }

        /// Een 'volgende' tekst is alleen relevant om te tonen op de laatste pagina binnen een item voordat 
        /// een nieuw item komt.
        /// Je kunt er echter ook voor kiezen dat een volgende item gewoon niet aangekondigd wordt. Dat gaat
        /// via 'TonenInVolgende'.
        private static bool IsLaatsteSlide(IEnumerable<string> tekstOmTeRenderen, string huidigeTekst, ILiturgieRegel regel, ILiturgieContent deel)
        {
            return tekstOmTeRenderen.Last() == huidigeTekst && regel.Content.Last() == deel;
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

        /// <summary>
        /// Open een presentatie op het meegegeven pad
        /// </summary>
        /// <param name="path">het pad waar de powerpointpresentatie kan worden gevonden</param>
        /// <returns>de powerpoint presentatie</returns>
        private IMppPresentatie OpenPps(string path)
        {
            //controleer voor het openen van de presentatie op het meegegeven path of de presentatie bestaat
            return File.Exists(path) ? _applicatie.Open(path, metWindow: false) : null;
        }

        private void SluitAlles()
        {
            _presentatie?.Dispose();
            _applicatie?.Dispose();
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