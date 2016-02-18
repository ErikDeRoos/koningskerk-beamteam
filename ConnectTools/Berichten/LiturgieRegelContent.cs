
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
        /// De inhoud. De actuele tekst bij tekst, een link bij ppt.
        /// </summary>
        public string Inhoud { get; set; }
    }
}
