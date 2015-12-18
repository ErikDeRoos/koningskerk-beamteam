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

        public static string ClosestPathName(string fromPath)
        {
            return fromPath.Split(new[] { Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar }).Last();
        }
    }


    public class FileEngine<T> : IEngine<T> where T : class, ISetSettings, new()
    {
        public bool Cached { get; set; }
        private IEnumerable<IDbSet<T>> _dirCache;
        private IInstellingenFactory _instellingenFactory;

        /// <param name="cached">True = cache structure (paths, filetrees and settings). Content is never cached.</param>
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
            var startDir = _instellingenFactory.LoadFromXMLFile().FullDatabasePath;  // on-the-fly instellingen benaderen
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

        public string Name { get; private set; }
        public T Settings { get { return GetSettings(_cached); } set { ChangeSettings(value, _cached); } }

        public FileSet(string inDir, bool cached)
        {
            _inDir = inDir;
            _cached = cached;

            Name = FileEngineDefaults.ClosestPathName(_inDir);
        }

        private static IEnumerable<IDbItem> GetItems(string atDir, bool itemsHaveSubContent, bool askCached)
        {
            if (itemsHaveSubContent)
                return Directory.GetDirectories(atDir).Select(d => new FileBundledItem(d, askCached)).ToList();
            return Directory.GetFiles(atDir).Select(d => new FileItem(d)).ToList();
        }

        public IEnumerable<IDbItem> Where(Func<IDbItem, bool> query)
        {
            if (!_cached)
                return GetItems(_inDir, Settings.ItemsHaveSubContent, _cached);
            if (_itemCache == null)
                _itemCache = GetItems(_inDir, Settings.ItemsHaveSubContent, _cached);
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
                return new T();
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

        private void ChangeSettings(T newSettings, bool cached)
        {
            if (cached)
                _settingsCached = newSettings;

            var fileName = Path.Combine(_inDir, FileEngineDefaults.SetSettingsName);
            var serializer = new XmlSerializer(typeof(T));
            using (TextWriter sw = new StreamWriter(fileName))
            {
                serializer.Serialize(sw, this);
                sw.Flush();
            }
        }
    }

    public class FileBundledItem : IDbItem
    {
        private string _inDir;
        private bool _cached;

        public string Name { get; private set; }
        public IDbItemContent Content { get; private set; }

        internal FileBundledItem(string dirPath, bool cached)
        {
            _inDir = dirPath;
            _cached = cached;

            Name = FileEngineDefaults.ClosestPathName(dirPath);
            Content = new DirContent(_inDir, cached);
        }

        class DirContent : IDbItemContent
        {
            private string _inDir;
            private bool _cached;
            private IEnumerable<IDbItem> _itemCache;

            public string Type { get { return FileEngineDefaults.BundleTypeDir; } }

            public Stream Content { get { return new MemoryStream(); } }

            public string PersistentLink { get { return String.Empty; } }

            public DirContent(string dirPath, bool cached)
            {
                _inDir = dirPath;
                _cached = cached;
            }

            private static IEnumerable<IDbItem> GetItems(string atDir)
            {
                return Directory.GetFiles(atDir).Select(d => new FileItem(d)).ToList();
            }

            public IEnumerable<IDbItem> TryAccessSubs()
            {
                if (!_cached)
                    return GetItems(_inDir);
                if (_itemCache == null)
                    _itemCache = GetItems(_inDir);
                return _itemCache;
            }
        }
    }

    class FileItem : IDbItem
    {
        private string _filePath;

        public string Name { get; private set; }
        public IDbItemContent Content { get; private set; }

        public FileItem(string filePath)
        {
            _filePath = filePath;

            Name = Path.GetFileNameWithoutExtension(filePath);
            Content = new FileContent(filePath);
        }
        class FileContent : IDbItemContent
        {
            private string _filePath;

            public string Type { get; private set; }
            public Stream Content { get { return ReadFile(_filePath); } }
            public string PersistentLink { get { return _filePath; } }

            public FileContent(string filePath)
            {
                _filePath = filePath;

                Type = Path.GetExtension(_filePath).Substring(1);  // remove dot
            }

            public static Stream ReadFile(string filePath)
            {
                return new FileStream(filePath, FileMode.Open);
            }

            public IEnumerable<IDbItem> TryAccessSubs()
            {
                return new List<IDbItem>();
            }
        }
    }
}
