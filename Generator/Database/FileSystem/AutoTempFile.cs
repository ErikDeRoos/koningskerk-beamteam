// Copyright 2018 door Erik de Roos
using System;
using System.IO;

namespace Generator.Database.FileSystem
{
    public class AutoTempFile : IDisposable
    {
        private readonly string _tempFile;
        private bool _disposed = false;

        public AutoTempFile(Stream fromStream)
        {
            _tempFile = System.IO.Path.GetTempFileName();
            using (var file = new FileStream(_tempFile, FileMode.Append))
            {
                fromStream.CopyTo(file);
            }
        }

        public string GetLink()
        {
            if (_disposed)
                throw new ObjectDisposedException("TempFile");
            return _tempFile;
        }

        private void ReleaseUnmanagedResources()
        {
            try
            {
                File.Delete(_tempFile);
            }
            catch
            {
                // ignored, delete can go wrong when still open in PowerPoint
            }
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        ~AutoTempFile()
        {
            ReleaseUnmanagedResources();
        }
    }
}
