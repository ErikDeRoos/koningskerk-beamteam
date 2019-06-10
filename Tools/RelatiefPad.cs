using System;
using System.IO;
using System.Linq;

namespace Tools
{
    public static class RelatiefPad
    {
        public static string ReplaceWithRelativePath(string applicationPath, string pathToMakeRelative)
        {
            var selectedPath = pathToMakeRelative;
            // Simple relative
            if (selectedPath.StartsWith(applicationPath))
                selectedPath = selectedPath.Replace(applicationPath, ".");
            // Complex relative
            else
            {
                // Find common path
                var minIndex = Math.Min(applicationPath.Length, selectedPath.Length);
                string equalStringPart = null;
                for (int index = 0; index < minIndex; index++)
                {
                    if (selectedPath[index] == applicationPath[index])
                        continue;
                    if (index > 0)
                        equalStringPart = selectedPath.Substring(0, index);
                    break;
                }

                // Make sure last path isn't a path with a shared starting name
                if (!equalStringPart.EndsWith($"{Path.DirectorySeparatorChar}"))
                    equalStringPart = equalStringPart.Substring(0, equalStringPart.LastIndexOf(Path.DirectorySeparatorChar) + 1);

                // Use difference in application path to calculate relative path
                var diff = applicationPath.Substring(equalStringPart.Length);
                var amountOfRelative = diff.Count(c => c == Path.DirectorySeparatorChar);

                selectedPath = "." + new String(Enumerable.Range(0, amountOfRelative).SelectMany(c => "..").ToArray()) + Path.DirectorySeparatorChar + pathToMakeRelative.Substring(equalStringPart.Length);
            }

            return selectedPath;
        }

        public static string ReplaceWithNormalPath(string applicationPath, string relativePath)
        {
            if (!relativePath.StartsWith("."))
                return relativePath;

            return applicationPath + relativePath.Substring(1);
        }
    }
}
