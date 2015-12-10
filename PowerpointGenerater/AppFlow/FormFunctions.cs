using ISettings;
using System.Windows.Forms;

namespace PowerpointGenerater.AppFlow
{
    public abstract class MainForm : Form
    {
        public abstract void Opstarten(string fileName = null);
    }
    public abstract class SettingsForm : Form
    {
        public Instellingen Instellingen { get; protected set; }
        public abstract void Opstarten(IInstellingen vanInstellingen);
    }
}
