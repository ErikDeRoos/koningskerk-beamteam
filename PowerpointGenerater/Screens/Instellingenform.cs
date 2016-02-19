using ISettings;
using ISettings.CommonImplementation;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static System.Int32;

namespace PowerpointGenerater
{
    partial class Instellingenform : Form
    {
        [Dependency]
        public IInstellingenFactory InstellingenFactory { get; set; }

        public IInstellingen Instellingen { get; private set; }
        private IEnumerable<IMapmask> _masks; 

        public Instellingenform()
        {
            InitializeComponent();
        }

        public void Opstarten()
        {
            var vanInstellingen = InstellingenFactory.LoadFromXmlFile();

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
            tbLiturgieLezen.Text = vanInstellingen.StandaardTeksten.LiturgieLezen;
            tbLiturgieTekst.Text = vanInstellingen.StandaardTeksten.LiturgieTekst;

            _masks = vanInstellingen.Masks;
        }

        #region Eventhandlers
        private void button1_Click(object sender, EventArgs e)
        {
            //kies een bestand en sla het pad op
            var temp = KiesFile();
            if (!temp.Equals(""))
                textBox1.Text = temp;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //kies een bestand en sla het pad op
            var temp = KiesFile();
            if (!temp.Equals(""))
                textBox2.Text = temp;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var applicationPath = AppDomain.CurrentDomain.BaseDirectory;

            //open een open window met bepaalde instellingen
            var openFolderDialog1 = new FolderBrowserDialog
            {
                Description = "Kies map van de Database",
                SelectedPath = applicationPath
            };

            //return als er word geannuleerd
            if (openFolderDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            var dbPath = openFolderDialog1.SelectedPath;
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
        private static string KiesFile()
        {
            var applicationPath = AppDomain.CurrentDomain.BaseDirectory;

            //open een open window met bepaalde instellingen
            var openFileDialog1 = new OpenFileDialog
            {
                Filter = "Template bestanden|*.pptx;*.potx",
                Title = "Kies bestand",
                InitialDirectory = applicationPath
            };

            //return als er word geannuleerd
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return "";
            //return bestandspad
            var selectedPath = openFileDialog1.FileName;
            if (selectedPath.StartsWith(applicationPath))
                selectedPath = selectedPath.Replace(applicationPath, ".");

            return selectedPath;
        }
        #endregion Functions

        private void button2_Click(object sender, EventArgs e)
        {
            int regelsPerSlide;
            if (!TryParse(textBox4.Text, out regelsPerSlide))
                regelsPerSlide = 6;
            Instellingen = new Instellingen(textBox3.Text, textBox1.Text, textBox2.Text, regelsPerSlide,
                new StandaardTeksten
                {
                    Volgende = tbVolgende.Text,
                    Voorganger = tbVoorganger.Text,
                    Collecte = tbCollecte.Text,
                    Collecte1 = tbCollecte1.Text,
                    Collecte2 = tbCollecte2.Text,
                    Lezen = tbLezen.Text,
                    Tekst = tbTekst.Text,
                    Liturgie = tbLiturgie.Text,
                    LiturgieLezen = tbLiturgieLezen.Text,
                    LiturgieTekst = tbLiturgieTekst.Text
                },
                _masks
            );
        }
    }
}
