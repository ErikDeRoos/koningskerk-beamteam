using IDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using ISettings;

namespace PowerpointGenerater.Database
{
    public static class FileEngineDefaults
    {
        public const string BundleTypeDir = "<dir>";
        public const string CommonFilesSetName = "Common";
        public const string SetSettingsName = "instellingen.xml";
        public const string SetArchiveName = "inhoud.zip";

        private static readonly char[] pathSeparators = new char[] { Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar };

        public static string ClosestPathName(string fromPath)
        {
            return fromPath.Split(pathSeparators).Last();
        }
    }


    /// <summary>
    /// File-system gebaseerde database. Lazy loading. Caching default aan. Read-only.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FileEngine<T> : IEngine<T> where T : class, ISetSettings, new()
    {
        public bool Cached { get; set; }
        private IEnumerable<IDbSet<T>> _dirCache;
        private IInstellingenFactory _instellingenFactory;

        /// <param name="cached">
        /// True = cache structure (paths, filetrees and settings), 
        /// content is never cached but combination of on-access loading and lazy loading instead.
        /// </param>
        public FileEngine(IInstellingenFactory instellingenFactory)
        {
            _instellingenFactory = instellingenFactory;
            Cached = true;
        }

        private static IEnumerable<FileSet<T>> GetDirs(string startDir, bool askCached)
        {
            return Directory.GetDirectories(startDir).Select(d => new FileSet<T>(d, askCached)).ToList();
        }

        public IEnumerable<IDbSet<T>> Where(Func<IDbSet<T>, bool> query)
        {
            var startDir = _instellingenFactory.LoadFromXmlFile().FullDatabasePath;  // on-the-fly instellingen benaderen
            if (!Cached)
                return GetDirs(startDir, Cached);
            if (_dirCache == null)
                _dirCache = GetDirs(startDir, Cached);
            return _dirCache.Where(query);
        }
    }

    class FileSet<T> : IDbSet<T> where T : class, ISetSettings, new()
    {
        private string _inDir;
        private bool _cached;
        private IEnumerable<IDbItem> _itemCache;
        private T _settingsCached;
        private IFinder _finderCached;

        public string Name { get; private set; }
        public T Settings { get { return GetSettings(_cached); } set { ChangeSettings(value, _cached); } }

        public FileSet(string inDir, bool cached)
        {
            _inDir = inDir;
            _cached = cached;

            Name = FileEngineDefaults.ClosestPathName(_inDir);
        }

        private IFinder GetFinder()
        {
            if (_cached && _finderCached != null)
                return _finderCached;
            var finder = (IFinder)null;
            if (Settings.UseContainer)
                finder = new FileZipFinder(_inDir, FileEngineDefaults.SetArchiveName, Settings.ItemsHaveSubContent, _cached);
            else
                finder = new FileFinder(_inDir, Settings.ItemsHaveSubContent, _cached);
            if (_cached)
                _finderCached = finder;
            return finder;
        }

        public IEnumerable<IDbItem> Where(Func<IDbItem, bool> query)
        {
            if (!_cached)
                return GetFinder().GetItems();
            if (_itemCache == null)
                _itemCache = GetFinder().GetItems();
            return _itemCache.Where(query);
        }

        private T GetSettings(bool cached)
        {
            if (cached)
            {
                if (_settingsCached == null)
                    _settingsCached = GetSettings(false);
                return _settingsCached;
            }

            var fileName = Path.Combine(_inDir, FileEngineDefaults.SetSettingsName);
            if (!File.Exists(fileName))
                ChangeSettings(new T(), false);
            try {
                var serializer = new XmlSerializer(typeof(T));
                var settings = new XmlReaderSettings();
                using (var textReader = new StreamReader(fileName))
                {
                    using (var xmlReader = XmlReader.Create(textReader, settings))
                    {
                        return (serializer.Deserialize(xmlReader) as T) ?? new T();
                    }
                }
            }
            catch (InvalidOperationException)  // XML niet in juiste format
            {
                var nieuw = new T();
                ChangeSettings(nieuw, false);
                return nieuw;
            }
        }

        private void ChangeSettings(T newSettings, bool cached)
        {
            try {
                var fileName = Path.Combine(_inDir, FileEngineDefaults.SetSettingsName);
                var serializer = new XmlSerializer(typeof(T));
                using (TextWriter sw = new StreamWriter(fileName))
                {
                    serializer.Serialize(sw, newSettings);
                    sw.Flush();
                }
            }
            finally
            {
                if (cached)
                    _settingsCached = newSettings;
            }
        }
    }

    interface IFinder
    {
        IEnumerable<IDbItem> GetItems();
    }
}
