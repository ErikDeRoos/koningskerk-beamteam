// Copyright 2018 door Erik de Roos
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

        [TestInitialize]
        public void Initialise()
        {
            _liturgieInterpreteer = A.Fake<ILiturgieInterpreteer>();
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

                var oplossing = sut.LosOp(liturgieItem);

                Assert.AreEqual(oplossing.Resultaat, LiturgieOplossingResultaat.Opgelost);
            }

            [DataTestMethod]
            [DataRow("Welkom")]
            public void CommonItem_Gevonden(string onderdeel)
            {
                var liturgieItem = FakeInterpretatie(onderdeel);
                var database = FakeDatabase(FileEngineDefaults.CommonFilesSetName, onderdeel);
                var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, _liturgieInterpreteer, DefaultEmptyName)) as ILiturgieLosOp;

                var oplossing = sut.LosOp(liturgieItem);

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

                var oplossing = sut.LosOp(liturgieItem);

                Assert.AreEqual(oplossing.Regel.Display.Naam, onderdeel);
            }

            [DataTestMethod]
            [DataRow("Psalm2", "100", "Psalm")]
            public void NormaalItem_GebruikDisplay(string onderdeel, string fragment, string display)
            {
                var liturgieItem = FakeInterpretatie(onderdeel, fragment: fragment);
                var database = FakeDatabase(onderdeel, fragment, display: display);
                var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, _liturgieInterpreteer, DefaultEmptyName)) as ILiturgieLosOp;

                var oplossing = sut.LosOp(liturgieItem);

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

                var oplossing = sut.LosOp(liturgieItem, maskList);

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

                var oplossing = sut.LosOp(liturgieItem, maskList);

                Assert.AreEqual(oplossing.Regel.Display.Naam, maskUseName);
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
            var zoekresultaat = A.Fake<IOplossing>();
            A.CallTo(() => zoekresultaat.OnderdeelNaam).Returns(onderdeel);
            A.CallTo(() => zoekresultaat.FragmentNaam).Returns(fragment);
            A.CallTo(() => zoekresultaat.Status).Returns(status);
            if (display != null)
                A.CallTo(() => zoekresultaat.OnderdeelDisplayNaam).Returns(display);
            var database = A.Fake<ILiturgieDatabase.ILiturgieDatabase>();
            A.CallTo(database)
                .Where(d => d.Method.Name == "ZoekOnderdeel")
                .WithReturnType<IOplossing>()
                .WithAnyArguments()
                .Returns(zoekresultaat);
            return database;
        }
    }
}
