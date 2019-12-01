// Copyright 2019 door Erik de Roos
using System.Collections.Generic;

namespace Generator.Database.FileSystem
{
    /// <summary>
    /// Engine wrapper voor toegang tot andere engines.
    /// Aanname is dat alle engines ingeladen zijn bij het aanmaken van de interface.
    /// </summary>
    public interface IEngineManager
    {
        /// <summary>
        /// De standaard engine
        /// </summary>
        /// <returns></returns>
        IEngineSelection GetDefault();

        /// <summary>
        /// Alle beschikbare engines
        /// </summary>
        IEnumerable<IEngineSelection> Extensions { get; }
    }
}
