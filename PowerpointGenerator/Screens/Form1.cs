﻿// Copyright 2017 door Remco Veurink en Erik de Roos
using ILiturgieDatabase;
using ISettings;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PowerpointGenerator.Properties;
using Generator;
using System.Collections.Generic;
using Generator.Database;

namespace PowerpointGenerator.Screens
{
    internal partial class Form1 : Form
    {
        private readonly IInstellingenFactory _instellingenFactory;
        private readonly GeneratieInterface<CompRegistration> _funcs;
        private readonly ILiturgieLosOp _liturgieOplosser;
        private readonly ILiturgieInterpreteer _liturgieInterperator;
        private readonly string _startBestand;

        //locatie van het programma op de pc
        private string _programDirectory = "";

        // huidige zoekresultaat voor autocomplete
        private IVrijZoekresultaat _huidigZoekresultaat;
        private object _dropdownLocker = new object();

        public Form1(IInstellingenFactory instellingenOplosser, GeneratieInterface<CompRegistration> funcs, ILiturgieLosOp liturgieOplosser, ILiturgieInterpreteer liturgieInterperator, string startBestand)
        {
            _instellingenFactory = instellingenOplosser;
            _funcs = funcs;
            _liturgieOplosser = liturgieOplosser;
            _liturgieInterperator = liturgieInterperator;
            _startBestand = startBestand;
            InitializeComponent();
            this.Icon = Icon.FromHandle(Resources.Powerpoint_Overlay_icon.GetHicon());
        }

        public void Opstarten()
        {
            HelpRequested += Form1_HelpRequested;
            _programDirectory = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar;
            _funcs.TempLiturgiePath = _programDirectory + @"temp.liturgie";

            KeyDown += Form1_KeyDown;
            _funcs.RegisterVoortgang(PresentatieVoortgangCallback);
            _funcs.RegisterGereedmelding(PresentatieGereedmeldingCallback);

            progressBar1.Visible = false;

            _funcs.Registration.LiturgieRichTextBox = richTextBox1;
            _funcs.Registration.VoorgangerTextBox = textBox2;
            _funcs.Registration.Collecte1eTextBox = textBox3;
            _funcs.Registration.Collecte2eTextBox = textBox4;
            _funcs.Registration.LezenRichTextBox = textBox1;
            _funcs.Registration.TekstRichTextBox = textBox5;

            _funcs.Opstarten(_startBestand);

            // TODO test
            _liturgieOplosser.VrijZoeken("psalm ", _liturgieInterperator, _huidigZoekresultaat);

            TriggerZoeklijstVeranderd();
        }

        #region Eventhandlers
        #region menu eventhandlers
        #region bestand

        private void openLiturgieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _funcs.LoadWorkingfile(Openen());
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Opslaan(_funcs.GetWorkingFile());
        }

        private void slaLiturgieOpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opslaan_Op_Locatie(_funcs.GetWorkingFile(), _funcs.CurrentFile);
        }

        private void nieuweLiturgieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _funcs.NieuweLiturgie();
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
                if (!_instellingenFactory.WriteToFile(formulier.Instellingen))
                    MessageBox.Show(Resources.Form1_Niet_opgeslagen_wegens_te_lang_pad);
            }
        }
        private void bekijkDatabaseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "/root, \"" + _instellingenFactory.LoadFromFile().FullDatabasePath + "\"");
        }
        private void stopPowerpointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KillPowerpointProcesses();
        }
        private void invoerenMasksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var formulier = new MaskInvoer(_instellingenFactory.LoadFromFile().Masks);
            if (formulier.ShowDialog() == DialogResult.OK)
            {
                var instellingen = _instellingenFactory.LoadFromFile();
                instellingen.ClearMasks();
                foreach (var mask in formulier.Masks)
                {
                    instellingen.AddMask(mask);
                }
                if (!_instellingenFactory.WriteToFile(instellingen))
                    MessageBox.Show(Resources.Form1_Niet_opgeslagen_wegens_te_lang_pad);
            }
        }
        #endregion opties
        private void contactToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new Contactform();
            form.Show();
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
        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            TriggerZoeklijstVeranderd();
        }
        private void textBox6_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                HuidigeTekstInvoegen();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            HuidigeTekstInvoegen();
        }
        #endregion formulier eventhandlers
        #endregion Eventhandlers


        private void TriggerZoeklijstVeranderd()
        {
            _huidigZoekresultaat = _liturgieOplosser.VrijZoeken(textBox6.Text, _liturgieInterperator, _huidigZoekresultaat);
            if (_huidigZoekresultaat.ZoeklijstAanpassing == VrijZoekresultaatAanpassingType.Geen)
                return;

            // We gaan kijken wat de verandering is.
            // Dit moet wat slimmer dan gewoon verwijderen/toevoegen omdat deze lijst zich instabiel gedraagt
            textBox6.SuspendLayout();
            lock(_dropdownLocker)  // Lock om te voorkomen dat werk nog niet af is als we er nog een x in komen (lijkt namelijk te gebeuren)
            {
                if (textBox6.AutoCompleteCustomSource == null)
                {
                    textBox6.AutoCompleteCustomSource = new AutoCompleteStringCollection();
                    textBox6.AutoCompleteCustomSource.AddRange(_huidigZoekresultaat.AlleMogelijkheden.ToArray());
                }
                else if (_huidigZoekresultaat.ZoeklijstAanpassing == VrijZoekresultaatAanpassingType.Alles)
                {
                    textBox6.AutoCompleteCustomSource.Clear();
                    textBox6.AutoCompleteCustomSource.AddRange(_huidigZoekresultaat.AlleMogelijkheden.ToArray());
                }
                else if (_huidigZoekresultaat.ZoeklijstAanpassing == VrijZoekresultaatAanpassingType.Deel)
                {
                    textBox6.AutoCompleteCustomSource.AddRange(_huidigZoekresultaat.DeltaMogelijkhedenToegevoegd.ToArray());
                    foreach (var item in _huidigZoekresultaat.DeltaMogelijkhedenVerwijderd)
                    {
                        textBox6.AutoCompleteCustomSource.Remove(item);
                    }
                }
            }
            textBox6.ResumeLayout();
        }

        private void HuidigeTekstInvoegen()
        {
            var liturgie = richTextBox1.Lines.ToList();
            liturgie.Add(textBox6.Text);
            richTextBox1.Lines = liturgie.ToArray();
            textBox6.Text = null;
        }

        public void StartGenereren()
        {
            if (_funcs.Status == GeneratorStatus.Gestopt)
            {
                //sla een back up voor als er iets fout gaat
                Opslaan_Op_Locatie(_funcs.GetWorkingFile(), _funcs.TempLiturgiePath);

                // creeer lijst van liturgie
                var ingeladenLiturgie = _funcs.LiturgieOplossingen().ToList();

                //als niet alle liturgie is gevonden geven we een melding of de gebruiker toch door wil gaan met genereren
                if (ingeladenLiturgie.Any(l => l.Resultaat != LiturgieOplossingResultaat.Opgelost))
                {
                    var errorformulier = new LiturgieNotFoundFormulier(ingeladenLiturgie.Where(l => l.Resultaat != LiturgieOplossingResultaat.Opgelost));
                    if (errorformulier.ShowDialog() == DialogResult.Cancel)
                        return;
                    ingeladenLiturgie = ingeladenLiturgie.Where(l => l.Resultaat == LiturgieOplossingResultaat.Opgelost).ToList();
                }

                // open een save window voor de presentatie
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
                var saveStatus = _funcs.CheckFileSavePossibilities(fileName);
                if (saveStatus == GeneratieInterface<CompRegistration>.FileSavePossibility.NotDeleteable)
                {
                    MessageBox.Show("Het geselecteerde bestand kan niet aangepast worden.\n\nControleer of het bestand nog geopend is.", "Bestand niet toegankelijk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (saveStatus == GeneratieInterface<CompRegistration>.FileSavePossibility.NotCreateable)
                {
                    MessageBox.Show("Het geselecteerde bestand kan niet aangepast worden.\n\nControleer of het bestand nog geopend is.", "Bestand niet toegankelijk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // start de progress tracking
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = ingeladenLiturgie.Count;

                // de knop de status laten reflecteren
                button1.Text = "Stop";

                var status = _funcs.StartGenereren(ingeladenLiturgie, fileName);
                if (status.Fout != null)
                    MessageBox.Show(status.Fout.Melding + "\n\n" + status.Fout.Oplossing, status.Fout.Oplossing);
            }
            else {
                var status = _funcs.StopGenereren();
                if (status.Fout != null)
                    MessageBox.Show(status.Fout.Melding + "\n\n" + status.Fout.Oplossing, status.Fout.Oplossing);
            }
        }

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
                return _funcs.OpenenopLocatie(pad);
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
                    _funcs.Opslaan(saveFileDialog1.FileName, bestand);
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
                button1.Text = "Generate";
                progressBar1.Visible = false;
                if (string.IsNullOrEmpty(foutmelding))
                {
                    if (slidesGemist !=null && slidesGemist > 0)
                        MessageBox.Show($"Bij het maken van de presentatie zijn [{slidesGemist}] slides mislukt", "Missende slides", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    if (string.IsNullOrEmpty(opgeslagenAlsBestand))
                        return;
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
    }
}
