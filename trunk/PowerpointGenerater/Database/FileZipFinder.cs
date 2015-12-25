using IDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;

namespace PowerpointGenerater.Database
{
    public class FileZipFinder : IFinder, IDisposable
    {
        private string _atDir;
        private string _setName;
        private bool _itemsHaveSubContent;
        private bool _cached;
        private Stream _archiveStream;
        private ZipArchive _archive;
        public FileZipFinder(string atDir, string setName, bool itemsHaveSubContent, bool askCached)
        {
            _atDir = atDir;
            _setName = setName;
            _itemsHaveSubContent = itemsHaveSubContent;
            _cached = askCached;
        }

        private ZipArchive GetZip()
        {
            if (_archiveStream == null)
                _archiveStream = new FileStream(Path.Combine(_atDir, _setName), FileMode.Open, FileAccess.Read);
            if (_archive == null)
                _archive = new ZipArchive(_archiveStream, ZipArchiveMode.Read);
            return _archive;
        }

        public IEnumerable<IDbItem> GetItems()
        {
            var archive = GetZip();
            if (_itemsHaveSubContent)
                return archive.Entries.AsDirectoryStructure().Directories.Select(d => new FileZipBundledItem(d, _cached)).ToList();
            return archive.Entries.Select(e => new FileZipItem(e)).ToList();
        }

        public void Dispose()
        {
            if (_archiveStream != null)
                _archiveStream.Dispose();
        }
    }

    public class FileZipBundledItem : IDbItem
    {
        private IZipArchiveDirectory _inDir;
        private bool _cached;

        public string Name { get; private set; }
        public IDbItemContent Content { get; private set; }

        internal FileZipBundledItem(IZipArchiveDirectory archiveDir, bool cached)
        {
            _inDir = archiveDir;
            _cached = cached;

            Name = _inDir.Name;
            Content = new DirContent(_inDir.Entries, cached);
        }

        class DirContent : IDbItemContent
        {
            private IEnumerable<ZipArchiveEntry> _entries;
            private bool _cached;
            private IEnumerable<IDbItem> _itemCache;

            public string Type { get { return FileEngineDefaults.BundleTypeDir; } }

            public Stream Content { get { return new MemoryStream(); } }

            public string PersistentLink { get { return string.Empty; } }

            public DirContent(IEnumerable<ZipArchiveEntry> fileEntries, bool cached)
            {
                _entries = fileEntries;
                _cached = cached;
            }

            private static IEnumerable<IDbItem> GetItems(IEnumerable<ZipArchiveEntry> entries)
            {
                return entries.Select(d => new FileZipItem(d)).ToList();
            }

            public IEnumerable<IDbItem> TryAccessSubs()
            {
                if (!_cached)
                    return GetItems(_entries);
                if (_itemCache == null)
                    _itemCache = GetItems(_entries);
                return _itemCache;
            }
        }
    }

    class FileZipItem : IDbItem
    {
        private ZipArchiveEntry _entry;

        public string Name { get; private set; }
        public IDbItemContent Content { get; private set; }

        public FileZipItem(ZipArchiveEntry entry)
        {
            _entry = entry;

            Name = Path.GetFileNameWithoutExtension(_entry.Name);
            Content = new FileContent(_entry);
        }
        class FileContent : IDbItemContent
        {
            private ZipArchiveEntry _entry;

            public string Type { get; private set; }
            public Stream Content { get { return _entry.Open(); } }
            public string PersistentLink { get { return _entry.FullName; } }

            public FileContent(ZipArchiveEntry entry)
            {
                _entry = entry;

                Type = Path.GetExtension(entry.Name).Substring(1);  // remove dot
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
