// Copyright 2017 door Erik de Roos
using System;
using System.Collections.Generic;

namespace IDatabase.Engine
{
    /// <summary>
    /// Engine
    /// </summary>
    public interface IEngine<T> where T : class, ISetSettings, new()
    {
        /// <summary>
        /// Krijg de beschikbare sets
        /// </summary>
        IEnumerable<IDbSet<T>> Where(Func<IDbSet<T>, bool> query);

        /// <summary>
        /// Krijg alle beschikbare onderdeel namen
        /// </summary>
        /// <returns></returns>
        IEnumerable<IDbName> GetAllNames();
    }
}
