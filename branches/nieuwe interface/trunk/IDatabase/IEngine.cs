using System;
using System.Collections.Generic;
using System.IO;

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

    /// <summary>
    /// Per set kunnen de items gehaald worden.
    /// Elke set heeft zijn individuele instellingen 
    /// </summary>
    public interface IDbSet<T> where T : class, ISetSettings, new()
    {
        /// <summary>
        /// Zoek in een set
        /// </summary>
        IEnumerable<IDbItem> Where(Func<IDbItem, bool> query);
        /// <summary>
        /// Naam van de dataset zoals deze voorkomt op de server (meestal directory naam, case sensitive)
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Toegang tot de set-specifieke settings (lezen/schrijven)
        /// </summary>
        T Settings { get; set; }
    }

    public interface IDbItem
    {
        /// <summary>
        /// Naam (lowercase) van het item binnen de set. Bij subs is dit de naam (lowercase) 
        /// van de afzonderlijke items.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Content
        /// </summary>
        IDbItemContent Content { get; }
    }

    public interface IDbItemContent
    {
        /// <summary>
        /// Bestand type (lowercase)
        /// </summary>
        string Type { get; }
        /// <summary>
        /// Stream van content. Indien null kan het zijn dat je subcontent hebt.
        /// </summary>
        Stream Content { get; }
        /// <summary>
        /// Link naar waar de inhoud staat.
        /// </summary>
        string PersistentLink { get; }
        /// <summary>
        /// Als de instellingen zeggen dat er subcontent mogelijk is, kan je hier bij de subs.
        /// </summary>
        IEnumerable<IDbItem> TryAccessSubs();
    }

    public interface ISetSettings
    {
        /// <summary>
        /// Als de inhoud samengevoegd is in een container (meestal zip)
        /// </summary>
        bool UseContainer { get; }
        /// <summary>
        /// De naam van de set zoals deze weergegeven moet worden
        /// </summary>
        string DisplayName { get; }
        /// <summary>
        /// Als de items niet de content bevatten maar eerst subitems waaronder pas de content zit
        /// </summary>
        bool ItemsHaveSubContent { get; }
    }
}
