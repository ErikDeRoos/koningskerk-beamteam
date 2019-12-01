// Copyright 2019 door Erik de Roos
using System;
using System.Collections.Generic;

namespace Generator.Database.FileSystem
{
    /// <summary>
    /// Per set kunnen de items gehaald worden.
    /// Elke set heeft zijn individuele instellingen 
    /// </summary>
    public interface IDbSet
    {
        /// <summary>
        /// Naam van de dataset zoals deze voorkomt op de server (meestal directory naam)
        /// </summary>
        DbItemName Name { get; }

        /// <summary>
        /// Toegang tot de set-specifieke settings (lezen/schrijven)
        /// </summary>
        DbSetSettings Settings { get; set; }

        /// <summary>
        /// Zoek in een set
        /// </summary>
        IEnumerable<IDbItem> Where(Func<IDbItem, bool> query);

        /// <summary>
        /// Krijg alle beschikbare fragment namen
        /// </summary>
        /// <returns></returns>
        IEnumerable<DbItemName> GetAllNames();
    }
}
