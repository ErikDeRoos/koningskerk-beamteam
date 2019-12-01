// Copyright 2019 door Erik de Roos
using System.Collections.Generic;

namespace Generator.Database.Models
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
        public string Naam { get; set; }
        public string AlternatieveNaam { get; set; }
    }
}
