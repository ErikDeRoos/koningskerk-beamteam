using System.Windows.Forms;

namespace PowerpointGenerator
{
    public partial class Contactform : Form
    {
        public Contactform()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            Help.ShowHelp(this, "help.chm", HelpNavigator.TopicId, "20");
        }
    }
}
