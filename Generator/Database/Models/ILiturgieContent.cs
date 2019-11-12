// Copyright 2016 door Erik de Roos

namespace Generator.Database.Models
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
