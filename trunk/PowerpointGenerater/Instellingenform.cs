using System;
using System.Windows.Forms;

namespace PowerpointGenerater
{
    public partial class Instellingenform : Form
    {
        Form1 hoofdformulier;
        public Instellingenform(Form1 formulier)
        {
            InitializeComponent();
            textBox1.Text = formulier.instellingen.Templateliederen;
            textBox2.Text = formulier.instellingen.Templatetheme;
            textBox3.Text = formulier.instellingen.Databasepad;
            textBox4.Text = formulier.instellingen.Regelsperslide.ToString();

            tbVolgende.Text = formulier.instellingen.StandaardTekst.Volgende;
            tbVoorganger.Text = formulier.instellingen.StandaardTekst.Voorganger;
            tbCollecte.Text = formulier.instellingen.StandaardTekst.Collecte;
            tbCollecte1.Text = formulier.instellingen.StandaardTekst.Collecte1;
            tbCollecte2.Text = formulier.instellingen.StandaardTekst.Collecte1;
            tbLezen.Text = formulier.instellingen.StandaardTekst.Lezen;
            tbTekst.Text = formulier.instellingen.StandaardTekst.Tekst;
            tbLiturgie.Text = formulier.instellingen.StandaardTekst.Liturgie;

            hoofdformulier = formulier;
        }

        #region Eventhandlers
        private void button1_Click(object sender, EventArgs e)
        {
            //kies een bestand en sla het pad op
            String temp = KiesFile();
            if(!temp.Equals(""))
                textBox1.Text = temp;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //kies een bestand en sla het pad op
            String temp = KiesFile();
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
        private String KiesFile()
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

        
    }
}
