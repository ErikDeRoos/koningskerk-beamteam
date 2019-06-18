// Copyright 2018 door Erik de Roos
using FakeItEasy;
using Generator.Database.FileSystem;
using IDatabase;
using IDatabase.Engine;
using ILiturgieDatabase;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Generator.Tests
{
    public class LiturgieDatabaseTests
    {
        private ILiturgieSettings _liturgieSettings;

        [TestInitialize]
        public void Initialise()
        {
            _liturgieSettings = FakeLiturgieSettings();
        }

        [TestClass]
        public class ZoekOnderdeelMethod : LiturgieDatabaseTests
        {
            [DataTestMethod]
            [DataRow("Psalm", "100")]
            public void NormaalItem_Gevonden(string onderdeel, string fragment)
            {
                var engine = new EngineMock<FileEngineSetSettings>(f => f
                    .AddSet(onderdeel)
                    .AddItem(fragment)
                    .SetContent("txt", "lege reeks")
                    );
                var manager = FakeEngineManager(engine);
                var sut = (new Database.LiturgieDatabase(manager)) as ILiturgieDatabase.ILiturgieDatabase;

                var oplossing = sut.ZoekOnderdeel(onderdeel, fragment, null, _liturgieSettings);

                Assert.AreEqual(oplossing.Status, LiturgieOplossingResultaat.Opgelost);
            }

            [DataTestMethod]
            [DataRow("Psalm", "100", "2 - 4", new[] { 2, 3, 4 })]
            [DataRow("Psalm", "100", " - 4", new[] { 1, 2, 3, 4 })]
            [DataRow("Psalm", "100", "2 - ", new[] { 2, 3 })]
            public void NormaalItem_Opgesplitst(string onderdeel, string fragment, string nummer, int[] opgesplitstAls)
            {
                var itemSubContent = string.Join(" ", Enumerable.Range(1, opgesplitstAls.Max()).Select(r => $"{r} Line."));
                var fragmentDelen = new[] { nummer };
                var engine = new EngineMock<FileEngineSetSettings>(f => f
                    .AddSet(onderdeel)
                    .ChangeSettings(new FileEngineSetSettings() { ItemIsSubContent = true })
                    .AddItem(fragment)
                    .SetContent("txt", itemSubContent)
                    );
                var manager = FakeEngineManager(engine);
                var sut = (new Database.LiturgieDatabase(manager)) as ILiturgieDatabase.ILiturgieDatabase;

                var oplossing = sut.ZoekOnderdeel(onderdeel, fragment, fragmentDelen, _liturgieSettings);

                Assert.AreEqual(oplossing.Content.Count(), opgesplitstAls.Length);
                Assert.AreEqual(oplossing.Content.All(c => opgesplitstAls.Contains(c.Nummer.Value)), true);
            }

            [DataTestMethod]
            [DataRow("Johannes", "3")]
            public void BijbeltekstItem_Gevonden(string onderdeel, string fragment)
            {
                var engine = new EngineMock<FileEngineSetSettings>(f => f
                    .AddSet(onderdeel)
                    .AddItem(fragment)
                    .SetContent("txt", "lege reeks")
                    );
                var manager = FakeEngineManagerExtension(Database.LiturgieDatabaseSettings.DatabaseNameBijbeltekst, engine);
                var sut = (new Database.LiturgieDatabase(manager)) as ILiturgieDatabase.ILiturgieDatabase;

                var oplossing = sut.ZoekOnderdeel(VerwerkingType.bijbeltekst, onderdeel, fragment, null, _liturgieSettings);

                Assert.AreEqual(oplossing.Status, LiturgieOplossingResultaat.Opgelost);
            }

            [DataTestMethod]
            [DataRow("Johannes", "3", "2", "1 In den beginne 2 was het woord 3 en het woord was", 2, "was het woord")]
            [DataRow("Deutronomium", "10", "1", "1 Jakob nam 5 mannen mee. 2 Zij gingen een stukje lopen", 1, "Jakob nam 5 mannen mee.")]
            [DataRow("Deutronomium", "10", "1", "1 Jakob nam 5-7 mannen mee. 2 Zij gingen een stukje lopen", 1, "Jakob nam 5-7 mannen mee.")]
            [DataRow("Deutronomium", "10", "2", "1 Jakob nam 5 mannen mee. 2-3 Zij gingen een stukje lopen", 2, "Zij gingen een stukje lopen")]
            [DataRow("Deutronomium", "10", "4", "1 Jakob nam 5 mannen mee. 2-3 Zij gingen een stukje lopen, 4 het was fijn.", 4, "het was fijn.")]
            public void BijbeltekstItem_ItemIsSubcontent(string onderdeel, string fragment, string find, string inContent, int foundNumber, string foundContent)
            {
                var engine = new EngineMock<FileEngineSetSettings>(f => f
                    .AddSet(onderdeel)
                    .ChangeSettings(new FileEngineSetSettings() { ItemIsSubContent = true })
                    .AddItem(fragment)
                    .SetContent("txt", inContent)
                    );
                var manager = FakeEngineManagerExtension(Database.LiturgieDatabaseSettings.DatabaseNameBijbeltekst, engine);
                var delen = new[] { find };
                var sut = (new Database.LiturgieDatabase(manager)) as ILiturgieDatabase.ILiturgieDatabase;

                var oplossing = sut.ZoekOnderdeel(VerwerkingType.bijbeltekst, onderdeel, fragment, delen, _liturgieSettings);

                var eersteContent = oplossing.Content.FirstOrDefault();
                Assert.IsNotNull(eersteContent);
                Assert.AreEqual(eersteContent.Inhoud, foundContent);
            }
        }



        private static IEngineManager<T> FakeEngineManager<T>(IEngine<T> defaultEngine) where T : class, ISetSettings, new()
        {
            var manager = A.Fake<IEngineManager<T>>();
            var defaultReturn = A.Fake<IEngineSelection<T>>();
            A.CallTo(() => defaultReturn.Engine).Returns(defaultEngine);
            A.CallTo(() => defaultReturn.Name).Returns("default");
            A.CallTo(() => manager.GetDefault()).Returns(defaultReturn);
            return manager;
        }

        private static IEngineManager<T> FakeEngineManagerExtension<T>(string name, IEngine<T> engine) where T : class, ISetSettings, new()
        {
            var manager = A.Fake<IEngineManager<T>>();
            var defaultReturn = A.Fake<IEngineSelection<T>>();
            A.CallTo(() => defaultReturn.Engine).Returns(engine);
            A.CallTo(() => defaultReturn.Name).Returns(name);
            A.CallTo(() => manager.Extensions).Returns(new[] { defaultReturn });
            return manager;
        }

        private static ILiturgieSettings FakeLiturgieSettings()
        {
            var settings = A.Fake<ILiturgieSettings>();
            A.CallTo(() => settings.ToonBijbeltekstenInLiturgie).Returns(true);
            return settings;
        }
    }
}
