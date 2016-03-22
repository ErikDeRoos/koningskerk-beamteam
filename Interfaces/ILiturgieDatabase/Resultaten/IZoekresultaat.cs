using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface IZoekresultaat
    {
        LiturgieOplossingResultaat? Fout { get; }
        string OnderdeelNaam { get; }
        string OnderdeelDisplayNaam { get; }
        string FragmentNaam { get; }
        IEnumerable<ILiturgieContent> Content { get; }
        bool ZonderContentSplitsing { get; }
    }
}
