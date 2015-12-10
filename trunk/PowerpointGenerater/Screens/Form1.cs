using Microsoft.Practices.Unity;
using PowerpointGenerater.AppFlow;
using PowerpointGenerater.Powerpoint;
using PowerpointGenerator.Database;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PowerpointGenerater
{
    public partial class Form1 : MainForm
    {
        // unity container voor dependency injection
        public IUnityContainer DI { get; set; }

        //huidige bestand
        private string _currentfile = "";
        //locatie van het programma op de pc
        private string _programDirectory = "";
        //locatie van temporary liturgie (restore punt)
        private string _tempLiturgiePath = "";
        //instellingen
        private Instellingen _instellingen;
        //generator
        private PPGenerator _powerpoint;

        public Form1()
        {
            InitializeComponent();
        }

        public override void Opstarten(string startBestand = null)
        {
            HelpRequested += new HelpEventHandler(Form1_HelpRequested);
            string[] temp = Application.ExecutablePath.Split('\\');
            _programDirectory = "";
            for (int i = 0; (i + 1) < temp.Count(); i++)
            {
                _programDirectory += temp[i];
                _programDirectory += @"\";
            }
            _tempLiturgiePath = _programDirectory + @"temp.liturgie";

            KeyDown += new KeyEventHandler(Form1_KeyDown);

            if (File.Exists(_programDirectory + "Instellingen.xml") && File.Exists(_programDirectory + "masks.xml"))
            {
                _instellingen = Instellingen.LoadFromXMLFile(_programDirectory);
            }
            else {
                //default instellingen
                _instellingen = new Instellingen((_programDirectory + @"Resources\Database"), (_programDirectory + @"Resources\Database\Template Liederen.pptx"), (_programDirectory + @"Resources\Database\Achtergrond.pptx"));
                _instellingen.AddMask(new Mapmask("Ps", "psalm"));
                _instellingen.AddMask(new Mapmask("GK", "gezang"));
                _instellingen.AddMask(new Mapmask("LB", "lied"));
                _instellingen.AddMask(new Mapmask("Opw", "opwekking"));
            }

            _powerpoint = new PPGenerator(DI, PresentatieVoortgangCallback, PresentatieGereedmeldingCallback);
            progressBar1.Visible = false;

            if (!string.IsNullOrEmpty(startBestand) && File.Exists(startBestand))
            {
                LoadWorkingfile(OpenenopLocatie(startBestand));
            }
            else if (File.Exists(_tempLiturgiePath))
            {
                LoadWorkingfile(OpenenopLocatie(_tempLiturgiePath));
                File.Delete(_tempLiturgiePath);
            }
        }

        #region Eventhandlers
        #region menu eventhandlers
        #region bestand
        void openLiturgieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadWorkingfile(Openen());
        }
        void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Opslaan(GetWorkingFile());
        }
        void slaLiturgieOpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opslaan_Op_Locatie(GetWorkingFile(), _currentfile);
        }
        void nieuweLiturgieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _currentfile = "";
            richTextBox1.Text = "";
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
        }
        void afsluitenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion bestand
        #region bewerken
        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                nieuweLiturgieToolStripMenuItem_Click(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.O)
            {
                openLiturgieToolStripMenuItem_Click(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                slaLiturgieOpToolStripMenuItem_Click(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                Close();
            }
        }
        void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            richTextBox1.Redo();
        }
        void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            richTextBox1.Undo();
        }
        void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
        }
        void plakkenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Paste();
        }
        void kopierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }
        void knippenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Cut();
        }
        #endregion bewerken
        #region opties
        private void templatesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var formulier = DI.Resolve<SettingsForm>();
            formulier.Opstarten(_instellingen);
            if (formulier.ShowDialog() == DialogResult.Yes)
            {
                _instellingen = formulier.Instellingen;
                if (!_instellingen.WriteToXMLFile(_programDirectory))
                    MessageBox.Show("Niet opgeslagen wegens te lang pad");
            }
        }
        private void bekijkDatabaseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "/root, \"" + _instellingen.FullDatabasePath + "\"");
        }
        private void stopPowerpointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KillPowerpointProcesses();
        }
        private void invoerenMasksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var formulier = new MaskInvoer(_instellingen.Masks);
            if (formulier.ShowDialog() == DialogResult.OK)
            {
                _instellingen.ClearMasks();
                foreach (var mask in formulier.Masks)
                {
                    _instellingen.AddMask(mask);
                }
            }
        }
        #endregion opties
        private void contactToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Contactform form = new Contactform();
            form.Show();
        }
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnHelpRequested(new HelpEventArgs(new Point(0, 0)));
        }
        #endregion menu eventhandlers
        #region formulier eventhandlers
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Generate")
            {
                //open een save window
                var saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "Powerpoint bestand (*.ppt)|*.ppt|Powerpoint bestanden (*.pptx)|*.pptx";
                saveFileDialog1.Title = "Sla de presentatie op";
                //return als er word geannuleerd
                var fileName = saveFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.Cancel ? saveFileDialog1.FileName : null;
                if (string.IsNullOrEmpty(fileName))
                    return;

                // Check bestandsnaam
                if (!Path.HasExtension(fileName))
                    fileName += ".ppt";
                if (File.Exists(fileName))
                {
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch
                    {
                        MessageBox.Show("Het geselecteerde bestand kan niet aangepast worden.\n\nControleer of het bestand nog geopend is.", "Bestand niet toegankelijk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                try
                {
                    // Maak het bestand empty aan en check daarmee of er op die plek te schrijven is
                    var file = File.Create(fileName);
                    file.Close();
                }
                catch
                {
                    MessageBox.Show("Kan niet schrijven naar de opgegeven bestandsnaam.\n\nControleer of het pad toegankelijk is.", "Bestand niet toegankelijk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //sla een back up voor als er iets fout gaat
                Opslaan_Op_Locatie(GetWorkingFile(), _tempLiturgiePath);
                #region creeer lijst van liturgie
                // Liturgie uit tekstbox omzetten in leesbare items
                var ruweLiturgie = new InterpreteerLiturgieRuw().VanTekstregels(richTextBox1.Lines);
                // Ruwe liturgie omzetten naar zoekacties voor in t file systeem
                var liturgieZoekVoorbereider = new InterpreteerLiturgieZoekacie(_instellingen.Masks).VanOnderdelen(ruweLiturgie);
                // Zoek op het bestandssysteem zo veel mogelijk al op (behalve ppt, die gaan via COM element)
                var ingeladenLiturgie = new LiturgieDatabase(_instellingen.FullDatabasePath).Zoek(liturgieZoekVoorbereider);

                //als niet alle liturgie is gevonden geven we een melding of de gebruiker toch door wil gaan met genereren
                if (!ingeladenLiturgie.All(l => l.Resultaten.All(r => r.Gevonden)))
                {
                    var melding = string.Join(" ",
                      ingeladenLiturgie.Where(l => l.Resultaten.Any(r => !r.Gevonden))
                      .SelectMany(l => l.Resultaten.Where(r => !r.Gevonden).Select(r => l.EchteBenaming + " " + l.DeelBenaming + " " + r.Nummer))
                    );
                    var errorformulier = new LiturgieNotFoundFormulier(melding);
                    if (errorformulier.ShowDialog() == DialogResult.Cancel)
                    {
                        return;
                    }
                }
                #endregion creeer lijst van liturgie
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = ingeladenLiturgie.Count();

                button1.Text = "Stop";
                _powerpoint.Initialiseer(ingeladenLiturgie, textBox2.Text, textBox3.Text, textBox4.Text, textBox1.Text, textBox5.Text, _instellingen, fileName);
                var status = _powerpoint.Start();
                if (status.Fout != null)
                    MessageBox.Show(status.Fout.Melding + "\n\n" + status.Fout.Oplossing, status.Fout.Oplossing);
                if (status.NieuweStatus != PPGenerator.State.Gestart)
                    PresentatieGereedmeldingCallback();
            }
            else {
                var status = _powerpoint.Stop();
                if (status.Fout != null)
                    MessageBox.Show(status.Fout.Melding + "\n\n" + status.Fout.Oplossing, status.Fout.Oplossing);
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_instellingen.WriteToXMLFile(_programDirectory))
                MessageBox.Show("Niet opgeslagen wegens te lang pad");
        }
        private void Form1_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
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
        private string Openen()
        {
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
        private string OpenenopLocatie(string pad)
        {
            FileStream strm;
            //probeer om te lezen van gekozen bestand
            try
            {
                //open een filestream naar het gekozen bestand
                strm = new FileStream(pad, FileMode.Open, FileAccess.Read);

                //gebruik streamreader om te lezen van de filestream
                using (StreamReader rdr = new StreamReader(strm))
                {
                    //geef een melding dat het gelukt is
                    Console.WriteLine("uitgelezen");

                    //sla locatie op voor het huidige geopende bestand
                    _currentfile = pad;

                    //geef het resultaat van de streamreader terug als string
                    return rdr.ReadToEnd();
                }
            }
            //vang errors af en geef een melding dat er iets is fout gegaan
            catch (Exception)
            {
                MessageBox.Show("Fout tijdens openen bestand", "Bestand error",
                                 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            return "";
        }

        /// <summary>
        /// Opslaan van het meegegeven bestand op een locatie gekozen door savefiledialog
        /// </summary>
        /// <param name="bestand">bestand als string dat opgeslagen moet worden</param>
        private void Opslaan(string bestand)
        {
            //open een save window met bepaalde instellingen
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Liturgie bestanden|*.liturgie";
            saveFileDialog1.Title = "Sla het bestand op";

            //return als er word geannuleerd
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            // Als er een filename gekozen is die niet leeg is sla erin op
            if (saveFileDialog1.FileName != "")
            {
                //probeer om te schrijven naar gekozen bestand
                try
                {
                    //open een streamwriter naar gekozen bestand
                    using (StreamWriter writer = new StreamWriter(saveFileDialog1.FileName))
                    {
                        //schrijf string weg naar streamwriter
                        writer.Write(bestand);

                        Console.WriteLine("opgeslagen");

                        //sla de locatie op voor het huidige bestand
                        _currentfile = saveFileDialog1.FileName;
                    }
                }
                //vang errors af en geef een melding dat er iets is fout gegaan
                catch (Exception)
                {
                    MessageBox.Show("Fout tijdens opslaan bestand", "Bestand error",
                                 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        /// <summary>
        /// Opslaan van het meegegeven bestand op de meegegeven locatie
        /// </summary>
        /// <param name="bestand">bestand als string dat opgeslagen moet worden</param>
        /// <param name="path">path waarin het bestand moet worden opgeslagen</param>
        private void Opslaan_Op_Locatie(string bestand, string path)
        {
            //controleer dat het pad niet leeg is en anders laden we gewoon opslaan
            if (path.Equals(""))
            {
                Opslaan(bestand);
                return;
            }
            //probeer om te schrijven naar gekozen bestand
            try
            {
                //open een streamwriter naar gekozen bestand
                using (StreamWriter writer = new StreamWriter(path))
                {
                    //schrijf string weg naar streamwriter
                    writer.Write(bestand);

                    Console.WriteLine("opgeslagen");
                }
            }
            //vang errors af en geef een melding dat er iets is fout gegaan
            catch (Exception)
            {
                MessageBox.Show("Fout tijdens opslaan bestand", "Bestand error",
                             MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void KillPowerpointProcesses()
        {
            foreach (var proces in Process.GetProcessesByName("powerpnt"))
            {
                proces.Kill();
            }
        }

        private void PresentatieVoortgangCallback(int lijstStart, int lijstEind, int bijItem)
        {
            var actie = new Action(() =>
            {
                progressBar1.Value = bijItem;
                progressBar1.Minimum = lijstStart;
                progressBar1.Maximum = lijstEind;
                progressBar1.Refresh();
            });
            Invoke(actie);
        }
        private void PresentatieGereedmeldingCallback(string opgeslagenAlsBestand = null, string foutmelding = null)
        {
            var actie = new Action(() =>
            {
                button1.Text = "Generate";
                progressBar1.Visible = false;
                if (!string.IsNullOrEmpty(opgeslagenAlsBestand))
                {
                    if (File.Exists(_tempLiturgiePath))
                        File.Delete(_tempLiturgiePath);
                    var startInfo = new ProcessStartInfo();
                    startInfo.FileName = @"POWERPNT.exe";
                    startInfo.Arguments = opgeslagenAlsBestand;
                    Process.Start(startInfo);
                }
            });
            Invoke(actie);
        }

        #endregion Algemene functies
        #region Load/Get workingfile

        private void LoadWorkingfile(string input)
        {
            if (input.Equals(""))
                return;
            richTextBox1.Text = "";
            TextBox inputstring = new TextBox();
            inputstring.Text = input;
            int i = 0;
            for (; i < inputstring.Lines.Count(); i++)
            {
                if (inputstring.Lines[i].StartsWith("<"))
                {
                    break;
                }
                if (!inputstring.Lines[i].Equals(""))
                    richTextBox1.Text += inputstring.Lines[i] + "\n";
            }
            for (; i < inputstring.Lines.Count(); i++)
            {
                if (!inputstring.Lines[i].Equals(""))
                {
                    string[] inputstringparts = inputstring.Lines[i].Split('<', '>');
                    switch (inputstringparts[1])
                    {
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
                            for (int j = 2; j < inputstringparts.Count(); j += 2)
                            {
                                if (j + 2 < inputstringparts.Count())
                                    textBox1.Text += inputstringparts[j] + "\n";
                                else
                                    textBox1.Text += inputstringparts[j];
                            }
                            break;
                        case "Tekst":
                            textBox5.Text = "";
                            for (int j = 2; j < inputstringparts.Count(); j += 2)
                            {
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

        private string GetWorkingFile()
        {
            string output = richTextBox1.Text + "\n";
            output += "<Voorganger:>" + textBox2.Text + "\n";
            output += "<1e Collecte:>" + textBox3.Text + "\n";
            output += "<2e Collecte:>" + textBox4.Text + "\n";

            TextBox box = new TextBox();

            output += "<Lezen>";
            box.Text = textBox1.Text;
            for (int i = 0; i < box.Lines.Count(); i++)
            {
                if (!box.Lines[i].Equals(""))
                {
                    if (i + 1 < box.Lines.Count())
                        output += box.Lines[i] + "<n>";
                    else
                        output += box.Lines[i];
                }
            }
            output += "\n";

            output += "<Tekst>";
            box.Text = textBox5.Text;
            for (int i = 0; i < box.Lines.Count(); i++)
            {
                if (!box.Lines[i].Equals(""))
                {
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
        #endregion functies
    }
}
