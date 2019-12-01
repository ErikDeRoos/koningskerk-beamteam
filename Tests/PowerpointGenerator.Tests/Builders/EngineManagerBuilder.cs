// Copyright 2019 door Erik de Roos
using Generator.Database.FileSystem;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Generator.Tests.Builders
{
    public class EngineManagerBuilder
    {
        private readonly Mock<IEngine> _engine = new Mock<IEngine>();
        private readonly List<IDbSet> _onderdelen = new List<IDbSet>();

        public IEngineManager Build()
        {
            _engine.Setup(x => x.Where(It.IsAny<Func<IDbSet, bool>>()))
                .Returns<Func<IDbSet, bool>>(_onderdelen.Where);

            return MockEngineManager(_engine.Object);
        }
        public IEngineManager Build(string engineName)
        {
            _engine.Setup(x => x.Where(It.IsAny<Func<IDbSet, bool>>()))
                .Returns<Func<IDbSet, bool>>(_onderdelen.Where);

            return MockEngineManagerExtension(engineName, _engine.Object);
        }

        public EngineManagerBuilder AddOnderdeelAndFragment(string onderdeelNaam, string fragmentNaam, string content = "lege reeks", bool itemIsSubcontent = false)
        {
            var set = new Mock<IDbSet>();
            set.SetupGet(x => x.Name).Returns(new DbItemName { Name = onderdeelNaam, SafeName = onderdeelNaam });
            set.Setup(x => x.Settings).Returns(new DbSetSettings { DisplayName = onderdeelNaam, ItemIsSubContent = itemIsSubcontent });

            var fragmentenLijst = new List<IDbItem>();
            var fragment = new Mock<IDbItem>();
            fragment.SetupGet(x => x.Name).Returns(new DbItemName { Name = fragmentNaam, SafeName = fragmentNaam });
            fragment.Setup(x => x.Content).Returns(MockContent(content));
            fragmentenLijst.Add(fragment.Object);
            set.Setup(x => x.Where(It.IsAny<Func<IDbItem, bool>>()))
                .Returns<Func<IDbItem, bool>>(fragmentenLijst.Where);

            _onderdelen.Add(set.Object);
            return this;
        }


        private static IDbItemContent MockContent(string content)
        {
            var cont = new Mock<IDbItemContent>();
            cont.SetupGet(x => x.Type).Returns("txt");
            cont.Setup(x => x.GetContentStream()).Returns(() => {
                var memStream = new MemoryStream();
                var writer = new StreamWriter(memStream);
                writer.Write(content);
                writer.Flush();
                memStream.Seek(0, SeekOrigin.Begin);
                return memStream;
            });
            return cont.Object;
        }


        private static IEngineManager MockEngineManager(IEngine defaultEngine)
        {
            var defaultReturn = new Mock<IEngineSelection>();
            defaultReturn.SetupGet(x => x.Engine).Returns(defaultEngine);
            defaultReturn.SetupGet(x => x.Name).Returns("default");

            var manager = new Mock<IEngineManager>();
            manager.Setup(x => x.GetDefault()).Returns(defaultReturn.Object);
            return manager.Object;
        }

        private static IEngineManager MockEngineManagerExtension(string name, IEngine engine)
        {
            var defaultReturn = new Mock<IEngineSelection>();
            defaultReturn.SetupGet(x => x.Engine).Returns(engine);
            defaultReturn.SetupGet(x => x.Name).Returns(name);

            var manager = new Mock<IEngineManager>();
            manager.SetupGet(x => x.Extensions).Returns(new[] { defaultReturn.Object });
            return manager.Object;
        }
    }
}
