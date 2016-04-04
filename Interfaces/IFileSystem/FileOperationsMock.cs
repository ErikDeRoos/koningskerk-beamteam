// Copyright 2016 door Erik de Roos
using System;
using System.Collections.Generic;
using System.IO;

namespace IFileSystem
{
    public class FileOperationsMock : IFileOperations
    {
        public FileOperationsMock() { }

        public virtual string CombineDirectories(string atPath, string otherPath)
        {
            throw new NotImplementedException();
        }

        public void Delete(string fileName)
        {
            throw new NotImplementedException();
        }

        public virtual bool FileExists(string fileName)
        {
            throw new NotImplementedException();
        }

        public virtual bool DirExists(string atPath)
        {
            throw new NotImplementedException();
        }

        public virtual Stream FileReadStream(string filename)
        {
            throw new NotImplementedException();
        }

        public virtual Stream FileWriteStream(string filename)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<string> GetDirectories(string atPath)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<string> GetFiles(string atPath)
        {
            throw new NotImplementedException();
        }

        public virtual string GetTempFileName()
        {
            throw new NotImplementedException();
        }
    }
}
