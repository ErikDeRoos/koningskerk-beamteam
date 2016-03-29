// Copyright 2016 door Erik de Roos

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
        /// Indien True dan bevat de content alle content die er is. De visualisatie kan dan kiezen om shorthands
        /// te gebruiken.
        /// </summary>
        bool VolledigeContent { get; }
        /// <summary>
        /// Basis verzen, als afleiden niet lukt / kan. Indien niet null dan altijd gebruiken
        /// </summary>
        IVersenDefault VersenGebruikDefault { get; }
    }
}
