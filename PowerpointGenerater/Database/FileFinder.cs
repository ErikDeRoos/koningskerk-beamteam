using IDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PowerpointGenerater.Database
{

    public class FileFinder : IFinder
    {
        private string _atDir;
        private bool _itemsHaveSubContent;
        private bool _cached;
        public FileFinder(string atDir, bool itemsHaveSubContent, bool askCached)
        {
            _atDir = atDir;
            _itemsHaveSubContent = itemsHaveSubContent;
            _cached = askCached;
        }
        public IEnumerable<IDbItem> GetItems()
        {
            if (_itemsHaveSubContent)
                return Directory.GetDirectories(_atDir).Select(d => new FileBundledItem(d, _cached)).ToList();
            return Directory.GetFiles(_atDir).Select(d => new FileItem(d)).ToList();
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

            public string PersistentLink { get { return string.Empty; } }

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
