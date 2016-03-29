// Copyright 2016 door Erik de Roos
using System.Collections.Generic;

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
    }
}
