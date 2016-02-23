using System.Collections.Generic;
using System.IO;

namespace ConnectTools.Berichten
{
    public class Instellingen
    {
        public int Regelsperslide { get; set; }
        public StandaardTeksten StandaardTeksten { get; set; }
        public Stream TemplateThemeBestand { get; set; }
        public Stream TemplateLiederenBestand { get; set; }

    }
}
