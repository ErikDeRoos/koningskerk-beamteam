using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerpointGenerater.Database
{
    static class ArchiveExtender
    {
        public static IZipArchiveDirectory AsDirectoryStructure(this System.Collections.ObjectModel.ReadOnlyCollection<ZipArchiveEntry> entries)
        {
            throw new NotSupportedException();
            //var dirWorthy = entries.Where(e => e.FullName != e.Name).ToList();
        }
    }

    interface IZipArchiveDirectory {
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
