// Copyright 2019 door Erik de Roos
using System;
using System.Collections.Generic;

namespace Generator.Database.FileSystem
{
    /// <summary>
    /// Engine
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// Krijg de beschikbare sets
        /// </summary>
        IEnumerable<IDbSet> Where(Func<IDbSet, bool> query);

        /// <summary>
        /// Krijg alle beschikbare onderdeel namen
        /// </summary>
        /// <returns></returns>
        IEnumerable<DbItemName> GetAllNames();
    }
}
