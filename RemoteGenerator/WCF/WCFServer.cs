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

        public Token StartConnectie(Instellingen gebruikInstellingen)
        {
            return Generator.NieuweWachtrijRegel(gebruikInstellingen).Token;
        }

        public void StartGenereren(Token token, Liturgie metLiturgie)
        {
            Generator.UpdateWachtrijRegel(token, metLiturgie);
        }

        public Voortgang CheckVoortgang(Token token)
        {
            var item = Generator.Wachtrij.FirstOrDefault(w => w.Token.ID == token.ID);
            if (item == null)
                item = Generator.Gereed.FirstOrDefault(w => w.Token.ID == token.ID);
            if (item == null)
                return null;
            return item.Voortgang;
        }

        public byte[] DownloadResultaat(Token token)
        {
            var item = Generator.Gereed.FirstOrDefault(w => w.Token.ID == token.ID);
            if (item == null || item.Voortgang.VolledigMislukt)
                return null;
            return System.IO.File.ReadAllBytes(item.ResultaatOpgeslagenOp);
        }
    }
}
