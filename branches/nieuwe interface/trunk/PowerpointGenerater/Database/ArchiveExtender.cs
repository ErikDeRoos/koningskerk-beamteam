using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace PowerpointGenerater.Database
{
    static class ArchiveExtender
    {
        private static readonly string pathBuilderSeperator = "" + Path.DirectorySeparatorChar;
        private static readonly char[] pathSeparators = new char[] { Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar };

        /// <summary>
        /// Maak de archive lijst (die altijd plat is) tot een boom.
        /// </summary>
        public static IZipArchiveDirectory AsDirectoryStructure(this System.Collections.ObjectModel.ReadOnlyCollection<ZipArchiveEntry> entries)
        {
            return Process(entries.Select(e => new ArchivePathHelper() { Entry = e, Path = e.FullName.Split(pathSeparators) }), new string[0]);
        }
        private static DirStructResult Process(IEnumerable<ArchivePathHelper> onEntries, string[] thisPath)
        {
            var thisPathFileEntries = onEntries.Where(e => e.Path.Length - 1 == thisPath.Length && !string.IsNullOrEmpty(e.Path[e.Path.Length - 1]));
            var notThisPathFileEntries = onEntries.Where(e => e.Path.Length - 1 > thisPath.Length);
            var thisPathDirEntries = notThisPathFileEntries.Select(e => e.Path[thisPath.Length]).Distinct().Where(p => !string.IsNullOrEmpty(p));
            return new DirStructResult()
            {
                Name = ClosestPathName(thisPath),
                FullName = FullPathName(thisPath),
                Entries = thisPathFileEntries.Select(e => e.Entry).ToList(),
                Directories = thisPathDirEntries.Select(p => Process(notThisPathFileEntries.Where(e => e.Path[thisPath.Length] == p).ToList(), Combine(thisPath, p))).ToList(),
            };
        }

        private static IEnumerable<string> GetNextLevelDirs(this IEnumerable<string> fullPaths, string currentPath)
        {
            return fullPaths.
                Where(p => p.StartsWith(currentPath))
                .Select(p => p.Substring(0, p.IndexOfAny(pathSeparators, currentPath.Length)))
                .Distinct();
        }
        private static string ClosestPathName(string[] fromPath)
        {
            return fromPath.LastOrDefault();
        }
        private static string FullPathName(string[] fromPath)
        {
            return string.Join(pathBuilderSeperator, fromPath).TrimEnd(pathSeparators);
        }
        private static string[] Combine(string[] entry, string add)
        {
            var tempList = entry.ToList();
            tempList.Add(add);
            return tempList.ToArray();
        }

        private class DirStructResult : IZipArchiveDirectory
        {
            public string Name { get; set; }
            public string FullName { get; set; }
            public IEnumerable<ZipArchiveEntry> Entries { get; set; }
            public IEnumerable<IZipArchiveDirectory> Directories { get; set; }
        }

        private class ArchivePathHelper
        {
            public ZipArchiveEntry Entry { get; set; }
            public string[] Path { get; set; }
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
