using IDatabase;
using IDatabase.Engine;
using ISettings;
using System;
using System.Collections.Generic;

namespace PowerpointGenerator.Database
{
    class EngineManager<T> : IEngineManager<T> where T : class, ISetSettings, new()
    {
        private IEngineSelection<T> _default;

        public EngineManager(IInstellingenFactory settings, Generator.Database.FileSystem.FileEngine<T>.Factory<T> fac) {
            var baseDir = settings.LoadFromXmlFile().FullDatabasePath;
            _default = new EngineSelection<T>() {
                Name = "default",
                Engine = fac.Invoke(baseDir, true)
            };
        }


        public IEnumerable<IEngineSelection<T>> Extensions
        {
            get
            {
                throw new NotImplementedException();
            }
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
