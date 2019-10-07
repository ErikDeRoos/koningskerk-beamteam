// Copyright 2019 door Erik de Roos
using IDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using IFileSystem;
using IDatabase.Engine;

namespace Generator.Database.FileSystem
{
    public static class FileEngineDefaults
    {
        public const string BundleTypeDir = "<dir>";
        public const string CommonFilesSetName = "Common";
        public const string SetSettingsName = "instellingen.xml";
        public const string SetArchiveName = "inhoud.zip";
        public static readonly char NotSafe = ' ';
        public static readonly char Safe = '_';

        private static readonly char[] pathSeparators = new char[] { Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar };

        public static string ClosestPathName(string fromPath)
        {
            return fromPath.Split(pathSeparators).Last();
        }

        public static string CreateSafeName(string name)
        {
            return (name ?? string.Empty).Replace(NotSafe, Safe).ToLower();
        }
    }


    /// <summary>
    /// File-system gebaseerde database. Lazy loading. Read-only.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FileEngine<T> : IEngine<T> where T : class, ISetSettings, new()
    {
        public delegate IEngine<T2> Factory<T2>(string databasePad, bool cached) where T2 : class, ISetSettings, new();

        private bool _cached { get; set; }
        private IEnumerable<IDbSet<T>> _dirCache;
        private string _startDir;
        private IFileOperations _fileManager;

        /// <param name="cached">
        /// True = cache structure (paths, filetrees and settings), 
        /// content is never cached but combination of on-access loading and lazy loading instead.
        /// </param>
        public FileEngine(IFileOperations fileManager, string databasePad, bool cached)
        {
            _fileManager = fileManager;
            _startDir = databasePad;
            _cached = cached;
        }

        private IEnumerable<FileSet<T>> GetDirs(string startDir, bool askCached)
        {
            if (!_fileManager.DirExists(startDir))
                return new List<FileSet<T>>();
            return _fileManager.GetDirectories(startDir).Select(d => new FileSet<T>(_fileManager, d, askCached)).ToList();
        }

        private IEnumerable<IDbSet<T>> GetDbSet()
        {
            if (!_cached)
                return GetDirs(_startDir, _cached);
            if (_dirCache == null)
                _dirCache = GetDirs(_startDir, _cached);
            return _dirCache;
        }

        public IEnumerable<IDbSet<T>> Where(Func<IDbSet<T>, bool> query)
        {
            return GetDbSet().Where(query);
        }

        public IEnumerable<string> GetAllNames()
        {
            return GetDbSet()
                .Select(db => db.Name)
                .ToList();
        }
    }

    class FileSet<T> : IDbSet<T> where T : class, ISetSettings, new()
    {
        private string _inDir;
        private bool _cached;
        private IEnumerable<IDbItem> _itemCache;
        private T _settingsCached;
        private IFinder _finderCached;
        private IFileOperations _fileManager;

        public string Name { get; }
        public string SafeName { get; }
        public T Settings { get { return GetSettings(_cached); } set { ChangeSettings(value, _cached); } }

        public FileSet(IFileOperations fileManager, string inDir, bool cached)
        {
            _fileManager = fileManager;
            _inDir = inDir;
            _cached = cached;

            Name = FileEngineDefaults.ClosestPathName(_inDir);
            SafeName = FileEngineDefaults.CreateSafeName(Name);
        }

        private IFinder GetFinder()
        {
            if (_cached && _finderCached != null)
                return _finderCached;
            var finder = (IFinder)null;
            if (Settings.UseContainer)
                finder = new FileZipFinder(_fileManager, _inDir, FileEngineDefaults.SetArchiveName, Settings.ItemsHaveSubContent, _cached);
            else
                finder = new FileFinder(_fileManager, _inDir, Settings.ItemsHaveSubContent, _cached);
            if (_cached)
                _finderCached = finder;
            return finder;
        }

        private IEnumerable<IDbItem> GetItemSet()
        {
            if (!_cached)
                return GetFinder().GetItems();
            if (_itemCache == null)
                _itemCache = GetFinder().GetItems();
            return _itemCache;
        }

        public IEnumerable<IDbItem> Where(Func<IDbItem, bool> query)
        {
            return GetItemSet().Where(query);
        }

        private T GetSettings(bool cached)
        {
            if (cached)
            {
                if (_settingsCached == null)
                    _settingsCached = GetSettings(false);
                return _settingsCached;
            }

            var fileName = _fileManager.CombineDirectories(_inDir, FileEngineDefaults.SetSettingsName);
            if (!_fileManager.FileExists(fileName))
                ChangeSettings(new T(), false);
            try {
                var serializer = new XmlSerializer(typeof(T));
                var settings = new XmlReaderSettings();
                using (var textReader = new StreamReader(_fileManager.FileReadStream(fileName)))
                {
                    var xmlReader = XmlReader.Create(textReader, settings);
                    return (serializer.Deserialize(xmlReader) as T) ?? new T();
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
                var fileName = _fileManager.CombineDirectories(_inDir, FileEngineDefaults.SetSettingsName);
                var serializer = new XmlSerializer(typeof(T));
                using (var sw = new StreamWriter(_fileManager.FileWriteStream(fileName)))
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

        public IEnumerable<string> GetAllNames()
        {
            return GetItemSet()
                .Select(db => db.Name);
        }
    }

    interface IFinder
    {
        IEnumerable<IDbItem> GetItems();
    }
}
