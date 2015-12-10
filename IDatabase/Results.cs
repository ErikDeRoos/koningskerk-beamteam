using System.Collections.Generic;

namespace IDatabase {

    /// <summary>
    /// Ruwe liturgie regels, zoals ze ingevoerd zijn
    /// </summary>
    public interface ILiturgieOnderdeelRuw
    {
        string Benaming { get; }
        string Deel { get; }
        IEnumerable<string> Verzen { get; }
    }

    /// <summary>
    /// Ruwe liturgie regels, aangevuld met zoekactie hints (filesystem paden)
    /// </summary>
    public interface ILiturgieOnderdeelZoekactie
    {
        ILiturgieOnderdeelRuw Ruw { get; }
        string VirtueleBenaming { get; }
        string EchteBenaming { get; }
        LiturgieType Type { get; }
        IEnumerable<ILiturgieOnderdeelZoekactieHint> ZoekactieHints { get; }
    }
    public interface ILiturgieOnderdeelZoekactieHint
    {
        string Nummer { get; }
        string ZoekPad { get; }
    }
    public enum LiturgieType
    {
        /// <summary>
        /// Enkelvoudige aanduiding, bijvoorbeeld een openingsslide.
        /// </summary>
        EnkelZonderDeel,
        /// <summary>
        /// Enkelvoudige aanduiding met deel benaming, bijvoorbeeld een database 
        /// zoals opwekking waar wel nummers zijn maar geen verzen
        /// </summary>
        EnkelMetDeel,
        /// <summary>
        /// Meervoudige aanduiding met deel benaming, bijvoorbeeld een database
        /// zoals psalmen waar nummers zijn met individuele verzen
        /// </summary>
        MeerMetDeel,
    }

    /// <summary>
    /// Zoekresultaat item(s) samen met de zoekopdracht gegevens
    /// </summary>
    public interface ILiturgieZoekresultaat
    {
        LiturgieType Type { get; }
        string VirtueleBenaming { get; }
        string EchteBenaming { get; }
        string DeelBenaming { get; }
        IEnumerable<ILiturgieZoekresultaatDeel> Resultaten { get; }
    }
    /// <summary>
    /// Het gezochte item en of/wat er gevonden is
    /// </summary>
    public interface ILiturgieZoekresultaatDeel
    {
        string Nummer { get; }
        string Zoekopdracht { get; }
        bool Gevonden { get; }
        string Inhoud { get; }
        InhoudType InhoudType { get; }
    }
    public enum InhoudType
    {
        Tekst,
        PptLink,
    }
}
