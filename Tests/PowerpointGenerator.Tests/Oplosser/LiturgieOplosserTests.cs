// Copyright 2019 door Erik de Roos
using Generator.Database.FileSystem;
using Generator.Tests.Builders;
using ILiturgieDatabase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Generator.Tests
{
    public class LiturgieOplosserTests
    {
        private const string DefaultEmptyName = "!leeg";
        private LiturgieSettings _liturgieSettingsDefault;

        [TestInitialize]
        public void Initialise()
        {
            _liturgieSettingsDefault = new LiturgieSettings
            {
                ToonBijbeltekstenInLiturgie = true,
            };
        }

        [TestClass]
        public class LosOpMethod : LiturgieOplosserTests
        {
            [DataTestMethod]
            [DataRow("Psalm", "100")]
            public void NormaalItem_Gevonden(string onderdeel, string fragment)
            {
                var liturgieItem = MockInterpretatie(onderdeel, fragment);
                var database = new LiturgieDatabaseBuilder()
                    .ZoekSpecifiek_AddOnderdeelAndFragment(onderdeel, fragment)
                    .Build();
                var sut = new Generator.LiturgieOplosser.LiturgieOplosser(database, null, DefaultEmptyName);

                var oplossing = sut.ConverteerNaarSlide(liturgieItem, _liturgieSettingsDefault);

                Assert.AreEqual(oplossing.ResultaatStatus, DatabaseZoekStatus.Opgelost);
            }

            [DataTestMethod]
            [DataRow("Welkom")]
            public void CommonItem_Gevonden(string onderdeel)
            {
                var liturgieItem = MockInterpretatie(onderdeel);
                var database = new LiturgieDatabaseBuilder()
                    .ZoekSpecifiek_AddOnderdeelAndFragment(FileEngineDefaults.CommonFilesSetName, onderdeel)
                    .Build();
                var sut = new Generator.LiturgieOplosser.LiturgieOplosser(database, null, DefaultEmptyName);

                var oplossing = sut.ConverteerNaarSlide(liturgieItem, _liturgieSettingsDefault);

                Assert.AreEqual(oplossing.ResultaatStatus, DatabaseZoekStatus.Opgelost);
            }

            [DataTestMethod]
            [DataRow("Psalm", "100")]
            [DataRow("Welkom", null)]
            public void AlleItems_GebruikStandaardNaam(string onderdeel, string fragment)
            {
                var liturgieItem = MockInterpretatie(onderdeel, fragment);
                var databaseBuilder = new LiturgieDatabaseBuilder();
                if (fragment != null)
                    databaseBuilder.ZoekSpecifiek_AddOnderdeelAndFragment(onderdeel, fragment);
                else
                    databaseBuilder.ZoekSpecifiek_AddOnderdeelAndFragment(FileEngineDefaults.CommonFilesSetName, onderdeel);
                var database = databaseBuilder.Build();
                var sut = new Generator.LiturgieOplosser.LiturgieOplosser(database, null, DefaultEmptyName);

                var oplossing = sut.ConverteerNaarSlide(liturgieItem, _liturgieSettingsDefault);

                Assert.AreEqual(oplossing.ResultaatSlide.Display.Naam, onderdeel);
            }

            [DataTestMethod]
            [DataRow("Psalm2", "100", "Psalm")]
            public void NormaalItem_GebruikDisplay(string onderdeel, string fragment, string display)
            {
                var liturgieItem = MockInterpretatie(onderdeel, fragment);
                var database = new LiturgieDatabaseBuilder()
                    .ZoekSpecifiek_AddOnderdeelAndFragment(onderdeel, fragment, display)
                    .Build();
                var sut = new Generator.LiturgieOplosser.LiturgieOplosser(database, null, DefaultEmptyName);

                var oplossing = sut.ConverteerNaarSlide(liturgieItem, _liturgieSettingsDefault);

                Assert.AreEqual(oplossing.ResultaatSlide.Display.Naam, display);
            }

            [DataTestMethod]
            [DataRow("Psalm2", "100", "psalm2", "Psalm")]
            public void NormaalItem_GebruikMask(string onderdeel, string fragment, string maskRealName, string maskUseName)
            {
                var maskList = MaskList(maskRealName, maskUseName);
                var liturgieItem = MockInterpretatie(onderdeel, fragment);
                var database = new LiturgieDatabaseBuilder()
                    .ZoekSpecifiek_AddOnderdeelAndFragment(onderdeel, fragment)
                    .Build();
                var sut = new Generator.LiturgieOplosser.LiturgieOplosser(database, null, DefaultEmptyName);

                var oplossing = sut.ConverteerNaarSlide(liturgieItem, _liturgieSettingsDefault, maskList);

                Assert.AreEqual(oplossing.ResultaatSlide.Display.Naam, maskUseName);
            }

            [DataTestMethod]
            [DataRow("Welkom_groot", "Welkom_groot", "Welkom")]
            public void CommonItem_GebruikMask(string onderdeel, string maskRealName, string maskUseName)
            {
                var maskList = MaskList(maskRealName, maskUseName);
                var liturgieItem = MockInterpretatie(onderdeel);
                var database = new LiturgieDatabaseBuilder()
                    .ZoekSpecifiek_AddOnderdeelAndFragment(FileEngineDefaults.CommonFilesSetName, onderdeel)
                    .Build();
                var sut = new Generator.LiturgieOplosser.LiturgieOplosser(database, null, DefaultEmptyName);

                var oplossing = sut.ConverteerNaarSlide(liturgieItem, _liturgieSettingsDefault, maskList);

                Assert.AreEqual(oplossing.ResultaatSlide.Display.Naam, maskUseName);
            }

            [DataTestMethod]
            [DataRow("1 Petrus 1 : 1", "1_Petrus 1 : 1")]
            [DataRow("1 Petrus", "1_Petrus")]
            [DataRow("Sela ik zal er zijn", "Sela ik_zal_er_zijn")]
            public void MaakTotTekst_werkt(string invoer, string verwachtResultaat)
            {
                var zoekresultaat = MockZoekresultaat(new[] 
                {
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
                var liturgieInterpreteer = new Mock<ILiturgieTekstNaarObject>();
                liturgieInterpreteer.Setup(x => x.MaakTekstVanOpties(It.IsAny<LiturgieOptiesGebruiker>())).Returns(string.Empty);
                var sut = new LiturgieOplosser.LiturgieZoeker(null, liturgieInterpreteer.Object);

                var oplossing = sut.MaakTotTekst(invoer, null, zoekresultaat);

                Assert.AreEqual(verwachtResultaat, oplossing);
            }
        }

        private static IEnumerable<LiturgieMapmaskArg> MaskList(string maskRealName, string maskUseName)
        {
            return new[] {
                new LiturgieMapmaskArg
                {
                    Name = maskUseName,
                    RealName = maskRealName,
                }
            };
        }

        private static ILiturgieTekstObject MockInterpretatie(string onderdeel, string fragment = null)
        {
            var liturgieItem = new Mock<ILiturgieTekstObject>();
            liturgieItem.SetupGet(x => x.Benaming).Returns(onderdeel);
            liturgieItem.SetupGet(x => x.Deel).Returns(fragment);
            liturgieItem.SetupGet(x => x.OptiesGebruiker).Returns(new LiturgieOptiesGebruiker());
            return liturgieItem.Object;
        }

        private static IVrijZoekresultaat MockZoekresultaat(ZoekresultaatItem[] zoekresultaten)
        {
            var zoekresultaat = new Mock<IVrijZoekresultaat>();
            zoekresultaat.SetupGet(x => x.AlleMogelijkheden).Returns(zoekresultaten);
            return zoekresultaat.Object;
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
