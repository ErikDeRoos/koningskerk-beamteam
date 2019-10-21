// Copyright 2016 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface IOplossing
    {
        LiturgieOplossingResultaat Status { get; }
        IOplossingOnderdeel Onderdeel { get; }
        IOplossingOnderdeel Fragment { get; }
        IEnumerable<ILiturgieContent> Content { get; }
        bool ZonderContentSplitsing { get; }
        bool? StandaardNietTonenInLiturgie { get; }
    }

    public interface IOplossingOnderdeel
    {
        string VeiligeNaam { get; }
        string OrigineleNaam { get; }
        string DisplayNaam { get; }
    }
}
