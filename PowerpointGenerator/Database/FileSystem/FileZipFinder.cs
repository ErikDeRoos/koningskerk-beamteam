using IDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;
using IFileSystem;

namespace PowerpointGenerator.Database.FileSystem
{
    public class FileZipFinder : IFinder, IDisposable
    {
        private readonly string _atDir;
        private readonly string _setName;
        private readonly bool _itemsHaveSubContent;
        private readonly bool _cached;
        private Stream _archiveStream;
        private ZipArchive _archive;
        private IFileOperations _fileManager;

        public FileZipFinder(IFileOperations fileManager, string atDir, string setName, bool itemsHaveSubContent, bool askCached)
        {
            _fileManager = fileManager;
            _atDir = atDir;
            _setName = setName;
            _itemsHaveSubContent = itemsHaveSubContent;
            _cached = askCached;
        }

        /// <remarks>cache is geregeld door FileEngine die deze aanroep doet</remarks>
        public IEnumerable<IDbItem> GetItems()
        {
            var archive = GetZip();
            if (_itemsHaveSubContent)
            {
                var dirStructure = archive.Entries.AsDirectoryStructure();
                return dirStructure.Directories.Select(d => new FileZipBundledItem(d, _cached)).ToList();
            }
            return archive.Entries.Select(e => new FileZipItem(e)).ToList();
        }

        private ZipArchive GetZip()
        {
            if (_archiveStream == null)
                _archiveStream = _fileManager.FileReadStream(_fileManager.CombineDirectories(_atDir, _setName));
            return _archive ?? (_archive = new ZipArchive(_archiveStream, ZipArchiveMode.Read));
        }

        public void Dispose()
        {
            _archiveStream?.Dispose();
        }
    }

    public class FileZipBundledItem : IDbItem
    {
        public string Name { get; }
        public IDbItemContent Content { get; }

        internal FileZipBundledItem(IZipArchiveDirectory archiveDir, bool cached)
        {
            var inDir = archiveDir;

            Name = inDir.Name;
            Content = new DirContent(inDir.Entries, cached);
        }

        class DirContent : IDbItemContent
        {
            private readonly IEnumerable<ZipArchiveEntry> _entries;
            private readonly bool _cached;
            private IEnumerable<IDbItem> _itemCache;

            public string Type => FileEngineDefaults.BundleTypeDir;

            public Stream Content => new MemoryStream();

            public string PersistentLink => string.Empty;

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
                return _itemCache ?? (_itemCache = GetItems(_entries));
            }
        }
    }

    class FileZipItem : IDbItem
    {
        public string Name { get; }
        public IDbItemContent Content { get; }

        public FileZipItem(ZipArchiveEntry entry)
        {
            var entry1 = entry;

            Name = Path.GetFileNameWithoutExtension(entry1.Name);
            Content = new FileContent(entry1);
        }

        private class FileContent : IDbItemContent
        {
            private readonly ZipArchiveEntry _entry;

            public string Type { get; }
            public Stream Content => _entry.Open();
            public string PersistentLink => _entry.FullName;

            public FileContent(ZipArchiveEntry entry)
            {
                _entry = entry;

                Type = entry != null && entry.Name != null ? Path.GetExtension(entry.Name).Substring(1) : string.Empty;  // remove dot
            }

            public IEnumerable<IDbItem> TryAccessSubs()
            {
                return new List<IDbItem>();
            }
        }
    }
}
