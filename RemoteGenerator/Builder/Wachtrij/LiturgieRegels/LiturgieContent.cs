using ILiturgieDatabase;
using System;

namespace RemoteGenerator.Builder.Wachtrij.LiturgieRegels
{
    class LiturgieContent : ILiturgieContent
    {
        public string Inhoud { get { return InhoudType == ILiturgieDatabase.InhoudType.Tekst ? Tekst : StreamToken.LinkOpFilesysteem; } }

        public string Tekst { get; set; }
        public BestandStreamToken StreamToken { get; set; }

        public InhoudType InhoudType { get; set; }

        public int? Nummer { get; set; }

        public LiturgieContent(ConnectTools.Berichten.LiturgieRegelContent vanContent, Func<ConnectTools.Berichten.StreamToken, BestandStreamToken> bestandStreamTokenFactory)
        {
            switch (vanContent.InhoudType)
            {
                case ConnectTools.Berichten.InhoudType.PptLink:
                    InhoudType = InhoudType.PptLink;
                    StreamToken = bestandStreamTokenFactory(vanContent.InhoudBestand);
                    break;
                case ConnectTools.Berichten.InhoudType.Tekst:
                    InhoudType = InhoudType.Tekst;
                    Tekst = vanContent.InhoudTekst;
                    break;
                default:
                    throw new NotImplementedException();
            }
            Nummer = vanContent.Nummer;
        }
    }
}
