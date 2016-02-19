
namespace ILiturgieDatabase
{
    public interface ILiturgieContent
    {
        /// <summary>
        /// Nummering. Bij geen nummer worden er geen nummers getoond
        /// </summary>
        int? Nummer { get; }
        /// <summary>
        /// Type inhoud.
        /// </summary>
        InhoudType InhoudType { get; }
        /// <summary>
        /// De inhoud. De actuele tekst bij tekst, een link bij ppt.
        /// </summary>
        string Inhoud { get; }
    }
}
