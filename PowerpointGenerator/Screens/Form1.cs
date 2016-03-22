using ILiturgieDatabase;
using ISettings;
using PowerpointGenerator.Powerpoint;
using PowerpointGenerator.Database;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PowerpointGenerator.Properties;
using PowerpointGenerator.LiturgieInterpretator;

namespace PowerpointGenerator
{
    internal partial class Form1 : Form
    {
        private readonly ILiturgieLosOp _liturgieOplosser;
        private readonly IInstellingenFactory _instellingenFactory;
        private readonly Func<ISlideBuilder.IBuilder> _builderResolver;
        private readonly string _startBestand;

        //huidige bestand
        private string _currentfile = "";
        //locatie van het programma op de pc
        private string _programDirectory = "";
        //locatie van temporary liturgie (restore punt)
        private string _tempLiturgiePath = "";
        //generator
        private PpGenerator _powerpoint;
        private GeneratorStatus _status;

        public Form1(ILiturgieLosOp liturgieOplosser, IInstellingenFactory instellingenOplosser, Func<ISlideBuilder.IBuilder> builderResolver, string startBestand)
        {
            _liturgieOplosser = liturgieOplosser;
            _instellingenFactory = instellingenOplosser;
            _builderResolver = builderResolver;
            _startBestand = startBestand;
            InitializeComponent();
        }

        public void Opstarten()
        {
            HelpRequested += Form1_HelpRequested;
            _programDirectory = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar;
            _tempLiturgiePath = _programDirectory + @"temp.liturgie";

            KeyDown += Form1_KeyDown;

            _powerpoint = new PpGenerator(_builderResolver, PresentatieVoortgangCallback, PresentatieGereedmeldingCallback);
            progressBar1.Visible = false;

            if (!string.IsNullOrEmpty(_startBestand) && File.Exists(_startBestand))
            {
                LoadWorkingfile(OpenenopLocatie(_startBestand));
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

        private void openLiturgieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadWorkingfile(Openen());
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Opslaan(GetWorkingFile());
        }

        private void slaLiturgieOpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opslaan_Op_Locatie(GetWorkingFile(), _currentfile);
        }

        private void nieuweLiturgieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _currentfile = "";
            richTextBox1.Text = "";
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
        }

        private void afsluitenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion bestand
        #region bewerken

        private void Form1_KeyDown(object sender, KeyEventArgs e)
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
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            richTextBox1.Redo();
        }
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            richTextBox1.Undo();
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
        }
        private void plakkenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Paste();
        }
        private void kopierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }
        private void knippenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Cut();
        }
        #endregion bewerken
        #region opties
        private void templatesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var formulier = new Instellingenform(_instellingenFactory);
            formulier.Opstarten();
            if (formulier.ShowDialog() == DialogResult.Yes && formulier.Instellingen != null)
            {
                if (!_instellingenFactory.WriteToXmlFile(formulier.Instellingen))
                    MessageBox.Show(Resources.Form1_Niet_opgeslagen_wegens_te_lang_pad);
            }
        }
        private void bekijkDatabaseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "/root, \"" + _instellingenFactory.LoadFromXmlFile().FullDatabasePath + "\"");
        }
        private void stopPowerpointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KillPowerpointProcesses();
        }
        private void invoerenMasksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var formulier = new MaskInvoer(_instellingenFactory.LoadFromXmlFile().Masks);
            if (formulier.ShowDialog() == DialogResult.OK)
            {
                var instellingen = _instellingenFactory.LoadFromXmlFile();
                instellingen.ClearMasks();
                foreach (var mask in formulier.Masks)
                {
                    instellingen.AddMask(mask);
                }
                if (!_instellingenFactory.WriteToXmlFile(instellingen))
                    MessageBox.Show(Resources.Form1_Niet_opgeslagen_wegens_te_lang_pad);
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
            StartGenereren();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //
        }
        private void Form1_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            Help.ShowHelp(this, "help.chm", HelpNavigator.TopicId, "20");
            hlpevent.Handled = true;
        }
        #endregion formulier eventhandlers
        #endregion Eventhandlers

        public void StartGenereren()
        {
            if (_status == GeneratorStatus.Gestopt)
            {
                //sla een back up voor als er iets fout gaat
                Opslaan_Op_Locatie(GetWorkingFile(), _tempLiturgiePath);
                
                #region creeer lijst van liturgie
                // Liturgie uit tekstbox omzetten in leesbare items
                var ruweLiturgie = new InterpreteerLiturgieRuw().VanTekstregels(richTextBox1.Lines);
                // Zoek op het bestandssysteem zo veel mogelijk al op (behalve ppt, die gaan via COM element)
                var masks = MapMasksToLiturgie.Map(_instellingenFactory.LoadFromXmlFile().Masks);
                var ingeladenLiturgie = _liturgieOplosser.LosOp(ruweLiturgie, masks).ToList();

                //als niet alle liturgie is gevonden geven we een melding of de gebruiker toch door wil gaan met genereren
                if (ingeladenLiturgie.Any(l => l.Resultaat != LiturgieOplossingResultaat.Opgelost))
                {
                    var errorformulier = new LiturgieNotFoundFormulier(ingeladenLiturgie.Where(l => l.Resultaat != LiturgieOplossingResultaat.Opgelost));
                    if (errorformulier.ShowDialog() == DialogResult.Cancel)
                        return;
                    ingeladenLiturgie = ingeladenLiturgie.Where(l => l.Resultaat == LiturgieOplossingResultaat.Opgelost).ToList();
                }
                #endregion creeer lijst van liturgie

                #region open een save window

                var saveFileDialog1 = new SaveFileDialog
                {
                    Filter = Resources.Form1_Opslaan_pp_filter,
                    Title = Resources.Form1_Opslaan_pp_title
                };
                //return als er word geannuleerd
                var fileName = saveFileDialog1.ShowDialog() != DialogResult.Cancel ? saveFileDialog1.FileName : null;
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
                #endregion open een save window

                // start de progress tracking
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = ingeladenLiturgie.Count;

                // de knop de status laten reflecteren
                button1.Text = "Stop";
                _status = GeneratorStatus.AanHetGenereren;
                var status = _powerpoint.Initialiseer(ingeladenLiturgie.Select(l => l.Regel).ToList(), textBox2.Text, textBox3.Text, textBox4.Text, textBox1.Text, textBox5.Text, _instellingenFactory.LoadFromXmlFile(), fileName);
                if (status.Fout != null)
                    MessageBox.Show(status.Fout.Melding + "\n\n" + status.Fout.Oplossing, status.Fout.Oplossing);
                else {
                    status = _powerpoint.Start();
                    if (status.Fout != null)
                        MessageBox.Show(status.Fout.Melding + "\n\n" + status.Fout.Oplossing, status.Fout.Oplossing);
                }
                if (status.NieuweStatus != PpGenerator.State.Gestart)
                    PresentatieGereedmeldingCallback();
            }
            else {
                var status = _powerpoint.Stop();
                if (status.Fout != null)
                    MessageBox.Show(status.Fout.Melding + "\n\n" + status.Fout.Oplossing, status.Fout.Oplossing);
            }
        }

        #region functies
        #region Algemene functies

        /// <summary>
        /// Uitlezen van een file die gekozen word aan de hand van openfiledialog
        /// </summary>
        /// <returns> return inhoud gekozen file als string</returns>
        private string Openen()
        {
            //open een open window met bepaalde instellingen
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = Resources.Form1_Openen_liturgie_filter,
                Title = Resources.Form1_Openen_liturgie_title
            };

            return openFileDialog1.ShowDialog() == DialogResult.Cancel ? "" : OpenenopLocatie(openFileDialog1.FileName);
        }

        /// <summary>
        /// uitlezen van een file op meegegeven pad
        /// </summary>
        /// <param name="pad"></param>
        /// <returns></returns>
        private string OpenenopLocatie(string pad)
        {
            //probeer om te lezen van gekozen bestand
            try
            {
                //open een filestream naar het gekozen bestand
                var strm = new FileStream(pad, FileMode.Open, FileAccess.Read);

                //gebruik streamreader om te lezen van de filestream
                using (var rdr = new StreamReader(strm))
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
            var saveFileDialog1 = new SaveFileDialog
            {
                Filter = Resources.Form1_Openen_liturgie_filter,
                Title = "Sla het bestand op"
            };

            //return als er word geannuleerd
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            // Als er een filename gekozen is die niet leeg is sla erin op
            if (saveFileDialog1.FileName != "")
            {
                //probeer om te schrijven naar gekozen bestand
                try
                {
                    //open een streamwriter naar gekozen bestand
                    using (var writer = new StreamWriter(saveFileDialog1.FileName))
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
                using (var writer = new StreamWriter(path))
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
        private void PresentatieGereedmeldingCallback(string opgeslagenAlsBestand = null, string foutmelding = null, int? slidesGemist = null)
        {
            var actie = new Action(() =>
            {
                _status = GeneratorStatus.Gestopt;
                button1.Text = "Generate";
                progressBar1.Visible = false;
                if (string.IsNullOrEmpty(foutmelding))
                {
                    if (string.IsNullOrEmpty(opgeslagenAlsBestand)) return;
                    if (slidesGemist !=null && slidesGemist > 0)
                        MessageBox.Show($"Bij het maken van de presentatie zijn [{slidesGemist}] slides mislukt", "Missende slides", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    if (File.Exists(_tempLiturgiePath))
                        File.Delete(_tempLiturgiePath);
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = @"POWERPNT.exe",
                        Arguments = $"\"{opgeslagenAlsBestand}\"",
                        ErrorDialog = true
                    };
                    Process.Start(startInfo);
                }
                else
                    MessageBox.Show(foutmelding, "Fout", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var inputstring = new TextBox {Text = input};
            var i = 0;
            for (; i < inputstring.Lines.Length; i++)
            {
                if (inputstring.Lines[i].StartsWith("<"))
                {
                    break;
                }
                if (!inputstring.Lines[i].Equals(""))
                    richTextBox1.Text += inputstring.Lines[i] + "\n";
            }
            for (; i < inputstring.Lines.Length; i++)
            {
                if (inputstring.Lines[i].Equals("")) continue;
                var inputstringparts = inputstring.Lines[i].Split('<', '>');
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
                        for (var j = 2; j < inputstringparts.Length; j += 2)
                        {
                            if (j + 2 < inputstringparts.Length)
                                textBox1.Text += inputstringparts[j] + "\n";
                            else
                                textBox1.Text += inputstringparts[j];
                        }
                        break;
                    case "Tekst":
                        textBox5.Text = "";
                        for (var j = 2; j < inputstringparts.Length; j += 2)
                        {
                            if (j + 2 < inputstringparts.Length)
                                textBox5.Text += inputstringparts[j] + "\n";
                            else
                                textBox5.Text += inputstringparts[j];
                        }
                        break;
                }
            }
        }

        private string GetWorkingFile()
        {
            var output = richTextBox1.Text + "\n";
            output += "<Voorganger:>" + textBox2.Text + "\n";
            output += "<1e Collecte:>" + textBox3.Text + "\n";
            output += "<2e Collecte:>" + textBox4.Text + "\n";

            output += "<Lezen>";
            var regels = (textBox1.Text ?? "").Split(new[] { "\r\n" }, StringSplitOptions.None);
            for (var i = 0; i < regels.Length; i++)
            {
                if (regels[i].Equals("")) continue;
                if (i + 1 < regels.Length)
                    output += regels[i] + "<n>";
                else
                    output += regels[i];
            }
            output += "\n";

            output += "<Tekst>";
            regels = (textBox5.Text ?? "").Split(new[] { "\r\n" }, StringSplitOptions.None);
            for (var i = 0; i < regels.Length; i++)
            {
                if (regels[i].Equals("")) continue;
                if (i + 1 < regels.Length)
                    output += regels[i] + "<n>";
                else
                    output += regels[i];
            }
            output += "\n";

            return output;
        }
        #endregion Load/Save workingfile

        #endregion functies

        private enum GeneratorStatus
        {
            Gestopt,
            AanHetGenereren
        }
    }
}
