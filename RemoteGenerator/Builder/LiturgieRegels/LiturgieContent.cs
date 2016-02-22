﻿using ILiturgieDatabase;
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
                    File.WriteAllBytes(Inhoud, vanContent.Inhoud);
                    break;
                case ConnectTools.Berichten.InhoudType.Tekst:
                    InhoudType = InhoudType.Tekst;
                    Inhoud = Encoding.Unicode.GetString(vanContent.Inhoud);
                    break;
                default:
                    throw new NotImplementedException();
            }
            Nummer = vanContent.Nummer;
        }
    }
}