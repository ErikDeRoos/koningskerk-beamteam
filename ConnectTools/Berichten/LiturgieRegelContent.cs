// Copyright 2016 door Erik de Roos
using System.IO;

namespace ConnectTools.Berichten
{
    public class LiturgieRegelContent
    {
        /// <summary>
        /// Nummering. Bij geen nummer worden er geen nummers getoond
        /// </summary>
        public int? Nummer { get; set; }
        /// <summary>
        /// Type inhoud.
        /// </summary>
        public InhoudType InhoudType { get; set; }
        /// <summary>
        /// De inhoud. De actuele tekst bij tekst, de ppt bij een ppt.
        /// </summary>
        public string InhoudTekst { get; set; }
        /// <summary>
        /// De inhoud. De actuele tekst bij tekst, de ppt bij een ppt.
        /// </summary>
        public StreamToken InhoudBestand { get; set; }
    }
}
