using IDatabase;
using IDatabase.Engine;
using ISettings;
using System.Collections.Generic;

namespace PowerpointGenerator.Database
{
    class EngineManager<T> : IEngineManager<T> where T : class, ISetSettings, new()
    {
        private readonly IInstellingenFactory _settings;
        private readonly Generator.Database.FileSystem.FileEngine<T>.Factory<T> _fac;

        private IEngineSelection<T> _default;
        private IEngineSelection<T> _bijbeltekst;

        public IEnumerable<IEngineSelection<T>> Extensions {
            get
            {
                yield return GetDefault();
                yield return GetBijbeltekst();
            }
        }

        public EngineManager(IInstellingenFactory settings, Generator.Database.FileSystem.FileEngine<T>.Factory<T> fac) {
            _settings = settings;
            _fac = fac;
        }

        public IEngineSelection<T> GetDefault()
        {
            if (_default == null)
                _default = new EngineSelection<T>()
                {
                    Name = "default",
                    Engine = _fac.Invoke(_settings.LoadFromXmlFile().FullDatabasePath, true)
                };
            return _default;
        }

        private IEngineSelection<T> GetBijbeltekst()
        {
            if (_bijbeltekst == null)
                _bijbeltekst = new EngineSelection<T>()
                {
                    Name = Generator.Database.LiturgieDatabaseSettings.DatabaseNameBijbeltekst,
                    Engine = _fac.Invoke(_settings.LoadFromXmlFile().FullBijbelPath, true)
                };
            return _bijbeltekst;
        }

        class EngineSelection<T2> : IEngineSelection<T2> where T2 : class, ISetSettings, new()
        {
            public IEngine<T2> Engine { get; set; }
            public string Name { get; set; }
        }
    }
}
