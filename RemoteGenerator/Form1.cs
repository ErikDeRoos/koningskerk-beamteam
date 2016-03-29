// Copyright 2016 door Erik de Roos
using RemoteGenerator.Builder;
using System;
using System.Linq;
using System.Windows.Forms;

namespace RemoteGenerator
{
    internal partial class Form1 : Form
    {
        private readonly Func<IPpGenerator> _ippGeneratorResolver;

        public Form1(Func<IPpGenerator> ippGeneratorResolver, string startBestand)
        {
            _ippGeneratorResolver = ippGeneratorResolver;
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var generator = _ippGeneratorResolver();
            label2.Text = generator.Wachtrij.Count().ToString();
            label3.Text = generator.Verwerkt.Count().ToString();
        }
    }
}
