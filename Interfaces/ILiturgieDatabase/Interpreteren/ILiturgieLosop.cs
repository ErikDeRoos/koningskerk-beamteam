// Copyright 2017 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase {

    public interface ILiturgieLosOp
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        ILiturgieOplossing LosOp(ILiturgieInterpretatie item, ILiturgieSettings settings);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="masks"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        ILiturgieOplossing LosOp(ILiturgieInterpretatie item, IEnumerable<ILiturgieMapmaskArg> masks, ILiturgieSettings settings);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="masks"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        IEnumerable<ILiturgieOplossing> LosOp(IEnumerable<ILiturgieInterpretatie> items, IEnumerable<ILiturgieMapmaskArg> masks, ILiturgieSettings settings);

        /// <summary>
        /// Zoek in alle databases naar de opgegeven tekst
        /// </summary>
        IVrijZoekresultaat VrijZoeken(string zoekTekst, bool alsBijbeltekst = false, IVrijZoekresultaat vorigResultaat = null);

        /// <summary>
        /// Verschaft een basis inzicht van de opties op basis van het zoekresultaat
        /// </summary>
        ILiturgieOptiesGebruiker ZoekStandaardOptiesUitZoekresultaat(string invoerTekst, IVrijZoekresultaat zoekresultaat);

        /// <summary>
        /// Lees alle opties uit
        /// </summary>
        ILiturgieOptiesGebruiker ToonOpties(string invoerTekst);

        /// <summary>
        /// Zet de geinterpreteerde opties weer om naar een tekst
        /// </summary>
        string MaakTotTekst(ILiturgieOptiesGebruiker opties);

        /// <summary>
        /// Zet de geinterpreteerde opties weer om naar een tekst
        /// </summary>
        string MaakTotTekst(string invoerTekst, ILiturgieOptiesGebruiker opties);

        /// <summary>
        /// Splits de tekst op tussen een liturgie stuk en een opties stuk
        /// </summary>
        string[] SplitsVoorOpties(string liturgieRegel);
    }
}
