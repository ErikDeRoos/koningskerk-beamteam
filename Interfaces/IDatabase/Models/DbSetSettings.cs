// Copyright 2019 door Erik de Roos

namespace IDatabase
{
    public class DbSetSettings
    {
        /// <summary>
        /// Als de inhoud samengevoegd is in een container (meestal zip)
        /// </summary>
        public bool UseContainer { get; set; }
        /// <summary>
        /// De naam van de set zoals deze weergegeven moet worden
        /// Kan ook op gezocht worden indien dit is geactvieerd via instellingen
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Als de items niet de content bevatten maar eerst subitems waaronder pas de content zit
        /// </summary>
        public bool ItemsHaveSubContent { get; set; }
        /// <summary>
        /// Als de inhoud van het item bestaat uit subcontent, gescheiden door opvolgende nummering. Werkt alleen voor txt.
        /// </summary>
        public bool ItemIsSubContent { get; set; }
        /// <summary>
        /// Deze items standaard niet tonen in de liturgie
        /// </summary>
        public bool NotVisibleInIndex { get; set; }
    }
}
