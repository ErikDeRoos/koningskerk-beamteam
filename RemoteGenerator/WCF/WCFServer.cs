using System;
using ConnectTools.Berichten;

namespace RemoteGenerator.WCF
{
    public class WCFServer : IWCFServer
    {
        public Token StartConnectie(Liturgie metLiturgie)
        {
            throw new NotImplementedException();
        }

        public Voortgang CheckVoortgang(Token token)
        {
            throw new NotImplementedException();
        }

        public byte[] DownloadResultaat(Token token)
        {
            throw new NotImplementedException();
        }
    }
}
