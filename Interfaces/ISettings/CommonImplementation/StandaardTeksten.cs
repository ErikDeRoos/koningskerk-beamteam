﻿// Copyright 2016 door Remco Veurink en Erik de Roos

namespace ISettings.CommonImplementation
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
        public string LiturgieLezen { get; set; }
        public string LiturgieTekst { get; set; }
    }
}
