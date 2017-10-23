// Copyright 2016 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface IOplossing
    {
        LiturgieOplossingResultaat Status { get; }
        string OnderdeelNaam { get; }
        string OnderdeelDisplayNaam { get; }
        string FragmentNaam { get; }
        IEnumerable<ILiturgieContent> Content { get; }
        bool ZonderContentSplitsing { get; }
    }
}
