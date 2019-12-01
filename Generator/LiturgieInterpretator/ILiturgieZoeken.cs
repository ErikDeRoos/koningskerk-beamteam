// Copyright 2019 door Erik de Roos
using Generator.LiturgieInterpretator.Models;

namespace Generator.LiturgieInterpretator
{

    public interface ILiturgieZoeken
    {
        /// <summary>
        /// Zoek in alle databases naar de opgegeven tekst
        /// </summary>
        IVrijZoekresultaat VrijZoeken(string zoekTekst, bool alsBijbeltekst = false, IVrijZoekresultaat vorigResultaat = null);

        /// <summary>
        /// Verschaft een basis inzicht van de opties op basis van het zoekresultaat
        /// </summary>
        LiturgieOptiesGebruiker ZoekStandaardOptiesUitZoekresultaat(string invoerTekst, IVrijZoekresultaat zoekresultaat);

        /// <summary>
        /// Lees alle opties uit
        /// </summary>
        LiturgieOptiesGebruiker ToonOpties(string invoerTekst);

        /// <summary>
        /// Zet de geinterpreteerde opties weer om naar een tekst
        /// </summary>
        string MaakTotTekst(string invoerTekst, LiturgieOptiesGebruiker opties, IVrijZoekresultaat zoekresultaat);

        /// <summary>
        /// Splits de tekst op tussen een liturgie stuk en een opties stuk
        /// </summary>
        string[] SplitsVoorOpties(string liturgieRegel);
    }
}
