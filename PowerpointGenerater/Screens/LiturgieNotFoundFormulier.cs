using System.Windows.Forms;

namespace PowerpointGenerater
{
    public partial class LiturgieNotFoundFormulier : Form
    {
        public LiturgieNotFoundFormulier(string liturgieregel)
        {
            InitializeComponent();
            textBox1.Text = liturgieregel;
        }
    }
}
