
namespace ILiturgieDatabase
{
    public interface ILiturgieDisplay
    {
        /// <summary>
        /// Naam zoals deze getoond moet worden boven dia en als volgende (zonder sub naam, zonder verzen)
        /// </summary>
        /// <example>'psalm' in 'psalm 100: 1, 2'</example>
        string Naam { get; }
        /// <summary>
        /// Naam zoals deze getoond moet worden in liturgie (zonder sub naam, zonder verzen)
        /// </summary>
        /// <example>'psalm' in 'psalm 100: 1, 2'</example>
        string NaamOverzicht { get; }
        /// <summary>
        /// De sub naam 
        /// </summary>
        /// <example>'100' in 'psalm 100: 1, 2'</example>
        string SubNaam { get; }
        /// <summary>
        /// Als de versbeschrijving afgeleid moet worden van de liturgie content
        /// </summary>
        bool VersenAfleiden { get; }
        /// <summary>
        /// Basis verzen, als afleiden niet lukt / kan
        /// </summary>
        string VersenDefault { get; }
    }
}
