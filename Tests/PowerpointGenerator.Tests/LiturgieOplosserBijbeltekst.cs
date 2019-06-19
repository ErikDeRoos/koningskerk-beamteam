// Copyright 2019 door Erik de Roos
using FakeItEasy;
using ILiturgieDatabase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Generator.Tests
{
    public class LiturgieOplosserBijbeltekst
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
        public class LosOpMethod : LiturgieOplosserBijbeltekst
        {
            [DataTestMethod]
            [DataRow("Johannes", "3", new[] { "1", "2", "3" })]
            public void NormaalItem_ZoekInDatabase(string onderdeel, string fragment, IEnumerable<string> fragmentVerzen)
            {
                var liturgieItem = FakeBijbeltekstInterpretatie(onderdeel, fragment, fragmentVerzen);
                var database = FakeDatabase(onderdeel, fragment);
                var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, _liturgieInterpreteer, DefaultEmptyName)) as ILiturgieLosOp;

                var oplossing = sut.LosOp(liturgieItem, _liturgieSettings);

                A.CallTo(() => database.ZoekOnderdeel(VerwerkingType.bijbeltekst, liturgieItem.Benaming, liturgieItem.PerDeelVersen.First().Deel, liturgieItem.PerDeelVersen.First().Verzen, _liturgieSettings)).MustHaveHappened();
            }

            [DataTestMethod]
            [DataRow("Johannes", "3", new[] { "1", "2", "3" })]
            public void NormaalItem_Gevonden(string onderdeel, string fragment, IEnumerable<string> fragmentVerzen)
            {
                var liturgieItem = FakeBijbeltekstInterpretatie(onderdeel, fragment, fragmentVerzen);
                var database = FakeDatabase(onderdeel, fragment);
                var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, _liturgieInterpreteer, DefaultEmptyName)) as ILiturgieLosOp;

                var oplossing = sut.LosOp(liturgieItem, _liturgieSettings);

                Assert.AreEqual(oplossing.Resultaat, LiturgieOplossingResultaat.Opgelost);
            }
        }

        private static ILiturgieInterpretatieBijbeltekst FakeBijbeltekstInterpretatie(string onderdeel, string fragment, IEnumerable<string> verzen)
        {
            var liturgieItem = A.Fake<ILiturgieInterpretatieBijbeltekst>();
            A.CallTo(() => liturgieItem.Benaming).Returns(onderdeel);
            A.CallTo(() => liturgieItem.Deel).Returns(fragment);
            var liturgieItemDeel = A.Fake<ILiturgieInterpretatieBijbeltekstDeel>();
            A.CallTo(() => liturgieItemDeel.Deel).Returns(fragment);
            A.CallTo(() => liturgieItemDeel.Verzen).Returns(verzen.ToList());
            A.CallTo(() => liturgieItem.PerDeelVersen).Returns(new[] { liturgieItemDeel });
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

        private static ILiturgieSettings FakeLiturgieSettings()
        {
            var settings = A.Fake<ILiturgieSettings>();
            A.CallTo(() => settings.ToonBijbeltekstenInLiturgie).Returns(true);
            return settings;
        }
    }
}