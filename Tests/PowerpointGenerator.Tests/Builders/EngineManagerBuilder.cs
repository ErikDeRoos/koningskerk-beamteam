using FakeItEasy;
using IDatabase;
using IDatabase.Engine;
using ILiturgieDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.Tests.Builders
{
    public class EngineManagerBuilder
    {
        private readonly IEngine _engine;

        public EngineManagerBuilder()
        {
            _engine = A.Fake<IEngine>();
        }

        public IEngineManager Build()
        {
            return FakeEngineManager(_engine);
        }

        public EngineManagerBuilder AddOnderdeelAndFragment(string onderdeel, string fragment, string content = "lege reeks")
        {
            A.CallTo(() => _engine.)
        }


        private static IEngineManager FakeEngineManager(IEngine defaultEngine)
        {
            var manager = A.Fake<IEngineManager>();
            var defaultReturn = A.Fake<IEngineSelection>();
            A.CallTo(() => defaultReturn.Engine).Returns(defaultEngine);
            A.CallTo(() => defaultReturn.Name).Returns("default");
            A.CallTo(() => manager.GetDefault()).Returns(defaultReturn);
            return manager;
        }

        private static IEngineManager FakeEngineManagerExtension(string name, IEngine engine)
        {
            var manager = A.Fake<IEngineManager>();
            var defaultReturn = A.Fake<IEngineSelection>();
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
