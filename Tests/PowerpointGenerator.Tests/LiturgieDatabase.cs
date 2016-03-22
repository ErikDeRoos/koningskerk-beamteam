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
            var liturgieItem = A.Fake<ILiturgieInterpretatie>();
            A.CallTo(() => liturgieItem.Benaming).Returns(onderdeel);
            A.CallTo(() => liturgieItem.Deel).Returns(fragment);
            var zoekresultaat = A.Fake<IZoekresultaat>();
            A.CallTo(() => zoekresultaat.OnderdeelNaam).Returns(onderdeel);
            A.CallTo(() => zoekresultaat.FragmentNaam).Returns(fragment);
            var database = A.Fake<Database.ILiturgieDatabase>();
            A.CallTo(database)
                .Where(d => d.Method.Name == "ZoekOnderdeel")
                .WithReturnType<IZoekresultaat>()
                .WithAnyArguments()
                .Returns(zoekresultaat);
            var sut = (new Database.LiturgieOplosser(database)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem);

            Assert.That(oplossing.Resultaat, Is.EqualTo(LiturgieOplossingResultaat.Opgelost));
        }
    }
}
