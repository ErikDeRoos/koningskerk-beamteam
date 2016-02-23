﻿using RemoteGenerator.Builder.Wachtrij.LiturgieRegels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteGenerator.Builder.Wachtrij
{
    class Liturgie
    {
        public IEnumerable<LiturgieRegel> LiturgieRegels { get; set; }
        public string Voorganger { get; set; }
        public string Collecte1 { get; set; }
        public string Collecte2 { get; set; }
        public string Lezen { get; set; }
        public string Tekst { get; set; }

        public Liturgie(ConnectTools.Berichten.Liturgie vanLiturgie, Func<ConnectTools.Berichten.StreamToken, BestandStreamToken> bestandStreamTokenFactory)
        {
            LiturgieRegels = vanLiturgie.Regels.OrderBy(r => r.Index).Select(r => new LiturgieRegel(r, bestandStreamTokenFactory)).ToList(); ;

        }
    }
}