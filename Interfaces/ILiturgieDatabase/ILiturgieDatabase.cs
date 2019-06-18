// Copyright 2019 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface ILiturgieDatabase
    {
        IOplossing ZoekOnderdeel(string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen, ILiturgieSettings settings);
        IOplossing ZoekOnderdeel(VerwerkingType alsType, string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen, ILiturgieSettings settings);
        IEnumerable<IZoekresultaat> KrijgOnderdeelDefault();
        IEnumerable<IZoekresultaat> KrijgOnderdeelBijbel();
        IEnumerable<IZoekresultaat> KrijgAlleOnderdelen();
        IEnumerable<IZoekresultaat> KrijgAlleFragmenten(string onderdeelNaam);
    }
}
