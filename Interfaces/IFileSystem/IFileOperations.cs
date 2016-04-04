// Copyright 2016 door Erik de Roos
using System.Collections.Generic;
using System.IO;

namespace IFileSystem
{
    public interface IFileOperations
    {
        IEnumerable<string> GetDirectories(string atPath);
        bool DirExists(string atPath);
        IEnumerable<string> GetFiles(string atPath);
        bool FileExists(string fileName);
        string CombineDirectories(string atPath, string otherPath);
        Stream FileReadStream(string filename);
        Stream FileWriteStream(string filename);
        string GetTempFileName();
        void Delete(string fileName);
    }
}
