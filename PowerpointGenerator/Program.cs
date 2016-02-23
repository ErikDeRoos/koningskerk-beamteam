using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Windows.Forms;

namespace PowerpointGenerator
{

    //TODO: de volgorde in de liturgie bijbellezingen op de juiste plek;
    //TODO: de lay-out van het programma
    //TODO: het mogelijk maken om in het programma te kunnen werken met bijvoorbeeld “rechtmuisknop, plakken”


    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
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
