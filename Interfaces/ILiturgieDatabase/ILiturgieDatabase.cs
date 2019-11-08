// Copyright 2019 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface ILiturgieDatabase
    {
        IOplossing ZoekSpecifiekItem(VerwerkingType alsType, string onderdeelNaam, string fragmentNaam, IEnumerable<string> fragmentDelen, LiturgieSettings settings);
        IEnumerable<IZoekresultaat> KrijgAlleSetNamenInNormaleDb();
        IEnumerable<IZoekresultaat> KrijgAlleSetNamenInBijbelDb();
        IEnumerable<IZoekresultaat> KrijgAlleSetNamen();
        IEnumerable<IZoekresultaat> KrijgAlleFragmentenUitAlleDatabases(string setNaam);
        IEnumerable<IZoekresultaat> KrijgAlleFragmentenUitNormaleDb(string setNaam);
        IEnumerable<IZoekresultaat> KrijgAlleFragmentenUitBijbelDb(string setNaam);
    }
}
