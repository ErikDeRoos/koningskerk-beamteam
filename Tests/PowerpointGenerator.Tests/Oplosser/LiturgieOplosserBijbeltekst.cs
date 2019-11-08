// Copyright 2019 door Erik de Roos
using Generator.Tests.Builders;
using ILiturgieDatabase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Generator.Tests
{
    public class LiturgieOplosserBijbeltekst
    {
        private const string DefaultEmptyName = "!leeg";
        private ILiturgieTekstNaarObject _liturgieInterpreteer;
        private LiturgieSettings _liturgieSettingsDefault;

        [TestInitialize]
        public void Initialise()
        {
            _liturgieInterpreteer = new Mock<ILiturgieTekstNaarObject>().Object;
            _liturgieSettingsDefault = new LiturgieSettings
            {
                ToonBijbeltekstenInLiturgie = true,
            };
        }

        [TestClass]
        public class LosOpMethod : LiturgieOplosserBijbeltekst
        {
            [DataTestMethod]
            [DataRow("Johannes", "3", new[] { "1", "2", "3" })]
            public void NormaalItem_ZoekInDatabase(string onderdeel, string fragment, IEnumerable<string> fragmentVerzen)
            {
                var liturgieItem = MockBijbeltekstInvoer(onderdeel, fragment, fragmentVerzen);
                var databaseBuilder = new LiturgieDatabaseBuilder()
                    .KrijgItem_AddOnderdeelAndFragment(onderdeel, fragment);
                var database = databaseBuilder.Build();
                var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, _liturgieInterpreteer, DefaultEmptyName)) as ILiturgieSlideMaker;

                var oplossing = sut.ConverteerNaarSlide(liturgieItem, _liturgieSettingsDefault);

                databaseBuilder.Database.Verify(x => x.KrijgItem(VerwerkingType.bijbeltekst, liturgieItem.Benaming, liturgieItem.PerDeelVersen.First().Deel, liturgieItem.PerDeelVersen.First().Verzen, _liturgieSettingsDefault));
            }

            [DataTestMethod]
            [DataRow("Johannes", "3", new[] { "1", "2", "3" })]
            public void NormaalItem_Gevonden(string onderdeel, string fragment, IEnumerable<string> fragmentVerzen)
            {
                var liturgieItem = MockBijbeltekstInvoer(onderdeel, fragment, fragmentVerzen);
                var database = new LiturgieDatabaseBuilder()
                    .KrijgItem_AddOnderdeelAndFragment(onderdeel, fragment)
                    .Build();
                var sut = (new Generator.LiturgieOplosser.LiturgieOplosser(database, _liturgieInterpreteer, DefaultEmptyName)) as ILiturgieSlideMaker;

                var oplossing = sut.ConverteerNaarSlide(liturgieItem, _liturgieSettingsDefault);

                Assert.AreEqual(DatabaseZoekStatus.Opgelost, oplossing.ResultaatStatus);
            }
        }

        private static ILiturgieInterpretatieBijbeltekst MockBijbeltekstInvoer(string onderdeel, string fragment, IEnumerable<string> verzen)
        {
            var liturgieItemDeel = new Mock<ILiturgieInterpretatieBijbeltekstDeel>();
            liturgieItemDeel.SetupGet(x => x.Deel).Returns(fragment);
            liturgieItemDeel.SetupGet(x => x.Verzen).Returns(verzen);

            var liturgieItem = new Mock<ILiturgieInterpretatieBijbeltekst>();
            liturgieItem.SetupGet(x => x.Benaming).Returns(onderdeel);
            liturgieItem.SetupGet(x => x.Deel).Returns(fragment);
            liturgieItem.SetupGet(x => x.OptiesGebruiker).Returns(new LiturgieOptiesGebruiker());
            liturgieItem.SetupGet(x => x.PerDeelVersen).Returns(new[] { liturgieItemDeel.Object });
            return liturgieItem.Object;
        }
    }
}