using FakeItEasy;
using Generator.Database.FileSystem;
using IDatabase;
using IDatabase.Engine;
using ILiturgieDatabase;
using NUnit.Framework;

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

        private static IEngineManager<T> FakeEngineManager<T>(IEngine<T> defaultEngine) where T : class, ISetSettings, new()
        {
            var manager = A.Fake<IEngineManager<T>>();
            var defaultReturn = A.Fake<IEngineSelection<T>>();
            A.CallTo(() => defaultReturn.Engine).Returns(defaultEngine);
            A.CallTo(() => defaultReturn.Name).Returns("default");
            A.CallTo(() => manager.GetDefault()).Returns(defaultReturn);
            return manager;
        }
    }
}
