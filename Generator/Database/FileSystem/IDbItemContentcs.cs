// Copyright 2016 door Erik de Roos
using System.Collections.Generic;
using System.IO;

namespace Generator.Database.FileSystem
{
    public interface IDbItemContent
    {
        /// <summary>
        /// Bestand type (lowercase)
        /// </summary>
        string Type { get; }
        /// <summary>
        /// Link naar waar de inhoud staat.
        /// </summary>
        string PersistentLink { get; }
        /// <summary>
        /// Stream van content. Indien null kan het zijn dat je subcontent hebt.
        /// </summary>
        Stream GetContentStream();
        /// <summary>
        /// Als de instellingen zeggen dat er subcontent mogelijk is, kan je hier bij de subs.
        /// </summary>
        IEnumerable<IDbItem> TryAccessSubs();
    }
}
