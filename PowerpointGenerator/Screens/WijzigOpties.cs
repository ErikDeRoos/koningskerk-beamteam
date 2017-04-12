using ILiturgieDatabase;
using System.Windows.Forms;
using System;

namespace PowerpointGenerator.Screens
{
    public partial class WijzigOpties : Form
    {
        public WijzigOpties()
        {
            InitializeComponent();
        }

        public void Initialise(ILiturgieOptiesGebruiker geactiveerdeOpties)
        {
            if (geactiveerdeOpties == null)
                return;
            checkBox1.Checked = geactiveerdeOpties.AlsBijbeltekst;
            checkBox2.Checked = geactiveerdeOpties.NietVerwerkenViaDatabase;
            checkBox3.Checked = geactiveerdeOpties.ToonInOverzicht;
            textBox1.Text = geactiveerdeOpties.AlternatieveNaamOverzicht;
            checkBox4.Checked = geactiveerdeOpties.ToonInVolgende;
            textBox2.Text = geactiveerdeOpties.AlternatieveNaam;
        }

        public ILiturgieOptiesGebruiker GetOpties()
        {
            return new LiturgieOpties()
            {
                AlsBijbeltekst = checkBox1.Checked,
                NietVerwerkenViaDatabase = checkBox2.Checked,
                ToonInOverzicht = checkBox3.Checked,
                AlternatieveNaamOverzicht = textBox1.Text,
                ToonInVolgende = checkBox4.Checked,
                AlternatieveNaam = textBox2.Text,
            };
        }
    }

    public class LiturgieOpties : ILiturgieOptiesGebruiker
    {
        public bool AlsBijbeltekst { get; set; }

        public bool NietVerwerkenViaDatabase { get; set; }

        public bool ToonInOverzicht { get; set; }

        public string AlternatieveNaamOverzicht { get; set; }

        public string AlternatieveNaam { get; set; }

        public bool ToonInVolgende { get; set; }
    }
}
