// Copyright 2024 door Erik de Roos

using System.Xml.Serialization;

namespace Generator.Database.FileSystem
{
    [XmlRoot(ElementName = "FileEngineSetSettings")]
    public class DbSetSettings
    {
        /// <summary>
        /// Als de inhoud samengevoegd is in een container (meestal zip)
        /// </summary>
        [XmlElement(ElementName = "UseContainer")]
        public bool UseContainer { get; set; }
        /// <summary>
        /// De naam van de set zoals deze weergegeven moet worden
        /// Kan ook op gezocht worden indien dit is geactvieerd via instellingen
        /// </summary>
        [XmlElement(ElementName = "DisplayName")]
        public string DisplayName { get; set; }
        /// <summary>
        /// Als de items niet de content bevatten maar eerst subitems waaronder pas de content zit
        /// </summary>
        [XmlElement(ElementName = "ItemsHaveSubContent")]
        public bool ItemsHaveSubContent { get; set; }
        /// <summary>
        /// Als de inhoud van het item bestaat uit subcontent, gescheiden door opvolgende nummering. Werkt alleen voor txt.
        /// </summary>
        [XmlElement(ElementName = "ItemIsSubContent")]
        public bool ItemIsSubContent { get; set; }
        /// <summary>
        /// Deze items standaard niet tonen in de liturgie
        /// </summary>
        [XmlElement(ElementName = "NotVisibleInIndex")]
        public bool NotVisibleInIndex { get; set; }
        /// <summary>
        /// Geavanceerde settings voor complexe items
        /// </summary>
        [XmlElement(ElementName = "AdvancedSettingString")]
        public string AdvancedSettingString { get; set; }
    }
}
