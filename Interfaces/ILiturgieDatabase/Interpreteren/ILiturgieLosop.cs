// Copyright 2017 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase {

    public interface ILiturgieLosOp
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        ILiturgieOplossing LosOp(ILiturgieInterpretatie item);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="masks"></param>
        /// <returns></returns>
        ILiturgieOplossing LosOp(ILiturgieInterpretatie item, IEnumerable<ILiturgieMapmaskArg> masks);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="masks"></param>
        /// <returns></returns>
        IEnumerable<ILiturgieOplossing> LosOp(IEnumerable<ILiturgieInterpretatie> items, IEnumerable<ILiturgieMapmaskArg> masks);

        /// <summary>
        /// Zoek in alle databases naar de opgegeven tekst
        /// </summary>
        IVrijZoekresultaat VrijZoeken(string zoekTekst, IVrijZoekresultaat vorigResultaat = null);
    }
}
