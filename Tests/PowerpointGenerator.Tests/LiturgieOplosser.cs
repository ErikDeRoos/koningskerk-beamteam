// Copyright 2016 door Erik de Roos
using FakeItEasy;
using ILiturgieDatabase;
using NUnit.Framework;
using Generator.Database.FileSystem;
using System.Collections.Generic;
using System.Linq;

namespace Generator.Tests
{
    public class LiturgieOplosser
    {
        private static string DefaultEmptyName = "!leeg";

        [TestCase("Psalm", "100")]
        public void LosOp_NormaalItem_Gevonden(string onderdeel, string fragment)
        {
            var liturgieItem = FakeInterpretatie(onderdeel, fragment: fragment);
            var database = FakeDatabase(onderdeel, fragment);
            var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, DefaultEmptyName)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem);

            Assert.That(oplossing.Resultaat, Is.EqualTo(LiturgieOplossingResultaat.Opgelost));
        }

        [TestCase("Welkom")]
        public void LosOp_CommonItem_Gevonden(string onderdeel)
        {
            var liturgieItem = FakeInterpretatie(onderdeel);
            var database = FakeDatabase(FileEngineDefaults.CommonFilesSetName, onderdeel);
            var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, DefaultEmptyName)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem);

            Assert.That(oplossing.Resultaat, Is.EqualTo(LiturgieOplossingResultaat.Opgelost));
        }

        [TestCase("Psalm", "100")]
        [TestCase("Welkom", null)]
        public void LosOp_AlleItems_GebruikStandaardNaam(string onderdeel, string fragment)
        {
            var liturgieItem = FakeInterpretatie(onderdeel, fragment: fragment);
            var database = fragment != null ? FakeDatabase(onderdeel, fragment) : FakeDatabase(FileEngineDefaults.CommonFilesSetName, onderdeel);
            var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, DefaultEmptyName)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem);

            Assert.That(oplossing.Regel.Display.Naam, Is.EqualTo(onderdeel));
        }

        [TestCase("Psalm2", "100", "Psalm")]
        public void LosOp_NormaalItem_GebruikDisplay(string onderdeel, string fragment, string display)
        {
            var liturgieItem = FakeInterpretatie(onderdeel, fragment: fragment);
            var database = FakeDatabase(onderdeel, fragment, display: display);
            var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, DefaultEmptyName)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem);

            Assert.That(oplossing.Regel.Display.Naam, Is.EqualTo(display));
        }

        [TestCase("Psalm2", "100", "psalm2", "Psalm")]
        public void LosOp_NormaalItem_GebruikMask(string onderdeel, string fragment, string maskRealName, string maskUseName)
        {
            var maskList = FakeMask(maskRealName, maskUseName);
            var liturgieItem = FakeInterpretatie(onderdeel, fragment: fragment);
            var database = FakeDatabase(onderdeel, fragment);
            var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, DefaultEmptyName)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem, maskList);

            Assert.That(oplossing.Regel.Display.Naam, Is.EqualTo(maskUseName));
        }

        [TestCase("Welkom_groot", "Welkom_groot", "Welkom")]
        public void LosOp_CommonItem_GebruikMask(string onderdeel, string maskRealName, string maskUseName)
        {
            var maskList = FakeMask(maskRealName, maskUseName);
            var liturgieItem = FakeInterpretatie(onderdeel);
            var database = FakeDatabase(FileEngineDefaults.CommonFilesSetName, onderdeel);
            var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, DefaultEmptyName)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem, maskList);

            Assert.That(oplossing.Regel.Display.Naam, Is.EqualTo(maskUseName));
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

        private static ILiturgieDatabase.ILiturgieDatabase FakeDatabase(string onderdeel, string fragment, string display = null)
        {
            var zoekresultaat = A.Fake<IZoekresultaat>();
            A.CallTo(() => zoekresultaat.OnderdeelNaam).Returns(onderdeel);
            A.CallTo(() => zoekresultaat.FragmentNaam).Returns(fragment);
            if (display != null)
                A.CallTo(() => zoekresultaat.OnderdeelDisplayNaam).Returns(display);
            var database = A.Fake<ILiturgieDatabase.ILiturgieDatabase>();
            A.CallTo(database)
                .Where(d => d.Method.Name == "ZoekOnderdeel")
                .WithReturnType<IZoekresultaat>()
                .WithAnyArguments()
                .Returns(zoekresultaat);
            return database;
        }
    }

    public class LiturgieOplosser_Bijbeltekst
    {
        private static string DefaultEmptyName = "!leeg";

        [TestCase("Johannes", "3", new [] { "1" ,"2", "3"})]
        public void LosOp_NormaalItem_ZoekInDatabase(string onderdeel, string fragment, IEnumerable<string> fragmentVerzen)
        {
            var liturgieItem = FakeBijbeltekstInterpretatie(onderdeel, fragment, fragmentVerzen);
            var database = FakeDatabase(onderdeel, fragment);
            var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, DefaultEmptyName)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem);

            A.CallTo(() => database.ZoekOnderdeel(VerwerkingType.bijbeltekst, liturgieItem.Benaming, liturgieItem.PerDeelVersen.First().Deel, liturgieItem.PerDeelVersen.First().Verzen)).MustHaveHappened();
        }

        [TestCase("Johannes", "3", new[] { "1", "2", "3" })]
        public void LosOp_NormaalItem_Gevonden(string onderdeel, string fragment, IEnumerable<string> fragmentVerzen)
        {
            var liturgieItem = FakeBijbeltekstInterpretatie(onderdeel, fragment, fragmentVerzen);
            var database = FakeDatabase(onderdeel, fragment);
            var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, DefaultEmptyName)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem);

            Assert.That(oplossing.Resultaat, Is.EqualTo(LiturgieOplossingResultaat.Opgelost));
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

        private static ILiturgieDatabase.ILiturgieDatabase FakeDatabase(string onderdeel, string fragment, string display = null)
        {
            var zoekresultaat = A.Fake<IZoekresultaat>();
            A.CallTo(() => zoekresultaat.OnderdeelNaam).Returns(onderdeel);
            A.CallTo(() => zoekresultaat.FragmentNaam).Returns(fragment);
            if (display != null)
                A.CallTo(() => zoekresultaat.OnderdeelDisplayNaam).Returns(display);
            var database = A.Fake<ILiturgieDatabase.ILiturgieDatabase>();
            A.CallTo(database)
                .Where(d => d.Method.Name == "ZoekOnderdeel")
                .WithReturnType<IZoekresultaat>()
                .WithAnyArguments()
                .Returns(zoekresultaat);
            return database;
        }
    }
}
