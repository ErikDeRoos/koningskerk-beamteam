using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Windows.Forms;

namespace PowerpointGenerator
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            var container = new UnityContainer();
            IocConfig.SetDefault(container);
            container.LoadConfiguration();  // Overschijf default

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var startupForm = new Form1(container.Resolve<ILiturgieDatabase.ILiturgieLosOp>(), container.Resolve<ISettings.IInstellingenFactory>(), container.Resolve<Func<ISlideBuilder.IBuilder>>());
            startupForm.Opstarten(args.Length >= 1 ? args[0] : null);

            Application.Run(startupForm);
        }
    }
}
