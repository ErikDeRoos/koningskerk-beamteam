// Copyright 2019 door Erik de Roos
using Generator.Tests.Builders;
using ILiturgieDatabase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Generator.Tests
{
    public class LiturgieDatabaseTests
    {
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
        public class ZoekOnderdeelMethod : LiturgieDatabaseTests
        {
            [DataTestMethod]
            [DataRow("Psalm", "100")]
            public void DefaultDatabase_NormaalOnderdeel_Gevonden(string onderdeel, string fragment)
            {
                var manager = new EngineManagerBuilder()
                    .AddOnderdeelAndFragment(onderdeel, fragment)
                    .Build();
                var sut = new Database.LiturgieDatabase(manager);

                var oplossing = sut.ZoekSpecifiekItem(VerwerkingType.normaal, onderdeel, fragment, null, _liturgieSettingsDefault);

                Assert.AreEqual(DatabaseZoekStatus.Opgelost, oplossing.Status);
            }

            [DataTestMethod]
            [DataRow("Psalm", "100", "2 - 4", new[] { 2, 3, 4 })]
            [DataRow("Psalm", "100", " - 4", new[] { 1, 2, 3, 4 })]
            [DataRow("Psalm", "100", "2 - ", new[] { 2, 3 })]
            public void DefaultDatabase_SubcontentInTekst_JuisteNummersGeselecteerd(string onderdeel, string fragment, string nummerOpgave, int[] opgesplitstAls)
            {
                var fragmentDelen = new[] { nummerOpgave };
                var itemSubContent = string.Join(" ", Enumerable.Range(1, opgesplitstAls.Max()).Select(r => $"{r} Line."));
                var manager = new EngineManagerBuilder()
                    .AddOnderdeelAndFragment(onderdeel, fragment, itemSubContent, itemIsSubcontent: true)
                    .Build();
                var sut = new Database.LiturgieDatabase(manager);

                var oplossing = sut.ZoekSpecifiekItem(VerwerkingType.normaal, onderdeel, fragment, fragmentDelen, _liturgieSettingsDefault);

                Assert.AreEqual(opgesplitstAls.Length, oplossing.Content.Count());
                Assert.IsTrue(oplossing.Content.All(c => opgesplitstAls.Contains(c.Nummer.Value)));
            }

            [DataTestMethod]
            [DataRow("Johannes", "3")]
            public void BijbeltekstDatabase_NormaalOnderdeel_Gevonden(string onderdeel, string fragment)
            {
                var manager = new EngineManagerBuilder()
                    .AddOnderdeelAndFragment(onderdeel, fragment)
                    .Build(Database.LiturgieDatabaseSettings.DatabaseNameBijbeltekst);
                var sut = new Database.LiturgieDatabase(manager);

                var oplossing = sut.ZoekSpecifiekItem(VerwerkingType.bijbeltekst, onderdeel, fragment, null, _liturgieSettingsDefault);

                Assert.AreEqual(DatabaseZoekStatus.Opgelost, oplossing.Status);
            }

            [DataTestMethod]
            [DataRow("Johannes", "3", "2", "1 In den beginne 2 was het woord 3 en het woord was", "was het woord")]
            [DataRow("Deutronomium", "10", "1", "1 Jakob nam 5 mannen mee. 2 Zij gingen een stukje lopen", "Jakob nam 5 mannen mee.")]
            [DataRow("Deutronomium", "10", "1", "1 Jakob nam 5-7 mannen mee. 2 Zij gingen een stukje lopen", "Jakob nam 5-7 mannen mee.")]
            [DataRow("Deutronomium", "10", "2", "1 Jakob nam 5 mannen mee. 2-3 Zij gingen een stukje lopen", "Zij gingen een stukje lopen")]
            [DataRow("Deutronomium", "10", "4", "1 Jakob nam 5 mannen mee. 2-3 Zij gingen een stukje lopen, 4 het was fijn.", "het was fijn.")]
            public void BijbeltekstDatabase_SubcontentInTekst_JuisteContentUitTekstGehaald(string onderdeel, string fragment, string find, string inContent, string foundContent)
            {
                var manager = new EngineManagerBuilder()
                    .AddOnderdeelAndFragment(onderdeel, fragment, inContent, itemIsSubcontent:true)
                    .Build(Database.LiturgieDatabaseSettings.DatabaseNameBijbeltekst);
                var delen = new[] { find };
                var sut = new Database.LiturgieDatabase(manager);

                var oplossing = sut.ZoekSpecifiekItem(VerwerkingType.bijbeltekst, onderdeel, fragment, delen, _liturgieSettingsDefault);

                var eersteContent = oplossing.Content.FirstOrDefault();
                Assert.IsNotNull(eersteContent);
                Assert.AreEqual(foundContent, eersteContent.Inhoud);
            }
        }
    }
}
