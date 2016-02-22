using ConnectTools.Berichten;
using System;

namespace RemoteGenerator.Builder
{
    class WachtrijRegel
    {
        public Token Token { get; set; }
        public int Index { get; set; }
        public Liturgie Liturgie { get; set; }
        public Instellingen Instellingen { get; set; }
        public Voortgang Voortgang { get; set; }
        public string ResultaatOpgeslagenOp { get; set; }
        public DateTime ToegevoegdOp { get; set; }
    }
}
