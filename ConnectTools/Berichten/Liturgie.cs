using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectTools.Berichten
{
    public class Liturgie
    {
        public IEnumerable<LiturgieRegel> Regels { get; set; }
        public string Voorganger { get; set; }
        public string Collecte1 { get; set; }
        public string Collecte2 { get; set; }
        public string Lezen { get; set; }
        public string Tekst { get; set; }
        public byte[] TemplateTheme { get; set; }
        public byte[] TemplateLiederen { get; set; }
    }
}
