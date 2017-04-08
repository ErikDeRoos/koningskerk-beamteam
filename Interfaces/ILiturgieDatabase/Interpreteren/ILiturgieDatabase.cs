// Copyright 2017 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface ILiturgieDatabase
    {
        IZoekresultaat ZoekOnderdeel(string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null);
        IZoekresultaat ZoekOnderdeel(VerwerkingType alsType, string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null);
        IEnumerable<string> KrijgAlleOnderdelen();
        IEnumerable<string> KrijgAlleFragmenten(string onderdeelNaam);
    }
}
