// Copyright 2017 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface ILiturgieDatabase
    {
        IOplossing ZoekOnderdeel(string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null);
        IOplossing ZoekOnderdeel(VerwerkingType alsType, string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen = null);
        IEnumerable<IZoekresultaat> KrijgAlleOnderdelen();
        IEnumerable<IZoekresultaat> KrijgAlleFragmenten(string onderdeelNaam);
    }
}
