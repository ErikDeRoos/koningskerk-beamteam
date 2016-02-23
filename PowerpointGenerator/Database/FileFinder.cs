using IDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PowerpointGenerator.Database
{

    public class FileFinder : IFinder
    {
        private readonly string _atDir;
        private readonly bool _itemsHaveSubContent;
        private readonly bool _cached;
        public FileFinder(string atDir, bool itemsHaveSubContent, bool askCached)
        {
            _atDir = atDir;
            _itemsHaveSubContent = itemsHaveSubContent;
            _cached = askCached;
        }
        /// <remarks>cache is geregeld door FileEngine die deze aanroep doet</remarks>
        public IEnumerable<IDbItem> GetItems()
        {
            if (_itemsHaveSubContent)
                return Directory.GetDirectories(_atDir).Select(d => new FileBundledItem(d, _cached)).ToList();
            return Directory.GetFiles(_atDir).Select(d => new FileItem(d)).ToList();
        }
    }

    public class FileBundledItem : IDbItem
    {
        public string Name { get; }
        public IDbItemContent Content { get; }

        internal FileBundledItem(string dirPath, bool cached)
        {
            Name = FileEngineDefaults.ClosestPathName(dirPath);
            Content = new DirContent(dirPath, cached);
        }

        private class DirContent : IDbItemContent
        {
            private readonly string _inDir;
            private readonly bool _cached;
            private IEnumerable<IDbItem> _itemCache;

            public string Type => FileEngineDefaults.BundleTypeDir;

            public Stream Content => new MemoryStream();

            public string PersistentLink => string.Empty;

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
                return _itemCache ?? (_itemCache = GetItems(_inDir));
            }
        }
    }

    class FileItem : IDbItem
    {
        public string Name { get; }
        public IDbItemContent Content { get; }

        public FileItem(string filePath)
        {
            Name = Path.GetFileNameWithoutExtension(filePath);
            Content = new FileContent(filePath);
        }
        class FileContent : IDbItemContent
        {
            private readonly string _filePath;

            public string Type { get; }
            public Stream Content => ReadFile(_filePath);
            public string PersistentLink => _filePath;

            public FileContent(string filePath)
            {
                _filePath = filePath;

                Type = _filePath != null ? Path.GetExtension(_filePath).Substring(1) : string.Empty;
            }

            private static Stream ReadFile(string filePath)
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
