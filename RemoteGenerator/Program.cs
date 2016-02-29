using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using RemoteGenerator.WCF;
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
            var container = new UnityContainer();
            IocConfig.SetDefault(container);
            container.LoadConfiguration();  // Overschijf default

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var startupForm = new Form1();
            container.BuildUp(startupForm);
            startupForm.Opstarten(args.Length >= 1 ? args[0] : null);

            Host.DI = container;  // Dirty, maar WCF ondersteunt geen DI
            container.Resolve<IHost>().Start();
            Application.Run(startupForm);
            container.Resolve<IHost>().Stop();
        }
    }
}
