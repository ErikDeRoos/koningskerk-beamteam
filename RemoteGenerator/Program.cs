using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Windows.Forms;

namespace RemoteGenerator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var container = new UnityContainer().LoadConfiguration();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var startupForm = new Form1();
            container.BuildUp(startupForm);
            startupForm.Opstarten(args.Length >= 1 ? args[0] : null);

            Application.Run(startupForm);
        }
    }
}
