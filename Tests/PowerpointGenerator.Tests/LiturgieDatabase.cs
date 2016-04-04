using Generator.Database.FileSystem;
using IDatabase;
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
            var sut = (new Generator.Database.LiturgieDatabase(engine)) as ILiturgieDatabase.ILiturgieDatabase;

            var oplossing = sut.ZoekOnderdeel(onderdeel, fragment);

            Assert.That(oplossing.Status, Is.EqualTo(LiturgieOplossingResultaat.Opgelost));
        }
    }
}
