// Copyright 2019 door Erik de Roos
using Generator.Database.FileSystem;
using Generator.Database.Models;
using ISettings;
using System.Collections.Generic;

namespace PowerpointGenerator.Genereren
{
    class EngineManager : IEngineManager
    {
        private readonly IInstellingenFactory _settings;
        private readonly FileEngine.Factory _fac;

        private IEngineSelection _default;
        private IEngineSelection _bijbeltekst;

        public IEnumerable<IEngineSelection> Extensions {
            get
            {
                yield return GetDefault();
                yield return GetBijbeltekst();
            }
        }

        public EngineManager(IInstellingenFactory settings, FileEngine.Factory fac) {
            _settings = settings;
            _fac = fac;
        }

        public IEngineSelection GetDefault()
        {
            if (_default == null)
                _default = new EngineSelection()
                {
                    Name = LiturgieDatabaseSettings.DatabaseNameDefault,
                    Engine = _fac.Invoke(_settings.LoadFromFile().FullDatabasePath, true)
                };
            return _default;
        }

        private IEngineSelection GetBijbeltekst()
        {
            if (_bijbeltekst == null)
                _bijbeltekst = new EngineSelection()
                {
                    Name = LiturgieDatabaseSettings.DatabaseNameBijbeltekst,
                    Engine = _fac.Invoke(_settings.LoadFromFile().FullBijbelPath, true)
                };
            return _bijbeltekst;
        }

        class EngineSelection : IEngineSelection
        {
            public IEngine Engine { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
