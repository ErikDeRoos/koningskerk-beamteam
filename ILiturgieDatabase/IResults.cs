using System.Collections.Generic;

namespace ILiturgieDatabase {
    public interface ILiturgieInterpreteer
    {
        ILiturgieInterpretatie VanTekstregel(string regels);
    }

    public interface ILiturgieLosOp
    {
        ILiturgieOplossing LosOp(ILiturgieInterpretatie item);
        IEnumerable<ILiturgieOplossing> LosOp(IEnumerable<ILiturgieInterpretatie> items);
    }



    /// <summary>
    /// Ruwe liturgie regels, zoals ze ingevoerd zijn
    /// </summary>
    public interface ILiturgieInterpretatie
    {
        string Benaming { get; }
        string Deel { get; }
        IEnumerable<string> Verzen { get; }
        IEnumerable<string> Opties { get; }
    }


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


    public interface ILiturgieRegel
    {
        /// <summary>
        /// Naam zoals deze getoond moet worden boven dia en als volgende (zonder sub naam, zonder verzen)
        /// </summary>
        /// <example>'psalm' in 'psalm 100: 1, 2'</example>
        string NaamDisplay { get; }
        /// <summary>
        /// Naam zoals deze getoond moet worden in liturgie (zonder sub naam, zonder verzen)
        /// </summary>
        /// <example>'psalm' in 'psalm 100: 1, 2'</example>
        string OverzichtDisplay { get; }
        /// <summary>
        /// De sub naam 
        /// </summary>
        /// <example>'100' in 'psalm 100: 1, 2'</example>
        string SubNaamDisplay { get; }
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


    ///// <summary>
    ///// Ruwe liturgie regels, aangevuld met zoekactie hints
    ///// </summary>
    //public interface ILiturgieOnderdeelZoekactie
    //{
    //    ILiturgieInterpretatie Ruw { get; }
    //    string VirtueleBenaming { get; }
    //    string EchteBenaming { get; }
    //    LiturgieType Type { get; }
    //    IEnumerable<ILiturgieOnderdeelZoekactieHint> ZoekactieHints { get; }
    //}
    //public interface ILiturgieOnderdeelZoekactieHint
    //{
    //    string Nummer { get; }
    //    string ZoekPad { get; }
    //}
    //public enum LiturgieType
    //{
    //    /// <summary>
    //    /// Enkelvoudige aanduiding, bijvoorbeeld een openingsslide.
    //    /// </summary>
    //    EnkelZonderDeel,
    //    /// <summary>
    //    /// Enkelvoudige aanduiding met deel benaming, bijvoorbeeld een database 
    //    /// zoals opwekking waar wel nummers zijn maar geen verzen
    //    /// </summary>
    //    EnkelMetDeel,
    //    /// <summary>
    //    /// Meervoudige aanduiding met deel benaming, bijvoorbeeld een database
    //    /// zoals psalmen waar nummers zijn met individuele verzen
    //    /// </summary>
    //    MeerMetDeel,
    //}

    ///// <summary>
    ///// Zoekresultaat item(s) samen met de zoekopdracht gegevens
    ///// </summary>
    //public interface ILiturgieZoekresultaat
    //{
    //    LiturgieType Type { get; }
    //    string VirtueleBenaming { get; }
    //    string EchteBenaming { get; }
    //    string DeelBenaming { get; }
    //    IEnumerable<ILiturgieZoekresultaatDeel> Resultaten { get; }
    //}
    ///// <summary>
    ///// Het gezochte item en of/wat er gevonden is
    ///// </summary>
    //public interface ILiturgieZoekresultaatDeel
    //{
    //    string Nummer { get; }
    //    string Zoekopdracht { get; }
    //    bool Gevonden { get; }
    //    string Inhoud { get; }
    //    InhoudType InhoudType { get; }
    //}
}
