using System.Collections.Generic;
using System.IO.Compression;

namespace Generator.Database.FileSystem
{
    internal interface IZipArchiveDirectory {
        /// <summary>
        /// Name of this dir
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Name with relative path included
        /// </summary>
        string FullName { get; }
        /// <summary>
        /// Zip entries
        /// </summary>
        IEnumerable<ZipArchiveEntry> Entries { get; }
        /// <summary>
        /// Other dirs
        /// </summary>
        IEnumerable<IZipArchiveDirectory> Directories { get; }
    }
}