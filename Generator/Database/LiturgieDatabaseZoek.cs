// Copyright 2019 door Erik de Roos
using IDatabase;
using ILiturgieDatabase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Generator.Database
{
    /// <summary>
    /// Zoek naar de opgegeven liturgieen.
    /// </summary>
    public class LiturgieDatabaseZoek : ILiturgieDatabaseZoek
    {
        private readonly IEngineManager _databases;
        public LiturgieDatabaseZoek(IEngineManager database)
        {
            _databases = database;
        }

        public IEnumerable<IZoekresultaat> KrijgAlleSetNamen()
        {
            return KrijgResultatenUitEngine(_databases.Extensions);
        }
        public IEnumerable<IZoekresultaat> KrijgAlleSetNamenInNormaleDb()
        {
            return KrijgResultatenUitEngine(_databases.Extensions.Where(e => e.Name == LiturgieDatabaseSettings.DatabaseNameDefault));
        }
        public IEnumerable<IZoekresultaat> KrijgAlleSetNamenInBijbelDb()
        {
            return KrijgResultatenUitEngine(_databases.Extensions.Where(e => e.Name == LiturgieDatabaseSettings.DatabaseNameBijbeltekst));
        }

        public IEnumerable<IZoekresultaat> KrijgAlleFragmentenUitAlleDatabases(string setNaam)
        {
            return _databases.Extensions.SelectMany(de => KrijgResultatenUitEngine(de, setNaam));
        }
        public IEnumerable<IZoekresultaat> KrijgAlleFragmentenUitNormaleDb(string setNaam)
        {
            return _databases.Extensions
                .Where(e => e.Name == LiturgieDatabaseSettings.DatabaseNameDefault)
                .SelectMany(de => KrijgResultatenUitEngine(de, setNaam));
        }
        public IEnumerable<IZoekresultaat> KrijgAlleFragmentenUitBijbelDb(string setNaam)
        {
            return _databases.Extensions
                .Where(e => e.Name == LiturgieDatabaseSettings.DatabaseNameBijbeltekst)
                .SelectMany(de => KrijgResultatenUitEngine(de, setNaam));
        }

        private static IEnumerable<Zoekresultaat> KrijgResultatenUitEngine(IEnumerable<IDatabase.Engine.IEngineSelection> engineSet)
        {
            return engineSet
                .SelectMany(de => de.Engine.GetAllNames().Select(n => new Zoekresultaat(de.Name, n.Name, n.SafeName)));
        }
        private static IEnumerable<Zoekresultaat> KrijgResultatenUitEngine(IDatabase.Engine.IEngineSelection engineSet, string setNaam)
        {
            return engineSet.Engine
                .Where(s => string.Equals(s.Name.SafeName, setNaam, StringComparison.CurrentCultureIgnoreCase) || string.Equals(s.Settings.DisplayName, setNaam, StringComparison.CurrentCultureIgnoreCase))
                .SelectMany(set => set.GetAllNames().Select(n => new Zoekresultaat(engineSet.Name, n.Name, n.SafeName)));
        }

        private class Zoekresultaat : IZoekresultaat
        {
            public IZoekresultaatEntry Resultaat { get; }
            public IZoekresultaatBron Database { get; }

            public Zoekresultaat(string bron, string itemWeergave, string itemVeiligeNaam)
            {
                Database = new ZoekresultaatBron { Weergave = bron };
                Resultaat = new ZoekresultaatItem { Weergave = itemWeergave, VeiligeNaam = itemVeiligeNaam };
            }

            private class ZoekresultaatItem : IZoekresultaatEntry
            {
                public string Weergave { get; set; }
                public string VeiligeNaam { get; set; }

            }

            private class ZoekresultaatBron : IZoekresultaatBron
            {
                public string Weergave { get; set; }
            }
        }
    }
}
