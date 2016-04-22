// Copyright 2016 door Erik de Roos
using ConnectTools.Berichten;
using RemoteGenerator.Builder;
using System.Linq;
using ConnectTools;
using System.IO;
using Autofac;

namespace RemoteGenerator.WCF
{
    public class WCFServer : IWCFServer
    {
        internal IPpGenerator Generator { get; set; }

        public WCFServer()
        {
            Host.StaticIoCContainer.InjectProperties(this);  // Dirty, maar WCF ondersteunt geen DI. We gebruiken dus een servicelocator
        }

        public Token StartConnectie(BuilderData gebruikBuilderData, Liturgie metLiturgie)
        {
            return Generator.NieuweWachtrijRegel(gebruikBuilderData, metLiturgie).Token;
        }

        public void ToevoegenBestand(SendFile file)
        {
            Generator.UpdateWachtrijRegel(file.Token, file.FileToken.ID, file.FileByteStream);
        }

        public Voortgang StartGenereren(Token token)
        {
            return Generator.ProbeerTeStarten(token);
        }

        public Voortgang CheckVoortgang(Token token)
        {
            var item = Generator.Wachtrij.FirstOrDefault(w => w.Token.ID == token.ID);
            if (item == null)
                item = Generator.Verwerkt.FirstOrDefault(w => w.Token.ID == token.ID);
            if (item == null)
                return null;
            return item.Voortgang;
        }

        public Stream DownloadResultaat(Token token)
        {
            return Generator.KrijgGegenereerdBestand(token);
        }
    }
}
