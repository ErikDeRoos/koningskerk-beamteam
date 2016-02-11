using System.Collections.Generic;

namespace ILiturgieDatabase {
    public interface ILiturgieInterpreteer
    {
        ILiturgieInterpretatie VanTekstregel(string regels);
    }

    public interface ILiturgieLosOp
    {
        ILiturgieOplossing LosOp(ILiturgieInterpretatie item);
        ILiturgieOplossing LosOp(ILiturgieInterpretatie item, IEnumerable<ILiturgieMapmaskArg> masks);
        IEnumerable<ILiturgieOplossing> LosOp(IEnumerable<ILiturgieInterpretatie> items, IEnumerable<ILiturgieMapmaskArg> masks);
    }

    public interface ILiturgieMapmaskArg
    {
        string Name { get; }
        string RealName { get; }
    }


    /// <summary>
    /// Ruwe liturgie regels, zoals ze ingevoerd zijn
    /// </summary>
    public interface ILiturgieInterpretatie
    {
        string Benaming { get; }
        string Deel { get; }
        IEnumerable<string> Verzen { get; }
        string VerzenZoalsIngevoerd { get; }
        IEnumerable<string> Opties { get; }
    }
     
    /// <summary>
    /// Resultaatset van omzetten interpretatie naar resultaat
    /// </summary>
    public interface ILiturgieOplossing
    {
        ILiturgieInterpretatie VanInterpretatie { get; }
        ILiturgieRegel Regel { get; }
        LiturgieOplossingResultaat Resultaat { get; }
    }

    public enum LiturgieOplossingResultaat
    {
        Opgelost = 0,
        SetFout = 1,
        SubSetFout = 2,
        VersFout = 3,  // Vers niet gevonden
        VersOnderverdelingMismatch = 4,  // Als er wel versen gevraagd worden maar de set geen versen ondersteund
        VersOnleesbaar = 5,  // Vers data type probleem
    }

    /// <summary>
    /// Liturgie oplossing
    /// </summary>
    public interface ILiturgieRegel
    {
        /// <summary>
        /// Tekstuele presentatie
        /// </summary>
        ILiturgieDisplay Display { get; }
        /// <summary>
        /// Of deze regel in het liturgie overzicht moet komen
        /// </summary>
        bool TonenInOverzicht { get; }
        /// <summary>
        /// Of de inhoud verwerkt moet worden tot een slide
        /// </summary>
        bool VerwerkenAlsSlide { get; }
        /// <summary>
        /// Of een voorgaande slide een 'volgende' vermelding moet maken naar deze naam van deze slide
        /// </summary>
        bool TonenInVolgende { get; }

        /// <summary>
        /// Inhoud van de regel. 1 of meer. Bij 'VerwerkenAlsSlide' is 'False' dan is er geen content.
        /// </summary>
        IEnumerable<ILiturgieContent> Content { get; }
    }

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

    public enum InhoudType
    {
        Tekst,
        PptLink,
    }
}
