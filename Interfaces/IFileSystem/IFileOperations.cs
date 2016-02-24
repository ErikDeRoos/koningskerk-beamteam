using System.Collections.Generic;
using System.IO;

namespace IFileSystem
{
    public interface IFileOperations
    {
        IEnumerable<string> GetDirectories(string atPath);
        IEnumerable<string> GetFiles(string atPath);
        bool FileExists(string fileName);
        string CombineDirectories(string atPath, string otherPath);
        Stream FileReadStream(string filename);
        Stream FileWriteStream(string filename);
    }
}
