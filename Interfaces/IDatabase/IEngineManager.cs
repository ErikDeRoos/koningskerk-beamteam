﻿// Copyright 2016 door Erik de Roos
using IDatabase.Engine;
using System.Collections.Generic;

namespace IDatabase
{
    /// <summary>
    /// Engine wrapper voor toegang tot andere engines
    /// </summary>
    public interface IEngineManager<T> where T : class, ISetSettings, new()
    {
        /// <summary>
        /// De standaard engine
        /// </summary>
        /// <returns></returns>
        IEngineSelection<T> GetDefault();

        /// <summary>
        /// Alle beschikbare engines
        /// </summary>
        IEnumerable<IEngineSelection<T>> Extensions { get; }
    }
}