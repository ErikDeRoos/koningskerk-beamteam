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
        public void LosOp_NormaalItem_OpgezochtInDatabase(string naam, string deel)
        {
            var liturgieItem = A.Fake<ILiturgieInterpretatie>();
            A.CallTo(() => liturgieItem.Benaming).Returns(naam);
            A.CallTo(() => liturgieItem.Deel).Returns(deel);
            var result = A.Fake<IDbItem>();
            A.CallTo(() => result.Name).Returns(deel);
            var dbSet = A.Fake<IDbSet<FileEngineSetSettings>>();
            A.CallTo(() => dbSet.Name).Returns(naam);
            A.CallTo(() => dbSet.Where(a => true)).Returns(new[] { result });
            var fileEngine = A.Fake<IEngine<FileEngineSetSettings>>();
            A.CallTo(() => fileEngine.Where(a => true)).Returns(new[] { dbSet });
            var sut = (new Database.LiturgieDatabase(fileEngine)) as ILiturgieLosOp;

            var oplossing = sut.LosOp(liturgieItem);

            A.CallTo(() => result.Content).MustHaveHappened();
        }


        private static IEngine<FileEngineSetSettings> GetEngine(string dbSetName, string deelName)
        {
            var result = A.Fake<IDbItem>();
            A.CallTo(() => result.Name).Returns(deelName);
            var dbSet = A.Fake<IDbSet<FileEngineSetSettings>>();
            A.CallTo(() => dbSet.Name).Returns(dbSetName);
            A.CallTo(() => dbSet.Where(a => true)).Returns(new[] { result });
            var fileEngine = A.Fake<IEngine<FileEngineSetSettings>>();
            A.CallTo(() => fileEngine.Where(a => true)).Returns(new[] { dbSet });
            return fileEngine;
        }
    }
}
