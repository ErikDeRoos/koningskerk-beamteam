// Copyright 2016 door Erik de Roos

namespace IDatabase
{
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
        /// <summary>
        /// Als de inhoud van het item bestaat uit subcontent, gescheiden door opvolgende nummering. Werkt alleen voor txt.
        /// </summary>
        bool ItemIsSubContent { get; }
    }
}
