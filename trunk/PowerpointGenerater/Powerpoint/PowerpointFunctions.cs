using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Core;
using PowerpointGenerater.Database;

namespace PowerpointGenerater.Powerpoint {
  class PowerpointFunctions : IDisposable {
    private Microsoft.Office.Interop.PowerPoint.Application _objApp;
    private _Presentation _objPres;
    private CustomLayout _layout;
    private int _slideteller = 1;
    private bool _stop = false;

    private const String NieuweSlideAanduiding = "#";

    private IEnumerable<ILiturgieZoekresultaat> _liturgie = new List<ILiturgieZoekresultaat>();
    private string _voorganger;
    private string _collecte1;
    private string _collecte2;
    private string _lezen;
    private string _tekst;
    private Instellingen _instellingen;

    public delegate void Voortgang(int lijstStart, int lijstEind, int bijItem);
    private Voortgang _setVoortgang;
    public delegate void StatusWijziging(Status nieuweStatus);
    private StatusWijziging _setStatus;

    public PowerpointFunctions(Voortgang voortgangDelegate, StatusWijziging statusDelegate) {
      _setVoortgang = voortgangDelegate;
      _setStatus = statusDelegate;
    }

    /// <summary>
    /// Open een presentatie op het meegegeven pad
    /// </summary>
    /// <param name="path">het pad waar de powerpointpresentatie kan worden gevonden</param>
    /// <returns>de powerpoint presentatie</returns>
    private _Presentation OpenPPS(String path) {
      //controleer voor het openen van de presentatie op het meegegeven path of de presentatie bestaat
      if (File.Exists(path)) {
        return _objApp.Presentations.Open(path,
            MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoFalse);
      }
      return null;
    }

    public void PreparePresentation(IEnumerable<ILiturgieZoekresultaat> liturgie, string Voorganger, string Collecte1, string Collecte2, string Lezen, string Tekst, Instellingen gebruikInstellingen) {
      this._liturgie = liturgie;
      this._voorganger = Voorganger;
      this._collecte1 = Collecte1;
      this._collecte2 = Collecte2;
      this._lezen = Lezen;
      this._tekst = Tekst;
      this._instellingen = gebruikInstellingen;
      //Creeer een nieuwe lege presentatie volgens een bepaald thema
      _objApp = new Microsoft.Office.Interop.PowerPoint.Application();
      _objApp.Visible = MsoTriState.msoTrue;
      var presSet = _objApp.Presentations;
      _objPres = presSet.Open(_instellingen.FullTemplatetheme,
          MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoTrue);
      //sla het thema op, zodat dat in iedere nieuwe slide kan worden meegenomen
      _layout = _objPres.SlideMaster.CustomLayouts[PpSlideLayout.ppLayoutTitle];
      //minimaliseer powerpoint
      _objApp.WindowState = PpWindowState.ppWindowMinimized;
    }

    /// <summary>
    /// Genereer een presentatie aan de hand van meegegeven Liturgie en Template voor de Liederen
    /// </summary>
    /// <param name="Liturgie">Liturgie die de indeling en inhoud van de gegenereerde presentatie bepaald</param>
    public void GeneratePresentation() {
      _setStatus.Invoke(Status.Gestart);
      try {
        // Lijst maken zodat volgorde bekend is
        var lijst = _liturgie.ToList();

        //voor elke regel in de liturgie moeten sheets worden gemaakt
        foreach (var regel in lijst) {
          var huidigeItemIndex = lijst.IndexOf(regel);
          var volgendeIndex = huidigeItemIndex + 1;
          var volgende = volgendeIndex < lijst.Count ? lijst[volgendeIndex] : null;

          foreach (var inhoud in regel.Resultaten) {
            if (inhoud.InhoudType == InhoudType.Tekst)
              InvullenTekst(regel, inhoud, volgende);
            else
              InvullenSlide(regel, inhoud, volgende);
            if (_stop)
              break;
          }
          _setVoortgang.Invoke(0, _liturgie.Count(), volgendeIndex);
          if (_stop)
            break;
        }

        //maximaliseer de presentatie ter controle voor de gebruiker
        _objApp.WindowState = PpWindowState.ppWindowMaximized;
        _setStatus.Invoke(Status.StopGoed);
      }
      catch (Exception ex) {
        using (var sw = new StreamWriter("ppgenerator.log", false)) {
          sw.WriteLine(ex.ToString());
        }
        _setStatus.Invoke(Status.StopFout);
      }
    }

    public void Stop() {
      _stop = true;
    }

    private void InvullenTekst(ILiturgieZoekresultaat regel, ILiturgieZoekresultaatDeel inhoud, ILiturgieZoekresultaat volgende) {
      var tekstOmTeRenderen = inhoud.Inhoud;
      //zolang er nog iets is in te voegen in sheets
      while (!String.IsNullOrWhiteSpace(tekstOmTeRenderen)) {
        //regel de template om het lied op af te beelden
        var presentatie = OpenPPS(_instellingen.FullTemplateliederen);
        //voor elke slide in de presentatie(in principe moet dit er 1 zijn)
        foreach (Slide slide in presentatie.Slides) {
          //voor elk object op de slides (we zoeken naar de tekst die vervangen moet worden in de template)
          foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes) {
            //als de shape gelijk is aan een textbox bevat het dus tekst
            if (shape.Type == MsoShapeType.msoTextBox) {
              //als de template de tekst bevat "Liturgieregel" moet daar de liturgieregel komen
              if (shape.TextFrame.TextRange.Text.Equals("<Liturgieregel>"))
                InvullenLiturgieRegel(regel, inhoud, shape);
              //als de template de tekst bevat "Inhoud" moet daar de inhoud van het vers komen
              if (shape.TextFrame.TextRange.Text.Equals("<Inhoud>")) {
                // plaats zo veel mogelijk tekst op de slide totdat het niet meer past, krijg de restjes terug
                tekstOmTeRenderen = InvullenLiedTekst(tekstOmTeRenderen, slide, shape);
              }
              //als de template de tekst bevat "Volgende" moet daar de Liturgieregel van de volgende sheet komen
              if (shape.TextFrame.TextRange.Text.Equals("<Volgende>"))
                InvullenVolgende(regel, inhoud, volgende, shape);
            }
          }
        }
        //voeg slide in in het grote geheel
        VoegSlideinPresentatiein(presentatie.Slides);
        //sluit de template weer af
        presentatie.Close();
      }
    }

    private void InvullenSlide(ILiturgieZoekresultaat regel, ILiturgieZoekresultaatDeel inhoud, ILiturgieZoekresultaat volgende) {
      //open de presentatie met de sheets erin
      var presentatie = OpenPPS(inhoud.Inhoud);
      //voor elke slide in de presentatie(in principe moet dit er 1 zijn)
      foreach (Slide slide in presentatie.Slides) {
        //voor elk shape in de slide (we zoeken naar de tekst of andere dingen die vervangen moet worden in de geopende sheet)
        foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes) {
          //als de shape gelijk is aan een textbox bevat het dus tekst
          if (shape.Type == MsoShapeType.msoTextBox) {
            //als de template de tekst bevat "Voorganger: " moet daar de Voorgangersnaam achter komen
            if (shape.TextFrame.TextRange.Text.Equals("<Voorganger:>")) {
              shape.TextFrame.TextRange.Text = _instellingen.StandaardTekst.Voorganger;
              shape.TextFrame.TextRange.Text += _voorganger;
            }
            //als de template de tekst bevat "Collecte: " moet daar de collectedoel achter komen
            if (shape.TextFrame.TextRange.Text.Equals("<Collecte:>")) {
              shape.TextFrame.TextRange.Text = _instellingen.StandaardTekst.Collecte;
              shape.TextFrame.TextRange.Text += _collecte1;
            }
            //als de template de tekst bevat "1e Collecte: " moet daar de 1e collecte achter komen
            if (shape.TextFrame.TextRange.Text.Equals("<1e Collecte:>")) {
              shape.TextFrame.TextRange.Text = _instellingen.StandaardTekst.Collecte1;
              shape.TextFrame.TextRange.Text += _collecte1;
            }
            //als de template de tekst bevat "2e Collecte: " moet daar de 2e collecte achter komen
            if (shape.TextFrame.TextRange.Text.Equals("<2e Collecte:>")) {
              shape.TextFrame.TextRange.Text = _instellingen.StandaardTekst.Collecte2;
              shape.TextFrame.TextRange.Text += _collecte2;
            }
            //als de template de tekst bevat "Volgende" moet daar de Liturgieregel van de volgende sheet komen
            if (shape.TextFrame.TextRange.Text.Equals("<Volgende>")) {
              InvullenVolgende(regel, inhoud, volgende, shape);
            }
            //als de template de tekst bevat "Volgende" moet daar de te lezen schriftgedeeltes komen
            if (shape.TextFrame.TextRange.Text.Equals("<Lezen>")) {
              shape.TextFrame.TextRange.Text = _instellingen.StandaardTekst.Lezen;
              shape.TextFrame.TextRange.Text += _lezen;
            }
            if (shape.TextFrame.TextRange.Text.Equals("<Tekst>")) {
              shape.TextFrame.TextRange.Text = _instellingen.StandaardTekst.Tekst;
              shape.TextFrame.TextRange.Text += _tekst;
            }
            if (shape.TextFrame.TextRange.Text.Equals("<Tekst_Onder>")) {
              shape.TextFrame.TextRange.Text = _tekst;
            }
          }
          if (shape.Type == MsoShapeType.msoTable) {
            if (shape.Table.Rows[1].Cells[1].Shape.TextFrame.TextRange.Text.Equals("<Liturgie>"))
              VulLiturgieTabel(shape.Table, _liturgie, _lezen, _tekst, _instellingen.StandaardTekst.Liturgie);
          }
        }
      }
      //voeg de slides in in het grote geheel
      VoegSlideinPresentatiein(presentatie.Slides);
      //sluit de geopende presentatie weer af
      presentatie.Close();
    }

    private static void VulLiturgieTabel(Table inTabel, IEnumerable<ILiturgieZoekresultaat> liturgie, String lezen, String tekst, String instellingLiturgie) {
      // Te tonen liturgie in lijst plaatsen zodat we de plek per index weten
      int liturgieIndex = 0;
      var teTonenLiturgie = liturgie.Where(l => l.Type != LiturgieType.EnkelZonderDeel).ToList();
      
      var lezengehad = false;
      var tekstgehad = false;
      var deleterows = new List<Row>();
      for (int index = 1; index <= inTabel.Rows.Count; index++) {
        if (!inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text.Equals("<Liturgie>")) {
          var liturgiegevonden = liturgieIndex < teTonenLiturgie.Count;
          if (liturgiegevonden) {
            var toonItem = teTonenLiturgie[liturgieIndex];
            inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text = toonItem.VirtueleBenaming;
            if (toonItem.Type != LiturgieType.EnkelZonderDeel) {
              inTabel.Rows[index].Cells[2].Shape.TextFrame.TextRange.Text = toonItem.DeelBenaming;
              if (toonItem.Type == LiturgieType.MeerMetDeel)
                inTabel.Rows[index].Cells[3].Shape.TextFrame.TextRange.Text = ":" + LiedVerzen(toonItem.Resultaten);
            }
            liturgieIndex++;
          }
          if (!liturgiegevonden) {
            inTabel.Rows[index].Cells[1].Merge(inTabel.Rows[index].Cells[2]);
            if (inTabel.Rows[index].Cells.Count >= 3)
              inTabel.Rows[index].Cells[2].Merge(inTabel.Rows[index].Cells[3]);

            //volgorde voor het liturgiebord is
            //liederen
            //lezen
            //tekst
            if (!lezengehad) {
              if (!String.IsNullOrWhiteSpace(lezen)) {
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
            else if (!tekstgehad) {
              if (!String.IsNullOrWhiteSpace(tekst)) {
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

    private void InvullenVolgende(ILiturgieZoekresultaat regel, ILiturgieZoekresultaatDeel deel, ILiturgieZoekresultaat volgende, Microsoft.Office.Interop.PowerPoint.Shape shape) {
      // Alleen volgende tonen als we op het laatste item zitten en als volgende er is
      var tonen = regel.Resultaten.Last() == deel && volgende != null;
      // Check of de volgende 'blanco' heet, want dan tonen we m niet
      tonen = tonen && String.Compare(volgende.VirtueleBenaming, "Blanco", true) != 0;
      if (tonen)
        shape.TextFrame.TextRange.Text = String.Format("{0} {1}", _instellingen.StandaardTekst.Volgende, LiedNaam(volgende));
      else
        shape.TextFrame.TextRange.Text = string.Empty;
    }

    private String InvullenLiedTekst(String tempinhoud, Slide slide, Microsoft.Office.Interop.PowerPoint.Shape shape) {
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
        return null;  // er is niets over

      // kijk waar we eindigen als we instellinge-aantal tellen vanaf ons startpunt
      var eindIndex = regels.Select((r, i) => new { Regel = r, Index = i })
        .Where(r => r.Index >= beginIndex && (r.Index - beginIndex) < _instellingen.Regelsperslide && r.Regel != NieuweSlideAanduiding)
        .Select(r => r.Index)  // eindindex is er altijd als er een begin is
        .LastOrDefault();

      var optimaliseerEindIndex = eindIndex;
      // Kijk of we niet beter op een eerdere witregel kunnen stoppen
      if (!SkipRegel(regels[optimaliseerEindIndex]) && regels.Length != optimaliseerEindIndex + 1) {
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
      shape.TextFrame.TextRange.Text = String.Join("", insertLines.Select((l, i) => l + (i + 1 == insertLines.Count ? "" : "\r\n")));

      var overStart = optimaliseerEindIndex + 1;
      if (overStart >= regels.Length)
        return null;
      if (regels[overStart] == NieuweSlideAanduiding)
        overStart++;
      var overLines = regels.Skip(overStart).ToList();

      // afbreek teken tonen alleen als een vers doormidden gebroken is
      if (!SkipRegel(insertLines.Last()) && overLines.Any() && !SkipRegel(overLines.First()))
        shape.TextFrame.TextRange.Text += "\r\n >>";

      // Geef de resterende regels terug
      return String.Join("", overLines.Select((l, i) => l + (i + 1 == overLines.Count ? "" : "\r\n")));
    }
    private static Boolean SkipRegel(String regel) {
      return String.IsNullOrWhiteSpace(regel) || regel == NieuweSlideAanduiding;
    }

    private static void InvullenLiturgieRegel(ILiturgieZoekresultaat regel, ILiturgieZoekresultaatDeel vanafDeel, Microsoft.Office.Interop.PowerPoint.Shape shape) {
      shape.TextFrame.TextRange.Text = LiedNaam(regel, vanafDeelHint: vanafDeel);
    }

    private static String LiedNaam(ILiturgieZoekresultaat regel, ILiturgieZoekresultaatDeel vanafDeelHint = null) {
      if (regel.Type == LiturgieType.EnkelZonderDeel)
        return regel.VirtueleBenaming;
      else if (regel.Type == LiturgieType.EnkelMetDeel)
        return String.Format("{0} {1}", regel.VirtueleBenaming, regel.DeelBenaming);
      var vanafDeel = vanafDeelHint ?? regel.Resultaten.FirstOrDefault();  // Bij een deel hint tonen we alleen nog de huidige en komende versen
      var gebruikDeelRegels = regel.Resultaten.SkipWhile(r => r != vanafDeel);
      return String.Format("{0} {1}: {2}", regel.VirtueleBenaming, regel.DeelBenaming, LiedVerzen(gebruikDeelRegels));
    }
    private static String LiedVerzen(IEnumerable<ILiturgieZoekresultaatDeel> vanDelen) {
      return String.Join(",", vanDelen.Select(r => " " + r.Nummer)).TrimEnd(new char[] { ',' });
    }

    /// <summary>
    /// Voeg een slide in in de hoofdpresentatie op de volgende positie (hoofdpresentatie werd aangemaakt bij het maken van deze klasse)
    /// </summary>
    /// <param name="slides">de slide die ingevoegd moet worden (voorwaarde is hierbij dat de presentatie waarvan de slide onderdeel is nog wel geopend is)</param>
    private void VoegSlideinPresentatiein(Slides slides) {
      foreach (Slide slide in slides) {
        //dit gedeelte is om het probleem van de eerste slide die al bestaat op te lossen voor alle andere gevallen maken we gewoon een nieuwe slide aan
        Slide voeginslide;
        if (_slideteller == 1)
          voeginslide = _objPres.Slides[_slideteller];
        else
          voeginslide = _objPres.Slides.AddSlide(_slideteller, _layout);

        //verwijder alle standaard toegevoegde dingen
        while (voeginslide.Shapes.Count > 0) {
          voeginslide.Shapes[1].Delete();
        }
        //voeg de dingen van de template toe
        foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes) {
          try {
            shape.Copy();
            voeginslide.Shapes.Paste();
          }
          catch (Exception) { }
        }

        _slideteller++;
      }
    }

    public void Dispose() {
      _layout = null;
      //if (_objPres != null)   // Sluit dit, dan wordt de presentatie afgesloten
      //  _objPres.Close();
      _objPres = null;
      if (_objApp != null)
        _objApp.Quit();
      _objApp = null;
    }

    public enum Status {
      Gestart,
      StopFout,
      StopGoed,
    }
  }
}