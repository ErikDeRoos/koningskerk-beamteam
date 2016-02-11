using Microsoft.Practices.Unity;
using System;
using System.Windows.Forms;

namespace PowerpointGenerater
{

    //TODO: de volgorde in de liturgie bijbellezingen op de juiste plek;
    //TODO: de lay-out van het programma
    //TODO: het mogelijk maken om in het programma te kunnen werken met bijvoorbeeld “rechtmuisknop, plakken”


    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var di = new DefaultContainer();
            di.RegisterAll();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var startupForm = new Form1();
            di.BuildUp(startupForm);
            startupForm.Opstarten(args.Length >= 1 ? args[0] : null);

            Application.Run(startupForm);
        }
    }
}
