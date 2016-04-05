using FakeItEasy;
using Generator.Database.FileSystem;
using IDatabase;
using IDatabase.Engine;
using ILiturgieDatabase;
using NUnit.Framework;
using System.Linq;

namespace Generator.Tests
{
    public class LiturgieDatabase
    {
        [TestCase("Psalm", "100")]
        public void ZoekOnderdeel_NormaalItem_Gevonden(string onderdeel, string fragment)
        {
            var engine = new EngineMock<FileEngineSetSettings>(f => f
                .AddSet(onderdeel)
                .AddItem(fragment)
                .SetContent("txt", "lege reeks")
                );
            var manager = FakeEngineManager(engine);
            var sut = (new Generator.Database.LiturgieDatabase(manager)) as ILiturgieDatabase.ILiturgieDatabase;

            var oplossing = sut.ZoekOnderdeel(onderdeel, fragment);

            Assert.That(oplossing.Status, Is.EqualTo(LiturgieOplossingResultaat.Opgelost));
        }

        [TestCase("Johannes", "3")]
        public void ZoekOnderdeel_BijbeltekstItem_Gevonden(string onderdeel, string fragment)
        {
            var engine = new EngineMock<FileEngineSetSettings>(f => f
                .AddSet(onderdeel)
                .AddItem(fragment)
                .SetContent("txt", "lege reeks")
                );
            var manager = FakeEngineManager(Database.LiturgieDatabaseSettings.DatabaseNameBijbeltekst, engine);
            var sut = (new Generator.Database.LiturgieDatabase(manager)) as ILiturgieDatabase.ILiturgieDatabase;

            var oplossing = sut.ZoekOnderdeel(VerwerkingType.bijbeltekst, onderdeel, fragment);

            Assert.That(oplossing.Status, Is.EqualTo(LiturgieOplossingResultaat.Opgelost));
        }

        [TestCase("Johannes", "3", "2", "1 In den beginne 2 was het woord 3 en het woord was", 2, "was het woord")]
        public void ZoekOnderdeel_BijbeltekstItem_ItemIsSubcontent(string onderdeel, string fragment, string find, string inContent, int foundNumber, string foundContent)
        {
            var engine = new EngineMock<FileEngineSetSettings>(f => f
                .AddSet(onderdeel)
                .ChangeSettings(new FileEngineSetSettings() { ItemIsSubContent = true })
                .AddItem(fragment)
                .SetContent("txt", inContent)
                );
            var manager = FakeEngineManager(Database.LiturgieDatabaseSettings.DatabaseNameBijbeltekst, engine);
            var delen = new[] { find };
            var sut = (new Generator.Database.LiturgieDatabase(manager)) as ILiturgieDatabase.ILiturgieDatabase;

            var oplossing = sut.ZoekOnderdeel(VerwerkingType.bijbeltekst, onderdeel, fragment, delen);

            Assert.That(oplossing.Content.FirstOrDefault().Inhoud, Is.EqualTo(foundContent));
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

        private static IEngineManager<T> FakeEngineManager<T>(string name, IEngine<T> engine) where T : class, ISetSettings, new()
        {
            var manager = A.Fake<IEngineManager<T>>();
            var defaultReturn = A.Fake<IEngineSelection<T>>();
            A.CallTo(() => defaultReturn.Engine).Returns(engine);
            A.CallTo(() => defaultReturn.Name).Returns(name);
            A.CallTo(() => manager.Extensions).Returns(new[] { defaultReturn });
            return manager;
        }
    }
}
