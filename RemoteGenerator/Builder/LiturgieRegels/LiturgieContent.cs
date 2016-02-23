using ILiturgieDatabase;
using System;
using System.IO;
using System.Text;

namespace RemoteGenerator.Builder.LiturgieRegels
{
    class LiturgieContent : ILiturgieContent
    {
        public string Inhoud { get; set; }

        public InhoudType InhoudType { get; set; }

        public int? Nummer { get; set; }

        public LiturgieContent(ConnectTools.Berichten.LiturgieRegelContent vanContent)
        {
            switch (vanContent.InhoudType)
            {
                case ConnectTools.Berichten.InhoudType.PptLink:
                    InhoudType = InhoudType.PptLink;
                    Inhoud = Path.GetTempFileName();
                    var copyTo = new FileStream(Inhoud, FileMode.Create);
                    vanContent.InhoudBestand.CopyTo(copyTo);
                    copyTo.Close();
                    break;
                case ConnectTools.Berichten.InhoudType.Tekst:
                    InhoudType = InhoudType.Tekst;
                    Inhoud = vanContent.InhoudTekst;
                    break;
                default:
                    throw new NotImplementedException();
            }
            Nummer = vanContent.Nummer;
        }
    }
}
