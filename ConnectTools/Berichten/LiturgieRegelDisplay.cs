
namespace ConnectTools.Berichten
{
    public class LiturgieRegelDisplay
    {
        /// <summary>
        /// Naam zoals deze getoond moet worden boven dia en als volgende (zonder sub naam, zonder verzen)
        /// </summary>
        /// <example>'psalm' in 'psalm 100: 1, 2'</example>
        public string Naam { get; set; }
        /// <summary>
        /// Naam zoals deze getoond moet worden in liturgie (zonder sub naam, zonder verzen)
        /// </summary>
        /// <example>'psalm' in 'psalm 100: 1, 2'</example>
        public string NaamOverzicht { get; set; }
        /// <summary>
        /// De sub naam 
        /// </summary>
        /// <example>'100' in 'psalm 100: 1, 2'</example>
        public string SubNaam { get; set; }
        /// <summary>
        /// Als de versbeschrijving afgeleid moet worden van de liturgie content
        /// </summary>
        public bool VersenAfleiden { get; set; }
        /// <summary>
        /// Basis verzen, als afleiden niet lukt / kan
        /// </summary>
        public string VersenDefault { get; set; }
    }
}
