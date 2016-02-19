using System;
using ConnectTools.Berichten;
using RemoteGenerator.Builder;
using Microsoft.Practices.Unity;
using System.Linq;

namespace RemoteGenerator.WCF
{
    public class WCFServer : IWCFServer
    {
        [Dependency]
        internal IPpGenerator Generator { get; set; }

        public Token StartConnectie(Liturgie metLiturgie)
        {
            return Generator.NieuweWachtrijRegel(metLiturgie).Token;
        }

        public Voortgang CheckVoortgang(Token token)
        {
            var item = Generator.Wachtrij.FirstOrDefault(w => w.Token.ID == token.ID);
            if (item == null)
                return null;
            return item.Voortgang;
        }

        public byte[] DownloadResultaat(Token token)
        {
            var item = Generator.Wachtrij.FirstOrDefault(w => w.Token.ID == token.ID);
            if (item == null || !item.Voortgang.Gereed)
                return null;
            return item.Resultaat;
        }
    }
}
