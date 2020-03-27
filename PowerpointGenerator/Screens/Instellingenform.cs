// Copyright 2018 door Remco Veurink en Erik de Roos

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ISettings;
using ISettings.CommonImplementation;

namespace PowerpointGenerator.Screens
{
    partial class Instellingenform : Form
    {
        private readonly IInstellingenFactory _instellingenFactory;

        public IInstellingen Instellingen { get; private set; }
        private IEnumerable<IMapmask> _masks; 

        public Instellingenform(IInstellingenFactory instellingenFactory)
        {
            _instellingenFactory = instellingenFactory;
            InitializeComponent();
        }

        public void Opstarten()
        {
            var vanInstellingen = _instellingenFactory.LoadFromFile();

            textBox3.Text = vanInstellingen.DatabasePad;
            textBox5.Text = vanInstellingen.BijbelPad;
            textBox2.Text = vanInstellingen.TemplateTheme;
            textBox1.Text = vanInstellingen.TemplateLied;
            textBox6.Text = vanInstellingen.TemplateBijbeltekst;
            textBox4.Text = vanInstellingen.RegelsPerLiedSlide.ToString();
            textBox7.Text = vanInstellingen.RegelsPerBijbeltekstSlide.ToString();
            textBox8.Text = vanInstellingen.TekstFontName;
            textBox9.Text = vanInstellingen.TekstFontPointSize.ToString();
            textBox10.Text = vanInstellingen.TekstChar_a_OnARow.ToString();

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

            checkBox1.Checked = vanInstellingen.Een2eCollecte;
            checkBox2.Checked = vanInstellingen.DeLezenVraag;
            checkBox3.Checked = vanInstellingen.DeTekstVraag;
            checkBox4.Checked = vanInstellingen.GebruikDisplayNameVoorZoeken;
            checkBox5.Checked = vanInstellingen.ToonBijbeltekstenInLiturgie;
            checkBox6.Checked = vanInstellingen.ToonGeenVersenBijVolledigeContent;
            checkBox7.Checked = vanInstellingen.VersOnderbrekingOverSlidesHeen;
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

        private void button7_Click(object sender, EventArgs e)
        {
            //kies een bestand en sla het pad op
            var temp = KiesFile();
            if (!temp.Equals(""))
                textBox6.Text = temp;
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

        private void button6_Click(object sender, EventArgs e)
        {
            var applicationPath = AppDomain.CurrentDomain.BaseDirectory;

            //open een open window met bepaalde instellingen
            var openFolderDialog1 = new FolderBrowserDialog
            {
                Description = "Kies map van de Bijbel Database",
                SelectedPath = applicationPath
            };

            //return als er word geannuleerd
            if (openFolderDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            var dbPath = openFolderDialog1.SelectedPath;
            if (dbPath.StartsWith(applicationPath))
                dbPath = dbPath.Replace(applicationPath, ".");

            textBox5.Text = dbPath;
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
            var instellingen = new Instellingen(
                standaardTeksten: new StandaardTeksten
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
                masks: _masks
            )
            {
                DatabasePad = textBox3.Text,
                BijbelPad = textBox5.Text,
                TemplateTheme = textBox2.Text,
                TemplateLied = textBox1.Text,
                TemplateBijbeltekst = textBox6.Text,
                TekstFontName = textBox8.Text,
            };
            if (Int32.TryParse(textBox4.Text, out int regelsPerSlide))
                instellingen.RegelsPerLiedSlide = regelsPerSlide;
            if (Int32.TryParse(textBox7.Text, out int regelsPerBijbeltekstSlide))
                instellingen.RegelsPerBijbeltekstSlide = regelsPerBijbeltekstSlide;
            if (Int32.TryParse(textBox9.Text, out int fontPointSize))
                instellingen.TekstFontPointSize = fontPointSize;
            if (Int32.TryParse(textBox10.Text, out int char_a_OnARow))
                instellingen.TekstChar_a_OnARow = char_a_OnARow;
            instellingen.Een2eCollecte = checkBox1.Checked;
            instellingen.DeLezenVraag = checkBox2.Checked;
            instellingen.DeTekstVraag = checkBox3.Checked;
            instellingen.GebruikDisplayNameVoorZoeken = checkBox4.Checked;
            instellingen.ToonBijbeltekstenInLiturgie = checkBox5.Checked;
            instellingen.ToonGeenVersenBijVolledigeContent = checkBox6.Checked;
            instellingen.VersOnderbrekingOverSlidesHeen = checkBox7.Checked;

            Instellingen = instellingen;
        }
    }
}
