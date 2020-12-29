// Copyright 2019 door Remco Veurink en Erik de Roos
using Generator.Database.Models;
using Generator.LiturgieInterpretator;
using ISettings;
using PowerpointGenerator.Genereren;
using PowerpointGenerator.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PowerpointGenerator.Screens
{
    /// <remarks>
    /// Win 10 + VS2008, VS2012, etc. vs no way to disable DPI scaling
    /// 
    /// There is no option *listed* because, Microsoft. No Compatibility tab in the right-click context menu for the.exe, and so on.However, it is possible to disable DPI scaling with VS/BIDS/etc by using the right-click context menu option "troubleshoot compatibility". Select the option that says the program used to display correctly but no longer does so. (under 'select troubleshooting option" choose, "troubleshoot program".  Then select, "the program opens but doesn't display correctly", and then, "Program does not display property when large scale fond settings are selected").
    /// Presto! DPI scaling is disabled for VS.Now, to track down the .REG flag it is hopefully flipping...
    /// 
    /// [update] Reg setting info appears to be here:
    /// https://blogs.technet.microsoft.com/mspfe/2013/11/21/disabling-dpi-scaling-on-windows-8-1-the-enterprise-way/
    /// </remarks>
    internal partial class Form1 : Form
    {
        private readonly IInstellingenFactory _instellingenFactory;
        private readonly GeneratieInterface<CompRegistration> _funcs;
        private readonly string _startBestand;

        public Form1(IInstellingenFactory instellingenOplosser, GeneratieInterface<CompRegistration> funcs, ILiturgieZoeken liturgieZoeker, string startBestand)
        {
            _instellingenFactory = instellingenOplosser;
            _funcs = funcs;
            _startBestand = startBestand;
            InitializeComponent();
            liturgieEdit1._liturgieZoeker = liturgieZoeker;
            Icon = Icon.FromHandle(Resources.Powerpoint_Overlay_icon.GetHicon());
        }

        public void Opstarten()
        {
            HelpRequested += Form1_HelpRequested;
            _funcs.TempLiturgiePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\temp.liturgie";
            KeyDown += Form1_KeyDown;
            _funcs.RegisterVoortgang(PresentatieVoortgangCallback);
            _funcs.RegisterGereedmelding(PresentatieGereedmeldingCallback);

            progressBar1.Visible = false;

            _funcs.Registration.LiturgieRichTextBox = liturgieEdit1.TextBoxLiturgie;
            _funcs.Registration.VoorgangerTextBox = textBox2;
            _funcs.Registration.Collecte1eTextBox = textBox3;
            _funcs.Registration.Collecte2eTextBox = textBox4;
            _funcs.Registration.LezenRichTextBox = textBox1;
            _funcs.Registration.TekstRichTextBox = textBox5;

            _funcs.Opstarten(_startBestand);

            BouwLiturgieSchermOp();
        }

        #region Eventhandlers
        #region menu eventhandlers
        #region bestand

        private void openLiturgieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _funcs.LaadVanWerkbestand(Openen());
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Opslaan(_funcs.MaakWerkbestand());
        }

        private void slaLiturgieOpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpslaanOpLocatie(_funcs.MaakWerkbestand(), _funcs.CurrentFile);
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
                BouwLiturgieSchermOp();
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
        private void timerAutosave_Tick(object sender, EventArgs e)
        {
            OpslaanOpLocatie(_funcs.MaakWerkbestand(), _funcs.TempLiturgiePath);
        }
        #endregion formulier eventhandlers
        #endregion Eventhandlers

        private void BouwLiturgieSchermOp()
        {
            var huidigeInstellingen = _instellingenFactory.LoadFromFile();
            groupBox5.Visible = huidigeInstellingen.Een2eCollecte;
            groupBox3.Text = huidigeInstellingen.Een2eCollecte ? "Collecte 1" : "Collecte";
            groupBox4.Visible = huidigeInstellingen.DeLezenVraag;
            groupBox6.Visible = huidigeInstellingen.DeTekstVraag;
        }

        public void StartGenereren()
        {
            if (_funcs.Status == GeneratorStatus.Gestopt)
            {
                //sla een back up voor als er iets fout gaat
                OpslaanOpLocatie(_funcs.MaakWerkbestand(), _funcs.TempLiturgiePath);

                // creeer lijst van liturgie
                var parsedLiturgie = _funcs.LiturgieOplossingen().ToList();

                //als niet alle liturgie is gevonden geven we een melding of de gebruiker wil stoppen of toch door wil gaan
                var fouten = parsedLiturgie.Where(l => l.ResultaatStatus != DatabaseZoekStatus.Opgelost).ToList();
                if (fouten.Any())
                {
                    var errorformulier = new LiturgieNotFoundFormulier(fouten);
                    if (errorformulier.ShowDialog() != DialogResult.OK)
                        return;
                }

                // We gaan door met alle slides die wel opgelost kunnen worden
                var slides = parsedLiturgie.Where(l => l.ResultaatStatus == DatabaseZoekStatus.Opgelost).Select(pl => pl.ResultaatSlide).ToList();

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
                progressBar1.Maximum = parsedLiturgie.Count;

                // de knop de status laten reflecteren
                button1.Text = "Stop";

                var status = _funcs.StartGenereren(slides, fileName);
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

            return openFileDialog1.ShowDialog() == DialogResult.Cancel ? "" : OpenenOpLocatie(openFileDialog1.FileName);
        }

        /// <summary>
        /// uitlezen van een file op meegegeven pad
        /// </summary>
        /// <param name="pad"></param>
        /// <returns></returns>
        private string OpenenOpLocatie(string pad)
        {
            //probeer om te lezen van gekozen bestand
            try
            {
                return _funcs.OpenenOpLocatie(pad);
            }
                //vang errors af en geef een melding dat er iets is fout gegaan
            catch
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
                catch
                {
                    MessageBox.Show("Fout tijdens opslaan bestand", "Bestand error",
                                 MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        /// <summary>
        /// Opslaan van het meegegeven bestand op de meegegeven locatie
        /// </summary>
        /// <param name="bestandsinhoud">bestand als string dat opgeslagen moet worden</param>
        /// <param name="path">path waarin het bestand moet worden opgeslagen</param>
        private void OpslaanOpLocatie(string bestandsinhoud, string path)
        {
            //controleer dat het pad niet leeg is en anders laten we gewoon opslaan
            if (string.IsNullOrWhiteSpace(path))
            {
                Opslaan(bestandsinhoud);
                return;
            }
            //probeer om te schrijven naar gekozen bestand
            try
            {
                //open een streamwriter naar gekozen bestand
                using (var writer = new StreamWriter(path))
                {
                    //schrijf string weg naar streamwriter
                    writer.Write(bestandsinhoud);

                    Console.WriteLine("opgeslagen");
                }
            }
            //vang errors af en geef een melding dat er iets is fout gegaan
            catch
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

        private void PresentatieVoortgangCallback(int lijstTotaal, int bijItem, float individueleVoortgang)
        {
            var actie = new Action(() =>
            {
                progressBar1.Minimum = 0;
                progressBar1.Maximum = lijstTotaal * 100;
                progressBar1.Value = bijItem * 100 + (int)(individueleVoortgang * 100);
                progressBar1.Refresh();
            });
            Invoke(actie);
        }
        private void PresentatieGereedmeldingCallback(string opgeslagenAlsBestand = null, string foutmelding = null, int? slidesGemist = null)
        {
            var actie = new Action(() =>
            {
                button1.Text = "Maak slides";
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
