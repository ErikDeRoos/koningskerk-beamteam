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

        public void Dispose()
        {
            if (FileByteStream != null)
            {
                FileByteStream.Close();
                FileByteStream = null;
            }
        }
    }
}
