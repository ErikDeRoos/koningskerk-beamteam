using IDatabase;
using IDatabase.Engine;
using ISettings;
using System.Collections.Generic;

namespace PowerpointGenerator.Database
{
    class EngineManager<T> : IEngineManager<T> where T : class, ISetSettings, new()
    {
        private IEngineSelection<T> _default;

        public IEnumerable<IEngineSelection<T>> Extensions { get; private set; }

        public EngineManager(IInstellingenFactory settings, Generator.Database.FileSystem.FileEngine<T>.Factory<T> fac) {
            _default = new EngineSelection<T>() {
                Name = "default",
                Engine = fac.Invoke(settings.LoadFromXmlFile().FullDatabasePath, true)
            };
            Extensions = new List<EngineSelection<T>>()
            {
                new EngineSelection<T>() {
                    Name = Generator.Database.LiturgieDatabaseSettings.DatabaseNameBijbeltekst,
                    Engine = fac.Invoke(settings.LoadFromXmlFile().BijbelPad, true)
                },
            };
        }

        public IEngineSelection<T> GetDefault()
        {
            return _default;
        }

        class EngineSelection<T2> : IEngineSelection<T2> where T2 : class, ISetSettings, new()
        {
            public IEngine<T2> Engine { get; set; }
            public string Name { get; set; }
        }
    }
}
