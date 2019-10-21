// Copyright 2019 door Erik de Roos
using System;
using System.Collections.Generic;

namespace IDatabase
{
    /// <summary>
    /// Per set kunnen de items gehaald worden.
    /// Elke set heeft zijn individuele instellingen 
    /// </summary>
    public interface IDbSet<T> where T : class, ISetSettings, new()
    {
        /// <summary>
        /// Naam van de dataset zoals deze voorkomt op de server (meestal directory naam, case sensitive)
        /// </summary>
        IDbName Name { get; }

        /// <summary>
        /// Toegang tot de set-specifieke settings (lezen/schrijven)
        /// </summary>
        T Settings { get; set; }

        /// <summary>
        /// Zoek in een set
        /// </summary>
        IEnumerable<IDbItem> Where(Func<IDbItem, bool> query);

        /// <summary>
        /// Krijg alle beschikbare fragment namen
        /// </summary>
        /// <returns></returns>
        IEnumerable<IDbName> GetAllNames();
    }
}
