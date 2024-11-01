﻿// Copyright 2024 door Erik de Roos
using System.Collections.Generic;
using System.IO;

namespace Generator.Tools
{
    public class LocalFileOperations : IFileOperations
    {
        public string CombineDirectories(string atPath, string otherPath)
        {
            return Path.Combine(atPath, otherPath);
        }

        public bool DirExists(string atPath)
        {
            return Directory.Exists(atPath);
        }

        public bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        public Stream FileReadStream(string filename)
        {
            return new FileStream(filename, FileMode.Open, FileAccess.Read);
        }

        public Stream FileWriteStream(string filename)
        {
            return new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
        }

        public IEnumerable<string> GetDirectories(string atPath)
        {
            return Directory.GetDirectories(atPath);
        }

        public IEnumerable<string> GetFiles(string atPath)
        {
            return Directory.GetFiles(atPath);
        }

        public string GetTempFileName()
        {
            return Path.GetTempFileName();
        }

        public void Delete(string fileName)
        {
            File.Delete(fileName);
        }
    }
}
