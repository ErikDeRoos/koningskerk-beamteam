﻿using Generator.Database;
using Generator.Database.FileSystem;
using Generator.LiturgieInterpretator;
using Generator.LiturgieInterpretator.Models;
using Moq;
using System.Linq;

namespace Generator.Tests.Builders
{
    public class ZoekresultaatBuilder
    {
        public ILiturgieDatabaseZoek LiturgieDatabase { get; private set; }
        public ILiturgieTekstNaarObject LiturgieTekstNaarObject => new LiturgieTekstNaarObject();
        public LiturgieDatabaseZoekBuilder _dbBuilder = new LiturgieDatabaseZoekBuilder();

        private static readonly ZoekresultaatItem[] basisZoekresultaat = new[] 
        {
            new ZoekresultaatItem
            {
                OnderdeelNaam = "Sela",
            },
            new ZoekresultaatItem
            {
                OnderdeelNaam = "Sela",
                FragmentNaam = "ik zal er zijn",
            },
            new ZoekresultaatItem
            {
                OnderdeelNaam = "Sela",
                FragmentNaam = "de doop",
            },
            new ZoekresultaatItem
            {
                OnderdeelNaam = "Sela",
                FragmentNaam = "zegen jou",
            },
            new ZoekresultaatItem
            {
                OnderdeelNaam = "1 Petrus",
            },
            new ZoekresultaatItem
            {
                OnderdeelNaam = "1 Petrus",
                FragmentNaam = "1",
            }
        };

        public int AantalSets { get; private set; }
        public int AantalFragmenten { get; private set; }

        public IVrijZoekresultaat BuildDefault()
        {
            Build();
            return MockZoekresultaat(basisZoekresultaat);
        }

        public ZoekresultaatBuilder AddKrijgAlleSetNamen()
        {
            var sets = basisZoekresultaat.GroupBy(b => b.OnderdeelVeiligeNaam).ToList();

            foreach (var setGroup in sets)
            {
                var item = setGroup.First();
                _dbBuilder.KrijgAlleSetNamenInNormaleDb_AddOnderdeel(item.OnderdeelNaam, item.OnderdeelVeiligeNaam);
            }
            AantalSets = sets.Count;

            return this;
        }

        public ZoekresultaatBuilder AddKrijgAlleFragmentenUitSet(string predefinedSetNaam)
        {
            var sets = basisZoekresultaat.GroupBy(b => b.OnderdeelVeiligeNaam)
                .Where(sg => string.Compare(sg.Key, predefinedSetNaam, true) == 0)
                .First();

            foreach (var item in sets)
            {
                _dbBuilder.KrijgAlleFragmentenUitSet_AddFragment(item.OnderdeelNaam, item.FragmentNaam, item.OnderdeelVeiligeNaam, item.FragmentVeiligeNaam);
            }
            AantalFragmenten = sets.Count();

            return this;
        }

        private void Build()
        {
            LiturgieDatabase = _dbBuilder.Build();
        }

        private static IVrijZoekresultaat MockZoekresultaat(ZoekresultaatItem[] zoekresultaten)
        {
            var zoekresultaat = new Mock<IVrijZoekresultaat>();
            zoekresultaat.SetupGet(x => x.ZoekTerm).Returns("");
            zoekresultaat.SetupGet(x => x.AlleMogelijkheden).Returns(zoekresultaten);
            return zoekresultaat.Object;
        }


        private class ZoekresultaatItem : IVrijZoekresultaatMogelijkheid
        {
            public string Weergave { get { return $"{OnderdeelNaam} {FragmentNaam}".Trim(); } }
            public string VeiligeNaam { get { return $"{OnderdeelVeiligeNaam} {FragmentVeiligeNaam}".Trim(); } }
            public string UitDatabase { get; set; }

            public string OnderdeelNaam { get; set; }
            public string OnderdeelVeiligeNaam { get { return FileEngineDefaults.CreateSafeName(OnderdeelNaam); } }
            public string FragmentNaam { get; set; }
            public string FragmentVeiligeNaam { get { return FileEngineDefaults.CreateSafeName(FragmentNaam); } }

            public override bool Equals(object obj)
            {
                if (obj is IVrijZoekresultaatMogelijkheid vrijZoekresultaat)
                    return Equals(this, vrijZoekresultaat);

                return base.Equals(obj);
            }

            public bool Equals(IVrijZoekresultaatMogelijkheid x, IVrijZoekresultaatMogelijkheid y)
            {
                if (x == null || y == null)
                    return false;
                return x.Weergave.Equals(y.Weergave, System.StringComparison.InvariantCultureIgnoreCase);  // Alleen equals checks op weergave naam
            }

            public override string ToString()
            {
                return Weergave;
            }
        }
    }
}
