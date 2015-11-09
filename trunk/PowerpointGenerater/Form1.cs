using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace PowerpointGenerater {
  public partial class Form1 : Form {
    //huidige bestand
    private String Currentfile = "";
    //locatie van het programma op de pc
    private String ProgramDirectory = "";
    //locatie van temporary liturgie (restore punt)
    private String TempLiturgiePath = "";
    //instellingen
    public Instellingen instellingen;

    public AutoResetEvent autoEvent;
    private Thread GenerateThread;

    public Form1(string[] args) {
      InitializeComponent();
      Control.CheckForIllegalCrossThreadCalls = false;
      this.HelpRequested += new HelpEventHandler(this.Form1_HelpRequested);
      string[] temp = Application.ExecutablePath.Split('\\');
      ProgramDirectory = "";
      for (int i = 0; (i + 1) < temp.Count(); i++) {
        ProgramDirectory += temp[i];
        ProgramDirectory += @"\";
      }
      TempLiturgiePath = ProgramDirectory + @"temp.liturgie";

      this.KeyDown += new KeyEventHandler(Form1_KeyDown);

      if (File.Exists(ProgramDirectory + "Instellingen.xml") && File.Exists(ProgramDirectory + "masks.xml")) {
        instellingen = Instellingen.LoadXML(ProgramDirectory);
      }
      else {
        //default instellingen
        instellingen = new Instellingen((ProgramDirectory + @"Resources\Database"), (ProgramDirectory + @"Resources\Database\Template Liederen.pptx"), (ProgramDirectory + @"Resources\Database\Achtergrond.pptx"), 6);
        instellingen.AddMask(new Mapmask("Ps", "psalm"));
        instellingen.AddMask(new Mapmask("GK", "gezang"));
        instellingen.AddMask(new Mapmask("LB", "lied"));
        instellingen.AddMask(new Mapmask("Opw", "opwekking"));
      }

      progressBar1.Visible = false;
      autoEvent = new AutoResetEvent(false);

      if (args.Count() >= 1) {
        LoadWorkingfile(OpenenopLocatie(args[0]));
      }
      else if (File.Exists(TempLiturgiePath)) {
        LoadWorkingfile(OpenenopLocatie(TempLiturgiePath));
        File.Delete(TempLiturgiePath);
      }
    }

    #region Eventhandlers
    #region menu eventhandlers
    #region bestand
    void openLiturgieToolStripMenuItem_Click(object sender, EventArgs e) {
      LoadWorkingfile(Openen());
    }
    void toolStripMenuItem2_Click(object sender, EventArgs e) {
      Opslaan(GetWorkingFile());
    }
    void slaLiturgieOpToolStripMenuItem_Click(object sender, EventArgs e) {
      Opslaan_Op_Locatie(GetWorkingFile(), Currentfile);
    }
    void nieuweLiturgieToolStripMenuItem_Click(object sender, EventArgs e) {
      Currentfile = "";
      richTextBox1.Text = "";
      textBox1.Text = "";
      textBox2.Text = "";
      textBox3.Text = "";
      textBox4.Text = "";
      textBox5.Text = "";
    }
    void afsluitenToolStripMenuItem_Click(object sender, EventArgs e) {
      Close();
    }
    #endregion bestand
    #region bewerken
    void Form1_KeyDown(object sender, KeyEventArgs e) {
      if (e.Control && e.KeyCode == Keys.N) {
        nieuweLiturgieToolStripMenuItem_Click(null, null);
      }
      else if (e.Control && e.KeyCode == Keys.O) {
        openLiturgieToolStripMenuItem_Click(null, null);
      }
      else if (e.Control && e.KeyCode == Keys.S) {
        slaLiturgieOpToolStripMenuItem_Click(null, null);
      }
      else if (e.Control && e.KeyCode == Keys.E) {
        Close();
      }
    }
    void toolStripMenuItem4_Click(object sender, EventArgs e) {
      richTextBox1.Redo();
    }
    void toolStripMenuItem3_Click(object sender, EventArgs e) {
      richTextBox1.Undo();
    }
    void toolStripMenuItem1_Click(object sender, EventArgs e) {
      richTextBox1.SelectAll();
    }
    void plakkenToolStripMenuItem_Click(object sender, EventArgs e) {
      richTextBox1.Paste();
    }
    void kopierenToolStripMenuItem_Click(object sender, EventArgs e) {
      richTextBox1.Copy();
    }
    void knippenToolStripMenuItem_Click(object sender, EventArgs e) {
      richTextBox1.Cut();
    }
    #endregion bewerken
    #region opties
    private void templatesToolStripMenuItem1_Click(object sender, EventArgs e) {
      Instellingenform formulier = new Instellingenform(this);
      if (formulier.ShowDialog() == DialogResult.Yes) {
        instellingen.Templateliederen = formulier.textBox1.Text;
        instellingen.Templatetheme = formulier.textBox2.Text;
        instellingen.Databasepad = formulier.textBox3.Text;
        bool result = System.Int32.TryParse(formulier.textBox4.Text, out instellingen.Regelsperslide);
        if (!result) {
          instellingen.Regelsperslide = 6;
        }

        instellingen.StandaardTekst.Volgende = formulier.tbVolgende.Text;
        instellingen.StandaardTekst.Voorganger = formulier.tbVoorganger.Text;
        instellingen.StandaardTekst.Collecte = formulier.tbCollecte.Text;
        instellingen.StandaardTekst.Collecte1 = formulier.tbCollecte1.Text;
        instellingen.StandaardTekst.Collecte2 = formulier.tbCollecte2.Text;
        instellingen.StandaardTekst.Lezen = formulier.tbLezen.Text;
        instellingen.StandaardTekst.Tekst = formulier.tbTekst.Text;
        instellingen.StandaardTekst.Liturgie = formulier.tbLiturgie.Text;

        if (!Instellingen.WriteXML(instellingen, ProgramDirectory))
          MessageBox.Show("Niet opgeslagen wegens te lang pad");
      }
    }
    private void bekijkDatabaseToolStripMenuItem1_Click(object sender, EventArgs e) {
      Process.Start("explorer.exe", "/root, \"" + instellingen.FullDatabasePath + "\"");
    }
    private void stopPowerpointToolStripMenuItem_Click(object sender, EventArgs e) {
      foreach (var proces in Process.GetProcessesByName("powerpnt")) {
        proces.Kill();
      }
    }
    private void invoerenMasksToolStripMenuItem_Click(object sender, EventArgs e) {
      var formulier = new MaskInvoer(instellingen.GetMasks());
      if (formulier.ShowDialog() == DialogResult.OK) {
        instellingen.ClearMasks();
        foreach (var mask in formulier.Masks) {
          instellingen.AddMask(mask);
        }
      }
    }
    #endregion opties
    private void contactToolStripMenuItem_Click(object sender, EventArgs e) {
      Contactform form = new Contactform();
      form.Show();
    }
    private void helpToolStripMenuItem_Click(object sender, EventArgs e) {
      this.OnHelpRequested(new HelpEventArgs(new Point(0, 0)));
    }
    #endregion menu eventhandlers
    #region formulier eventhandlers
    private void button1_Click(object sender, EventArgs e) {
      if (button1.Text == "Generate") {
        //sla een back up voor als er iets fout gaat
        Opslaan_Op_Locatie(GetWorkingFile(), TempLiturgiePath);
        #region creeer lijst van liturgie
        // Liturgie uit tekstbox omzetten in leesbare items
        var ruweLiturgie = new InterpreteerLiturgieRuw().VanTekstregels(richTextBox1.Lines);
        // Ruwe liturgie omzetten naar zoekacties voor in t file systeem
        var liturgieZoekVoorbereider = new InterpreteerLiturgieZoekacie(instellingen.GetMasks()).VanOnderdelen(ruweLiturgie);
        // Zoek op het bestandssysteem zo veel mogelijk al op (behalve ppt, die gaan via COM element)
        var ingeladenLiturgie = new LiturgieDatabase(instellingen.FullDatabasePath).Zoek(liturgieZoekVoorbereider);

        //als niet alle liturgie is gevonden geven we een melding of de gebruiker toch door wil gaan met genereren
        if (!ingeladenLiturgie.All(l => l.Resultaten.All(r => r.Gevonden))) {
          var melding = String.Join(" ",
            ingeladenLiturgie.Where(l => l.Resultaten.Any(r => !r.Gevonden))
            .SelectMany(l => l.Resultaten.Where(r => !r.Gevonden).Select(r => l.EchteBenaming + " " + l.DeelBenaming + " " + r.Nummer))
          );
          var errorformulier = new LiturgieNotFoundFormulier(melding);
          if (errorformulier.ShowDialog() == DialogResult.Cancel) {
            return;
          }
        }
        #endregion creeer lijst van liturgie
        progressBar1.Visible = true;
        progressBar1.Value = 0;
        progressBar1.Minimum = 0;
        progressBar1.Maximum = richTextBox1.Lines.Count();
        progressBar1.Step = 1;

        button1.Text = "Stop";
        Thread t = new Thread(this.CheckProgress);
        t.IsBackground = true;
        t.Start();
        //maak een instantie van powerpoint
        var ppf = new PowerpointFunctions(this);
        ppf.InputGeneratePresentation(ingeladenLiturgie, instellingen.FullTemplateliederen, textBox2.Text, textBox3.Text, textBox4.Text, textBox1.Text, textBox5.Text);
        GenerateThread = new Thread(ppf.GeneratePresentation);
        GenerateThread.IsBackground = true;
        //genereer de presentatie
        GenerateThread.Start();
      }
      else {
        GenerateThread.Abort();
        autoEvent.Set();
      }
    }
    private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
      if (!Instellingen.WriteXML(instellingen, ProgramDirectory))
        MessageBox.Show("Niet opgeslagen wegens te lang pad");
    }
    private void Form1_HelpRequested(object sender, HelpEventArgs hlpevent) {
      Help.ShowHelp(this, "help.chm", HelpNavigator.TopicId, "20");
      hlpevent.Handled = true;
    }
    #endregion formulier eventhandlers
    #endregion Eventhandlers

    #region functies
    #region Algemene functies

    /// <summary>
    /// Uitlezen van een file die gekozen word aan de hand van openfiledialog
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns> return inhoud gekozen file als string</returns>
    private String Openen() {
      //open een open window met bepaalde instellingen
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      openFileDialog1.Filter = "Liturgie bestanden|*.liturgie";
      openFileDialog1.Title = "Kies bestand";

      //return als er word geannuleerd
      if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
        return "";

      return OpenenopLocatie(openFileDialog1.FileName);
    }

    /// <summary>
    /// uitlezen van een file op meegegeven pad
    /// </summary>
    /// <param name="pad"></param>
    /// <returns></returns>
    private String OpenenopLocatie(string pad) {
      FileStream strm;
      //probeer om te lezen van gekozen bestand
      try {
        //open een filestream naar het gekozen bestand
        strm = new FileStream(pad, FileMode.Open, FileAccess.Read);

        //gebruik streamreader om te lezen van de filestream
        using (StreamReader rdr = new StreamReader(strm)) {
          //geef een melding dat het gelukt is
          Console.WriteLine("uitgelezen");

          //sla locatie op voor het huidige geopende bestand
          Currentfile = pad;

          //geef het resultaat van de streamreader terug als string
          return rdr.ReadToEnd();
        }
      }
      //vang errors af en geef een melding dat er iets is fout gegaan
      catch (Exception) {
        System.Windows.Forms.MessageBox.Show("Fout tijdens openen bestand", "Bestand error",
                         System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
      }
      return "";
    }

    /// <summary>
    /// Opslaan van het meegegeven bestand op een locatie gekozen door savefiledialog
    /// </summary>
    /// <param name="bestand">bestand als string dat opgeslagen moet worden</param>
    private void Opslaan(String bestand) {
      //open een save window met bepaalde instellingen
      SaveFileDialog saveFileDialog1 = new SaveFileDialog();
      saveFileDialog1.Filter = "Liturgie bestanden|*.liturgie";
      saveFileDialog1.Title = "Sla het bestand op";

      //return als er word geannuleerd
      if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
        return;

      // Als er een filename gekozen is die niet leeg is sla erin op
      if (saveFileDialog1.FileName != "") {
        //probeer om te schrijven naar gekozen bestand
        try {
          //open een streamwriter naar gekozen bestand
          using (StreamWriter writer = new StreamWriter(saveFileDialog1.FileName)) {
            //schrijf string weg naar streamwriter
            writer.Write(bestand);

            Console.WriteLine("opgeslagen");

            //sla de locatie op voor het huidige bestand
            Currentfile = saveFileDialog1.FileName;
          }
        }
        //vang errors af en geef een melding dat er iets is fout gegaan
        catch (Exception) {
          System.Windows.Forms.MessageBox.Show("Fout tijdens opslaan bestand", "Bestand error",
                       System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
        }
      }
    }

    /// <summary>
    /// Opslaan van het meegegeven bestand op de meegegeven locatie
    /// </summary>
    /// <param name="bestand">bestand als string dat opgeslagen moet worden</param>
    /// <param name="path">path waarin het bestand moet worden opgeslagen</param>
    private void Opslaan_Op_Locatie(String bestand, String path) {
      //controleer dat het pad niet leeg is en anders laden we gewoon opslaan
      if (path.Equals("")) {
        Opslaan(bestand);
        return;
      }
      //probeer om te schrijven naar gekozen bestand
      try {
        //open een streamwriter naar gekozen bestand
        using (StreamWriter writer = new StreamWriter(path)) {
          //schrijf string weg naar streamwriter
          writer.Write(bestand);

          Console.WriteLine("opgeslagen");
        }
      }
      //vang errors af en geef een melding dat er iets is fout gegaan
      catch (Exception) {
        System.Windows.Forms.MessageBox.Show("Fout tijdens opslaan bestand", "Bestand error",
                     System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
      }
    }

    #endregion Algemene functies
    #region Load/Get workingfile

    private void LoadWorkingfile(string input) {
      if (input.Equals(""))
        return;
      richTextBox1.Text = "";
      TextBox inputstring = new TextBox();
      inputstring.Text = input;
      int i = 0;
      for (; i < inputstring.Lines.Count(); i++) {
        if (inputstring.Lines[i].StartsWith("<")) {
          break;
        }
        if (!inputstring.Lines[i].Equals(""))
          richTextBox1.Text += inputstring.Lines[i] + "\n";
      }
      for (; i < inputstring.Lines.Count(); i++) {
        if (!inputstring.Lines[i].Equals("")) {
          string[] inputstringparts = inputstring.Lines[i].Split('<', '>');
          switch (inputstringparts[1]) {
            case "Voorganger:":
              textBox2.Text = inputstringparts[2];
              break;
            case "1e Collecte:":
              textBox3.Text = inputstringparts[2];
              break;
            case "2e Collecte:":
              textBox4.Text = inputstringparts[2];
              break;
            case "Lezen":
              textBox1.Text = "";
              for (int j = 2; j < inputstringparts.Count(); j += 2) {
                if (j + 2 < inputstringparts.Count())
                  textBox1.Text += inputstringparts[j] + "\n";
                else
                  textBox1.Text += inputstringparts[j];
              }
              break;
            case "Tekst":
              textBox5.Text = "";
              for (int j = 2; j < inputstringparts.Count(); j += 2) {
                if (j + 2 < inputstringparts.Count())
                  textBox5.Text += inputstringparts[j] + "\n";
                else
                  textBox5.Text += inputstringparts[j];
              }
              break;
            default:
              break;
          }
        }
      }
    }

    private string GetWorkingFile() {
      string output = richTextBox1.Text + "\n";
      output += "<Voorganger:>" + textBox2.Text + "\n";
      output += "<1e Collecte:>" + textBox3.Text + "\n";
      output += "<2e Collecte:>" + textBox4.Text + "\n";

      TextBox box = new TextBox();

      output += "<Lezen>";
      box.Text = textBox1.Text;
      for (int i = 0; i < box.Lines.Count(); i++) {
        if (!box.Lines[i].Equals("")) {
          if (i + 1 < box.Lines.Count())
            output += box.Lines[i] + "<n>";
          else
            output += box.Lines[i];
        }
      }
      output += "\n";

      output += "<Tekst>";
      box.Text = textBox5.Text;
      for (int i = 0; i < box.Lines.Count(); i++) {
        if (!box.Lines[i].Equals("")) {
          if (i + 1 < box.Lines.Count())
            output += box.Lines[i] + "<n>";
          else
            output += box.Lines[i];
        }
      }
      output += "\n";

      return output;
    }
    #endregion Load/Save workingfile
    private void CheckProgress() {
      autoEvent.WaitOne();
      button1.Text = "Generate";
      progressBar1.Visible = false;
      if (File.Exists(TempLiturgiePath))
        File.Delete(TempLiturgiePath);
    }
    #endregion functies
  }
}
