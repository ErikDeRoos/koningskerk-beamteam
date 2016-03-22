using Autofac.Extras.FakeItEasy;
using FakeItEasy;
using IDatabase;
using ILiturgieDatabase;
using NUnit.Framework;
using PowerpointGenerator.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerpointGenerator.Tests
{
    public class LiturgieDatabase
    {
        [TestCase("Psalm", "100")]
        public void LosOp_NormaalItem_Gevonden(string onderdeel, string fragment)
        {
            var liturgieItem = FakeInterpretatie(onderdeel, fragment: fragment);
            var database = FakeDatabase(onderdeel, fragment);
            var sut = (new Database.LiturgieOplosser(database)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem);

            Assert.That(oplossing.Resultaat, Is.EqualTo(LiturgieOplossingResultaat.Opgelost));
        }

        [TestCase("Welkom")]
        public void LosOp_CommonItem_Gevonden(string onderdeel)
        {
            var liturgieItem = FakeInterpretatie(onderdeel);
            var database = FakeDatabase(FileEngineDefaults.CommonFilesSetName, onderdeel);
            var sut = (new Database.LiturgieOplosser(database)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem);

            Assert.That(oplossing.Resultaat, Is.EqualTo(LiturgieOplossingResultaat.Opgelost));
        }

        [TestCase("Psalm", "100")]
        [TestCase("Welkom", null)]
        public void LosOp_AlleItems_GebruikStandaardNaam(string onderdeel, string fragment)
        {
            var liturgieItem = FakeInterpretatie(onderdeel, fragment: fragment);
            var database = fragment != null ? FakeDatabase(onderdeel, fragment) : FakeDatabase(FileEngineDefaults.CommonFilesSetName, onderdeel);
            var sut = (new Database.LiturgieOplosser(database)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem);

            Assert.That(oplossing.Regel.Display.Naam, Is.EqualTo(onderdeel));
        }

        [TestCase("Psalm2", "100", "Psalm")]
        public void LosOp_NormaalItem_GebruikDisplay(string onderdeel, string fragment, string display)
        {
            var liturgieItem = FakeInterpretatie(onderdeel, fragment: fragment);
            var database = FakeDatabase(onderdeel, fragment, display: display);
            var sut = (new Database.LiturgieOplosser(database)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem);

            Assert.That(oplossing.Regel.Display.Naam, Is.EqualTo(display));
        }

        [TestCase("Psalm2", "100", "psalm2", "Psalm")]
        public void LosOp_NormaalItem_GebruikMask(string onderdeel, string fragment, string maskRealName, string maskUseName)
        {
            var maskItem = A.Fake<ILiturgieMapmaskArg>();
            A.CallTo(() => maskItem.RealName).Returns(maskRealName);
            A.CallTo(() => maskItem.Name).Returns(maskUseName);
            var maskList = new[] { maskItem };
            var liturgieItem = FakeInterpretatie(onderdeel, fragment: fragment);
            var database = FakeDatabase(onderdeel, fragment);
            var sut = (new Database.LiturgieOplosser(database)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem, maskList);

            Assert.That(oplossing.Regel.Display.Naam, Is.EqualTo(maskUseName));
        }


        private static ILiturgieInterpretatie FakeInterpretatie(string onderdeel, string fragment = null)
        {
            var liturgieItem = A.Fake<ILiturgieInterpretatie>();
            A.CallTo(() => liturgieItem.Benaming).Returns(onderdeel);
            if (fragment != null)
                A.CallTo(() => liturgieItem.Deel).Returns(fragment);
            return liturgieItem;
        }

        private static Database.ILiturgieDatabase FakeDatabase(string onderdeel, string fragment, string display = null)
        {
            var zoekresultaat = A.Fake<IZoekresultaat>();
            A.CallTo(() => zoekresultaat.OnderdeelNaam).Returns(onderdeel);
            A.CallTo(() => zoekresultaat.FragmentNaam).Returns(fragment);
            if (display != null)
                A.CallTo(() => zoekresultaat.OnderdeelDisplayNaam).Returns(display);
            var database = A.Fake<Database.ILiturgieDatabase>();
            A.CallTo(database)
                .Where(d => d.Method.Name == "ZoekOnderdeel")
                .WithReturnType<IZoekresultaat>()
                .WithAnyArguments()
                .Returns(zoekresultaat);
            return database;
        }
    }
}
