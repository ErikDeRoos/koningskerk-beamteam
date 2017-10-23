// Copyright 2017 door Erik de Roos
using System;
using System.IO;
using System.ServiceModel;

namespace ConnectTools.Berichten
{
    [MessageContract]
    public class SendFile : IDisposable
    {
        [MessageHeader(MustUnderstand = true)]
        public Token Token;

        [MessageHeader(MustUnderstand = true)]
        public StreamToken FileToken;

        [MessageBodyMember]
        public Stream FileByteStream;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    if (FileByteStream != null)
                    {
                        FileByteStream.Close();
                        FileByteStream = null;
                    }
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
