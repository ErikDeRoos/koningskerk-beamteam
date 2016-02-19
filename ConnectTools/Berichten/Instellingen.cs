using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectTools.Berichten
{
    public class Instellingen
    {
        int Regelsperslide { get; set; }
        StandaardTeksten StandaardTeksten { get; set; }

        IEnumerable<Mask> Masks { get; set; }

    }
}
