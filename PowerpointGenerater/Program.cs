using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PowerpointGenerater
{
    
    //TODO: de volgorde in de liturgie bijbellezingen op de juiste plek;
    //TODO: de nummering van de liederen (1/2, 2/2 etc.)
    //TODO: zwarte dia tonen
    //TODO: “1e collecte” wijzigen in “collecte”
    //TODO: het woordje “liturgie” weg boven de het liturgiebord
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(args));
        }
    }
}
