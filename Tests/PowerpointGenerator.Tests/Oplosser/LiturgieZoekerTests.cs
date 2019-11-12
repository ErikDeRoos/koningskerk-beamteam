// Copyright 2019 door Erik de Roos
using Generator.LiturgieInterpretator;
using Generator.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Generator.Tests
{
    public class LiturgieZoekerTests
    {
        [TestClass]
        public class MaakTotTekstMethod : LiturgieZoekerTests
        {
            [DataTestMethod]
            [DataRow("1 Petrus 1 : 1", "1_petrus 1 : 1")]
            [DataRow("1 Petrus", "1_petrus")]
            [DataRow("Sela ik zal er zijn", "sela ik_zal_er_zijn")]
            public void MaakTotTekst_werkt(string invoer, string verwachtResultaat)
            {
                var builder = new ZoekresultaatBuilder();
                var zoekresultaat = builder.BuildDefault();
                var sut = new LiturgieZoeker(null, builder.LiturgieTekstNaarObject);

                var oplossing = sut.MaakTotTekst(invoer, null, zoekresultaat);

                Assert.AreEqual(verwachtResultaat, oplossing);
            }
        }

        [TestClass]
        public class VrijZoekenMethod : LiturgieZoekerTests
        {
            [TestMethod]
            public void VrijZoeken_EersteKeerZoeken_GeefAlleSets()
            {
                const string query = "";
                var builder = new ZoekresultaatBuilder()
                    .AddKrijgAlleSetNamen();
                builder.BuildDefault();
                var sut = new LiturgieZoeker(builder.LiturgieDatabase, builder.LiturgieTekstNaarObject);

                var oplossing = sut.VrijZoeken(query);

                Assert.AreEqual(builder.AantalSets, oplossing.AlleMogelijkheden.Count());
            }

            [TestMethod]
            public void VrijZoeken_SpecifiekZoekenDeels_GeefAlleSetsPlusSpecifiekeFragmenten()
            {
                const string setToUse = "Sela";
                const string query = "Se";  // Deels zoeken op 'Sela': moet al fragmenten zoeken triggeren
                var builder = new ZoekresultaatBuilder()
                    .AddKrijgAlleSetNamen()
                    .AddKrijgAlleFragmentenUitSet(setToUse);
                var zoekresultaat = builder.BuildDefault();
                var sut = new LiturgieZoeker(builder.LiturgieDatabase, builder.LiturgieTekstNaarObject);

                var oplossing = sut.VrijZoeken(query, vorigResultaat: zoekresultaat);

                Assert.AreEqual(builder.AantalSets + builder.AantalFragmenten, oplossing.AlleMogelijkheden.Count());
            }

            [TestMethod]
            public void VrijZoeken_SpecifiekZoekenGeheel_GeefAlleSetsPlusSpecifiekeFragmenten()
            {
                const string setToUse = "Sela";
                const string query = "Sela ";  // Zoeken op 'Sela' specifiek, spatie moet zeker fragmenten zoeken triggeren
                var builder = new ZoekresultaatBuilder()
                    .AddKrijgAlleSetNamen()
                    .AddKrijgAlleFragmentenUitSet(setToUse);
                builder.BuildDefault();
                var sut = new LiturgieZoeker(builder.LiturgieDatabase, builder.LiturgieTekstNaarObject);

                var oplossing = sut.VrijZoeken(query);

                Assert.AreEqual(builder.AantalSets + builder.AantalFragmenten, oplossing.AlleMogelijkheden.Count());
            }
        }
    }
}
