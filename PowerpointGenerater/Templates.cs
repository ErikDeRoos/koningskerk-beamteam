using System;
using System.Windows.Forms;

namespace PowerpointGenerater
{
    public partial class Templates : Form
    {
        Form1 hoofdformulier;
        public Templates(Form1 formulier)
        {
            InitializeComponent();
            textBox1.Text = formulier.instellingen.Templateliederen;
            textBox2.Text = formulier.instellingen.Templatetheme;
            textBox3.Text = formulier.instellingen.Databasepad;
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
            //open een open window met bepaalde instellingen
            FolderBrowserDialog openFolderDialog1 = new FolderBrowserDialog();
            openFolderDialog1.Description = "Kies map van de Database";

            //return als er word geannuleerd
            if (openFolderDialog1.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            textBox3.Text = openFolderDialog1.SelectedPath;
        }
        #endregion Eventhandlers
        #region Functions
        /// <summary>
        /// Uitkiezen van een file aan de hand van openfiledialog
        /// </summary>
        /// <returns> return gekozen bestandspad</returns>
        private String KiesFile()
        {
            //open een open window met bepaalde instellingen
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Template bestanden|*.pptx;*.potx";
            openFileDialog1.Title = "Kies bestand";

            //return als er word geannuleerd
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return "";
            //return bestandspad
            return openFileDialog1.FileName;
        }
        #endregion Functions
    }
}
