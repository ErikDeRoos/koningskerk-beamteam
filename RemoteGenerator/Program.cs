using Autofac;
//using Autofac.Configuration;
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
            var builder = new ContainerBuilder();
            Bootstrap.SetDefault(builder);
            //container.RegisterModule(new ConfigurationModule( ConfigurationSettingsReader("mycomponents"));  // Overschijf default

            using (var container = builder.Build())
            {
                // Alles wat geen DI ondersteund nu aanmaken
                Host.StaticIoCContainer = container;

                // Start de main programma loop
                new ProgramInternals(container.Resolve<IHost>(new NamedParameter("address", "net.tcp://localhost:8000/wcfserver")), container.Resolve<Func<string, Form>>())
                    .Run(args.Length >= 1 ? args[0] : null);
            }
        }

        private class ProgramInternals {
            private readonly IHost _mainHostLoop;
            private readonly Func<string, Form> _startupFormResolver;
            public ProgramInternals(IHost mainHostLoop, Func<string, Form> startupFormResolver)
            {
                _mainHostLoop = mainHostLoop;
                _startupFormResolver = startupFormResolver;
            }

            public void Run(string startBestand)
            {
                _mainHostLoop.Start();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var startForm = _startupFormResolver(startBestand);
                Application.Run(startForm);
                _mainHostLoop.Stop();
            }
        }
    }
}
