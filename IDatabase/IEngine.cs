using System;
using System.Collections.Generic;

namespace IDatabase
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
    }
}
