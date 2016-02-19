﻿
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
        public byte[] Inhoud { get; set; }
    }
}
