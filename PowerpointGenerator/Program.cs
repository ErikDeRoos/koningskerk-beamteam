// Copyright 2016 door Erik de Roos
using Autofac;
using System;
using System.Windows.Forms;
using Tools;

namespace PowerpointGenerator
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try {
                var builder = new ContainerBuilder();
                Bootstrap.SetDefault(builder);

                using (var container = builder.Build())
                {
                    // Start de main programma loop
                    new ProgramInternals(container.Resolve<Func<string, Form>>())
                        .Run(args.Length >= 1 ? args[0] : null);
                }
            }
            catch (Exception Exc)
            {
                FoutmeldingSchrijver.Log(Exc);
                throw;
            }
        }

        private class ProgramInternals
        {
            private readonly Func<string, Form> _startupFormResolver;
            public ProgramInternals(Func<string, Form> startupFormResolver)
            {
                _startupFormResolver = startupFormResolver;
            }

            public void Run(string startBestand)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var startForm = _startupFormResolver(startBestand);
                Application.Run(startForm);
            }
        }
    }
}
