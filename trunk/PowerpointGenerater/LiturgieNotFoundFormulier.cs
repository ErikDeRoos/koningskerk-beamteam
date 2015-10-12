using System.Windows.Forms;

namespace PowerpointGenerater
{
    public partial class LiturgieNotFoundFormulier : Form
    {
        public LiturgieNotFoundFormulier(string liturgieregel)
        {
            InitializeComponent();
            this.textBox1.Text = liturgieregel;
        }
    }
}
