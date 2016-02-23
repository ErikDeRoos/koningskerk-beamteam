using Microsoft.Practices.Unity;
using RemoteGenerator.Builder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteGenerator
{
    public partial class Form1 : Form
    {
        [Dependency]
        public IUnityContainer DI { get; set; }

        public Form1()
        {
            InitializeComponent();
        }
        public void Opstarten(string startBestand = null)
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var generator = DI.Resolve<IPpGenerator>();
            label2.Text = generator.Wachtrij.Count().ToString();
            label3.Text = generator.Verwerkt.Count().ToString();
        }
    }
}
