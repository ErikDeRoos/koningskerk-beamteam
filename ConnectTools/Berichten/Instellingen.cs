using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectTools.Berichten
{
    public class Instellingen
    {
        string Databasepad { get; set; }
        string Templateliederen { get; set; }
        string Templatetheme { get; set; }
        int Regelsperslide { get; set; }
        StandaardTeksten StandaardTeksten { get; set; }

        IEnumerable<Mask> Masks { get; set; }

        string FullDatabasePath { get; set; }

        string FullTemplatetheme { get; set; }

        string FullTemplateliederen { get; set; }
    }
}
