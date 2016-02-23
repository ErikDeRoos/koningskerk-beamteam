using RemoteGenerator.Builder.Wachtrij;
using System;
using System.Collections.Generic;

namespace RemoteGenerator.Builder
{
    class WachtrijRegel
    {
        public ConnectTools.Berichten.Token Token { get; set; }
        public int Index { get; set; }
        public Liturgie Liturgie { get; set; }
        public Instellingen Instellingen { get; set; }
        public IEnumerable<BestandStreamToken> Bestanden { get; set; }
        public ConnectTools.Berichten.Voortgang Voortgang { get; set; }
        public string ResultaatOpgeslagenOp { get; set; }
        public DateTime ToegevoegdOp { get; set; }
    }
}
