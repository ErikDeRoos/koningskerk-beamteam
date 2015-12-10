using ISettings;
using PowerpointGenerater.AppFlow;
using System;
using System.Windows.Forms;

namespace PowerpointGenerater
{
    public partial class Instellingenform : SettingsForm
    {
        public Instellingenform()
        {
            InitializeComponent();
        }

        public override void Opstarten(IInstellingen vanInstellingen)
        {
            textBox1.Text = vanInstellingen.Templateliederen;
            textBox2.Text = vanInstellingen.Templatetheme;
            textBox3.Text = vanInstellingen.Databasepad;
            textBox4.Text = vanInstellingen.Regelsperslide.ToString();

            tbVolgende.Text = vanInstellingen.StandaardTeksten.Volgende;
            tbVoorganger.Text = vanInstellingen.StandaardTeksten.Voorganger;
            tbCollecte.Text = vanInstellingen.StandaardTeksten.Collecte;
            tbCollecte1.Text = vanInstellingen.StandaardTeksten.Collecte1;
            tbCollecte2.Text = vanInstellingen.StandaardTeksten.Collecte1;
            tbLezen.Text = vanInstellingen.StandaardTeksten.Lezen;
            tbTekst.Text = vanInstellingen.StandaardTeksten.Tekst;
            tbLiturgie.Text = vanInstellingen.StandaardTeksten.Liturgie;
        }

        #region Eventhandlers
        private void button1_Click(object sender, EventArgs e)
        {
            //kies een bestand en sla het pad op
            string temp = KiesFile();
            if (!temp.Equals(""))
                textBox1.Text = temp;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //kies een bestand en sla het pad op
            string temp = KiesFile();
            if (!temp.Equals(""))
                textBox2.Text = temp;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string applicationPath = AppDomain.CurrentDomain.BaseDirectory;

            //open een open window met bepaalde instellingen
            FolderBrowserDialog openFolderDialog1 = new FolderBrowserDialog();
            openFolderDialog1.Description = "Kies map van de Database";
            openFolderDialog1.SelectedPath = applicationPath;

            //return als er word geannuleerd
            if (openFolderDialog1.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            string dbPath = openFolderDialog1.SelectedPath;
            if (dbPath.StartsWith(applicationPath))
                dbPath = dbPath.Replace(applicationPath, ".");

            textBox3.Text = dbPath;
        }
        #endregion Eventhandlers

        #region Functions
        /// <summary>
        /// Uitkiezen van een file aan de hand van openfiledialog
        /// </summary>
        /// <returns> return gekozen bestandspad</returns>
        private string KiesFile()
        {
            string applicationPath = AppDomain.CurrentDomain.BaseDirectory;

            //open een open window met bepaalde instellingen
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Template bestanden|*.pptx;*.potx";
            openFileDialog1.Title = "Kies bestand";
            openFileDialog1.InitialDirectory = applicationPath;

            //return als er word geannuleerd
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return "";
            //return bestandspad
            string selectedPath = openFileDialog1.FileName;
            if (selectedPath.StartsWith(applicationPath))
                selectedPath = selectedPath.Replace(applicationPath, ".");

            return selectedPath;
        }
        #endregion Functions

        private void button2_Click(object sender, EventArgs e)
        {
            int regelsPerSlide = 0;
            if (!System.Int32.TryParse(textBox4.Text, out regelsPerSlide))
                regelsPerSlide = 6;
            Instellingen = new Instellingen(textBox3.Text, textBox3.Text, textBox2.Text, regelsPerSlide,
                new StandaardTeksten() { 
                    Volgende = tbVolgende.Text,
                    Voorganger = tbVoorganger.Text,
                    Collecte = tbCollecte.Text,
                    Collecte1 = tbCollecte1.Text,
                    Collecte2 = tbCollecte2.Text,
                    Lezen = tbLezen.Text,
                    Tekst = tbTekst.Text,
                    Liturgie = tbLiturgie.Text,
                }
            );
        }
    }
}
