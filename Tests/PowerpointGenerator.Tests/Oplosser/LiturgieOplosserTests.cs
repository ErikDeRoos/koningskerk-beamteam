// Copyright 2019 door Erik de Roos
using FakeItEasy;
using Generator.Database.FileSystem;
using ILiturgieDatabase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Generator.Tests
{
    public class LiturgieOplosserTests
    {
        private const string DefaultEmptyName = "!leeg";
        private ILiturgieInterpreteer _liturgieInterpreteer;
        private ILiturgieSettings _liturgieSettings;

        [TestInitialize]
        public void Initialise()
        {
            _liturgieInterpreteer = A.Fake<ILiturgieInterpreteer>();
            _liturgieSettings = FakeLiturgieSettings();
        }

        [TestClass]
        public class LosOpMethod : LiturgieOplosserTests
        {
            [DataTestMethod]
            [DataRow("Psalm", "100")]
            public void NormaalItem_Gevonden(string onderdeel, string fragment)
            {
                var liturgieItem = FakeInterpretatie(onderdeel, fragment: fragment);
                var database = FakeDatabase(onderdeel, fragment);
                var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, _liturgieInterpreteer, DefaultEmptyName)) as ILiturgieLosOp;

                var oplossing = sut.LosOp(liturgieItem, _liturgieSettings);

                Assert.AreEqual(oplossing.Resultaat, LiturgieOplossingResultaat.Opgelost);
            }

            [DataTestMethod]
            [DataRow("Welkom")]
            public void CommonItem_Gevonden(string onderdeel)
            {
                var liturgieItem = FakeInterpretatie(onderdeel);
                var database = FakeDatabase(FileEngineDefaults.CommonFilesSetName, onderdeel);
                var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, _liturgieInterpreteer, DefaultEmptyName)) as ILiturgieLosOp;

                var oplossing = sut.LosOp(liturgieItem, _liturgieSettings);

                Assert.AreEqual(oplossing.Resultaat, LiturgieOplossingResultaat.Opgelost);
            }

            [DataTestMethod]
            [DataRow("Psalm", "100")]
            [DataRow("Welkom", null)]
            public void AlleItems_GebruikStandaardNaam(string onderdeel, string fragment)
            {
                var liturgieItem = FakeInterpretatie(onderdeel, fragment: fragment);
                var database = fragment != null ? FakeDatabase(onderdeel, fragment) : FakeDatabase(FileEngineDefaults.CommonFilesSetName, onderdeel);
                var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, _liturgieInterpreteer, DefaultEmptyName)) as ILiturgieLosOp;

                var oplossing = sut.LosOp(liturgieItem, _liturgieSettings);

                Assert.AreEqual(oplossing.Regel.Display.Naam, onderdeel);
            }

            [DataTestMethod]
            [DataRow("Psalm2", "100", "Psalm")]
            public void NormaalItem_GebruikDisplay(string onderdeel, string fragment, string display)
            {
                var liturgieItem = FakeInterpretatie(onderdeel, fragment: fragment);
                var database = FakeDatabase(onderdeel, fragment, display: display);
                var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, _liturgieInterpreteer, DefaultEmptyName)) as ILiturgieLosOp;

                var oplossing = sut.LosOp(liturgieItem, _liturgieSettings);

                Assert.AreEqual(oplossing.Regel.Display.Naam, display);
            }

            [DataTestMethod]
            [DataRow("Psalm2", "100", "psalm2", "Psalm")]
            public void NormaalItem_GebruikMask(string onderdeel, string fragment, string maskRealName, string maskUseName)
            {
                var maskList = FakeMask(maskRealName, maskUseName);
                var liturgieItem = FakeInterpretatie(onderdeel, fragment: fragment);
                var database = FakeDatabase(onderdeel, fragment);
                var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, _liturgieInterpreteer, DefaultEmptyName)) as ILiturgieLosOp;

                var oplossing = sut.LosOp(liturgieItem, maskList, _liturgieSettings);

                Assert.AreEqual(oplossing.Regel.Display.Naam, maskUseName);
            }

            [DataTestMethod]
            [DataRow("Welkom_groot", "Welkom_groot", "Welkom")]
            public void CommonItem_GebruikMask(string onderdeel, string maskRealName, string maskUseName)
            {
                var maskList = FakeMask(maskRealName, maskUseName);
                var liturgieItem = FakeInterpretatie(onderdeel);
                var database = FakeDatabase(FileEngineDefaults.CommonFilesSetName, onderdeel);
                var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, _liturgieInterpreteer, DefaultEmptyName)) as ILiturgieLosOp;

                var oplossing = sut.LosOp(liturgieItem, maskList, _liturgieSettings);

                Assert.AreEqual(oplossing.Regel.Display.Naam, maskUseName);
            }

            [DataTestMethod]
            [DataRow("1 Petrus 1 : 1", "1_Petrus 1 : 1")]
            [DataRow("1 Petrus", "1_Petrus")]
            [DataRow("Sela ik zal er zijn", "Sela ik_zal_er_zijn")]
            public void MaakTotTekst_werkt(string invoer, string verwachtResultaat)
            {
                var zoekresultaat = FakeZoekresultaat(new[] {
                new ZoekresultaatItem
                {
                    Weergave = "Sela",
                    VeiligeNaam = "Sela",
                },
                new ZoekresultaatItem
                {
                    Weergave = "Sela ik zal er zijn",
                    VeiligeNaam = "Sela ik_zal_er_zijn",
                },
                new ZoekresultaatItem
                {
                    Weergave = "1 Petrus",
                    VeiligeNaam = "1_Petrus",
                },
                new ZoekresultaatItem
                {
                    Weergave = "1 Petrus 1",
                    VeiligeNaam = "1_Petrus 1",
                }
            });
                var database = A.Fake<ILiturgieDatabase.ILiturgieDatabase>();
                var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, _liturgieInterpreteer, DefaultEmptyName)) as ILiturgieLosOp;

                var oplossing = sut.MaakTotTekst(invoer, null, zoekresultaat);

                Assert.AreEqual(verwachtResultaat, oplossing);
            }
        }

        private static IEnumerable<ILiturgieMapmaskArg> FakeMask(string maskRealName, string maskUseName)
        {
            var maskItem = A.Fake<ILiturgieMapmaskArg>();
            A.CallTo(() => maskItem.RealName).Returns(maskRealName);
            A.CallTo(() => maskItem.Name).Returns(maskUseName);
            return new[] { maskItem };
        }

        private static ILiturgieInterpretatie FakeInterpretatie(string onderdeel, string fragment = null)
        {
            var liturgieItem = A.Fake<ILiturgieInterpretatie>();
            A.CallTo(() => liturgieItem.Benaming).Returns(onderdeel);
            if (fragment != null)
                A.CallTo(() => liturgieItem.Deel).Returns(fragment);
            return liturgieItem;
        }

        private static ILiturgieDatabase.ILiturgieDatabase FakeDatabase(string onderdeel, string fragment, string display = null, LiturgieOplossingResultaat status = LiturgieOplossingResultaat.Opgelost)
        {
            var zoekresultaatOnderdeel = A.Fake<IOplossingOnderdeel>();
            A.CallTo(() => zoekresultaatOnderdeel.OrigineleNaam).Returns(onderdeel);
            A.CallTo(() => zoekresultaatOnderdeel.VeiligeNaam).Returns(onderdeel);
            if (display != null)
                A.CallTo(() => zoekresultaatOnderdeel.DisplayNaam).Returns(display);
            var zoekresultaatFragment = A.Fake<IOplossingOnderdeel>();
            A.CallTo(() => zoekresultaatFragment.OrigineleNaam).Returns(fragment);
            A.CallTo(() => zoekresultaatFragment.VeiligeNaam).Returns(fragment);
            var zoekresultaat = A.Fake<IOplossing>();
            A.CallTo(() => zoekresultaat.Onderdeel).Returns(zoekresultaatOnderdeel);
            A.CallTo(() => zoekresultaat.Fragment).Returns(zoekresultaatFragment);
            A.CallTo(() => zoekresultaat.Status).Returns(status);
            var database = A.Fake<ILiturgieDatabase.ILiturgieDatabase>();
            A.CallTo(database)
                .Where(d => d.Method.Name == "ZoekOnderdeel")
                .WithReturnType<IOplossing>()
                .WithAnyArguments()
                .Returns(zoekresultaat);
            return database;
        }

        private static ILiturgieSettings FakeLiturgieSettings()
        {
            var settings = A.Fake<ILiturgieSettings>();
            A.CallTo(() => settings.ToonBijbeltekstenInLiturgie).Returns(true);
            return settings;
        }

        private static IVrijZoekresultaat FakeZoekresultaat(ZoekresultaatItem[] zoekresultaten)
        {
            var zoekresultaat = A.Fake<IVrijZoekresultaat>();
            A.CallTo(() => zoekresultaat.AlleMogelijkheden).Returns(zoekresultaten);
            return zoekresultaat;
        }

        private class ZoekresultaatItem : IVrijZoekresultaatMogelijkheid
        {
            public string Weergave { get; set; }
            public string VeiligeNaam { get; set; }
            public string UitDatabase { get; set; }

            public bool Equals(IVrijZoekresultaatMogelijkheid x, IVrijZoekresultaatMogelijkheid y)
            {
                if (x == null || y == null)
                    return false;
                return x.Weergave == y.Weergave;  // Alleen sorteren op weergave naam
            }

            public int GetHashCode(IVrijZoekresultaatMogelijkheid obj)
            {
                return obj.Weergave.GetHashCode();
            }

            public override string ToString()
            {
                return Weergave;
            }
        }
    }
}
