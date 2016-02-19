using ConnectTools.Berichten;

namespace RemoteGenerator.Builder
{
    class WachtrijRegel
    {
        public Token Token { get; set; }
        public int Index { get; set; }
        public Liturgie Liturgie { get; set; }
        public Voortgang Voortgang { get; set; }
        public byte[] Resultaat { get; set; }
    }
}
