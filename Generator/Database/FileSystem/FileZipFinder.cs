// Copyright 2019 door Erik de Roos
using IDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;
using IFileSystem;

namespace Generator.Database.FileSystem
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;
            if (disposing)
            {
                // dispose managed state (managed objects).
                if (_archiveStream != null)
                {
                    _archiveStream.Close();
                    _archiveStream = null;
                }
                _archive?.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override a finalizer below.
            // set large fields to null.

            disposedValue = true;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }

    public class FileZipBundledItem : IDbItem
    {
        public IDbName Name { get; }
        public IDbItemContent Content { get; }

        internal FileZipBundledItem(IZipArchiveDirectory archiveDir, bool cached)
        {
            var inDir = archiveDir;

            Name = new DbItemName
            {
                Name = inDir.Name,
                SafeName = FileEngineDefaults.CreateSafeName(inDir.Name)
            };
            Content = new DirContent(inDir.Entries, cached);
        }

        class DirContent : IDbItemContent
        {
            private readonly IEnumerable<ZipArchiveEntry> _entries;
            private readonly bool _cached;
            private IEnumerable<IDbItem> _itemCache;

            public string Type => FileEngineDefaults.BundleTypeDir;

            public string PersistentLink => string.Empty;

            public DirContent(IEnumerable<ZipArchiveEntry> fileEntries, bool cached)
            {
                _entries = fileEntries;
                _cached = cached;
            }

            public Stream GetContentStream()
            {
                return new MemoryStream();
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
        public IDbName Name { get; }
        public IDbItemContent Content { get; }

        public FileZipItem(ZipArchiveEntry entry)
        {
            var entry1 = entry;

            var entryName = Path.GetFileNameWithoutExtension(entry1.Name);
            Name = new DbItemName
            {
                Name = entryName,
                SafeName = FileEngineDefaults.CreateSafeName(entryName),
            };
            Content = new FileContent(entry1);
        }

        private class FileContent : IDbItemContent
        {
            private static readonly string[] ServeThisFromTemp = { "ppt", "pptx" };

            private readonly ZipArchiveEntry _entry;
            private readonly INameWrapper _nameWrapper;

            public string Type { get; }
            public string PersistentLink => _nameWrapper.GetLink();

            public FileContent(ZipArchiveEntry entry)
            {
                _entry = entry;
                Type = entry?.Name != null ? Path.GetExtension(entry.Name).Substring(1) : string.Empty;  // remove dot

                if (ServeThisFromTemp.Contains(Type.ToLower()))
                    _nameWrapper = new AutoTempFileWrapper(entry);
                else
                    _nameWrapper = new ArchiveEntryNameWrapper(entry);
            }

            public Stream GetContentStream()
            {
                return _entry.Open();
            }

            public IEnumerable<IDbItem> TryAccessSubs()
            {
                return new List<IDbItem>();
            }
        }

        private interface INameWrapper
        {
            string GetLink();
        }

        private class ArchiveEntryNameWrapper : INameWrapper
        {
            private readonly ZipArchiveEntry _entry;

            public ArchiveEntryNameWrapper(ZipArchiveEntry entry)
            {
                _entry = entry;
            }

            public string GetLink()
            {
                return _entry.FullName;
            }
        }

        private class AutoTempFileWrapper : INameWrapper
        {
            private readonly ZipArchiveEntry _entry;
            private AutoTempFile _autoTempFile;

            public AutoTempFileWrapper(ZipArchiveEntry entry)
            {
                _entry = entry;
            }

            public string GetLink()
            {
                if (_autoTempFile == null)
                    using (var stream = _entry.Open())
                        _autoTempFile = new AutoTempFile(stream);

                return _autoTempFile.GetLink();
            }
        }
    }
}
