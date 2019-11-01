// Copyright 2019 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface IOplossing
    {
        DatabaseZoekStatus Status { get; }
        OplossingOnderdeel Onderdeel { get; }
        OplossingOnderdeel Fragment { get; }
        IEnumerable<ILiturgieContent> Content { get; }
        bool ZonderContentSplitsing { get; }
        bool? StandaardNietTonenInLiturgie { get; }
    }

    public class OplossingOnderdeel
    {
        public string VeiligeNaam { get; set; }
        public string OrigineleNaam { get; set; }
        public string DisplayNaam { get; set; }
    }
}
