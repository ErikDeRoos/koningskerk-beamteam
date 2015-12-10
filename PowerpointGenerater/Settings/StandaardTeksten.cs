using ISettings;

namespace PowerpointGenerater
{
    public class StandaardTeksten : IStandaardTeksten
    {
        public string Volgende { get; set; }
        public string Voorganger { get; set; }
        public string Collecte1 { get; set; }
        public string Collecte2 { get; set; }
        public string Collecte { get; set; }
        public string Lezen { get; set; }
        public string Tekst { get; set; }
        public string Liturgie { get; set; }
    }
}
