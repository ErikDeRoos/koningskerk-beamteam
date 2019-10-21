// Copyright 2019 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface ILiturgieDatabase
    {
        IOplossing ZoekSpecifiek(string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen, ILiturgieSettings settings);
        IOplossing ZoekSpecifiek(VerwerkingType alsType, string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen, ILiturgieSettings settings);
        IEnumerable<IZoekresultaat> ZoekGeneriekOnderdeelDefault();
        IEnumerable<IZoekresultaat> ZoekGeneriekOnderdeelBijbel();
        IEnumerable<IZoekresultaat> ZoekGeneriekAlleOnderdelen();
        IEnumerable<IZoekresultaat> ZoekGeneriekAlleFragmenten(string onderdeelNaam);
    }
}
