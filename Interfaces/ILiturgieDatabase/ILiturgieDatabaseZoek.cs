// Copyright 2019 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface ILiturgieDatabaseZoek
    {
        IEnumerable<IZoekresultaat> KrijgAlleSetNamenInNormaleDb();
        IEnumerable<IZoekresultaat> KrijgAlleSetNamenInBijbelDb();
        IEnumerable<IZoekresultaat> KrijgAlleSetNamen();
        IEnumerable<IZoekresultaat> KrijgAlleFragmentenUitAlleDatabases(string setNaam);
        IEnumerable<IZoekresultaat> KrijgAlleFragmentenUitNormaleDb(string setNaam);
        IEnumerable<IZoekresultaat> KrijgAlleFragmentenUitBijbelDb(string setNaam);
    }
}
