// Copyright 2016 door Erik de Roos
using IDatabase;
using IFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Generator.Database.FileSystem
{

    public class FileFinder : IFinder
    {
        private readonly string _atDir;
        private readonly bool _itemsHaveSubContent;
        private readonly bool _cached;
        private IFileOperations _fileManager;

        public FileFinder(IFileOperations fileManager, string atDir, bool itemsHaveSubContent, bool askCached)
        {
            _fileManager = fileManager;
            _atDir = atDir;
            _itemsHaveSubContent = itemsHaveSubContent;
            _cached = askCached;
        }
        /// <remarks>cache is geregeld door FileEngine die deze aanroep doet</remarks>
        public IEnumerable<IDbItem> GetItems()
        {
            if (_itemsHaveSubContent)
                return _fileManager.GetDirectories(_atDir).Select(d => new FileBundledItem(_fileManager, d, _cached)).ToList();
            return _fileManager.GetFiles(_atDir).Select(d => new FileItem(_fileManager, d)).ToList();
        }
    }

    public class FileBundledItem : IDbItem
    {
        public string Name { get; }
        public IDbItemContent Content { get; }
        private IFileOperations _fileManager;

        internal FileBundledItem(IFileOperations fileManager, string dirPath, bool cached)
        {
            _fileManager = fileManager;
            Name = FileEngineDefaults.ClosestPathName(dirPath);
            Content = new DirContent(_fileManager, dirPath, cached);
        }

        private class DirContent : IDbItemContent
        {
            private readonly string _inDir;
            private readonly bool _cached;
            private IEnumerable<IDbItem> _itemCache;
            private IFileOperations _fileManager;

            public string Type => FileEngineDefaults.BundleTypeDir;

            public string PersistentLink => string.Empty;

            public DirContent(IFileOperations fileManager, string dirPath, bool cached)
            {
                _fileManager = fileManager;
                _inDir = dirPath;
                _cached = cached;
            }

            public Stream GetContentStream()
            {
                return new MemoryStream();
            }

            public IEnumerable<IDbItem> TryAccessSubs()
            {
                if (!_cached)
                    return GetItems(_inDir);
                return _itemCache ?? (_itemCache = GetItems(_inDir));
            }

            private IEnumerable<IDbItem> GetItems(string atDir)
            {
                return _fileManager.GetFiles(atDir).Select(d => new FileItem(_fileManager, d)).ToList();
            }
        }
    }

    class FileItem : IDbItem
    {
        public string Name { get; }
        public IDbItemContent Content { get; }
        private IFileOperations _fileManager;

        public FileItem(IFileOperations fileManager, string filePath)
        {
            _fileManager = fileManager;
            Name = Path.GetFileNameWithoutExtension(filePath);
            Content = new FileContent(_fileManager, filePath);
        }
        class FileContent : IDbItemContent
        {
            private readonly string _filePath;
            private IFileOperations _fileManager;

            public string Type { get; }
            public string PersistentLink => _filePath;

            public FileContent(IFileOperations fileManager, string filePath)
            {
                _fileManager = fileManager;
                _filePath = filePath;

                Type = _filePath != null ? Path.GetExtension(_filePath).Substring(1) : string.Empty;
            }

            public Stream GetContentStream()
            {
                return ReadFile(_filePath);
            }

            private Stream ReadFile(string filePath)
            {
                // Open a new stream and return the stream
                return _fileManager.FileReadStream(filePath);
            }

            public IEnumerable<IDbItem> TryAccessSubs()
            {
                return new List<IDbItem>();
            }
        }
    }
}
