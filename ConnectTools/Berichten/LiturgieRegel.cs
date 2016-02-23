using System.Collections.Generic;

namespace ConnectTools.Berichten
{
    public class LiturgieRegel
    {
        public int Index { get; set; }
        /// <summary>
        /// Tekstuele presentatie
        /// </summary>
        public LiturgieRegelDisplay Display { get; set; }
        /// <summary>
        /// Of deze regel in het liturgie overzicht moet komen
        /// </summary>
        public bool TonenInOverzicht { get; set; }
        /// <summary>
        /// Of de inhoud verwerkt moet worden tot een slide
        /// </summary>
        public bool VerwerkenAlsSlide { get; set; }
        /// <summary>
        /// Of een voorgaande slide een 'volgende' vermelding moet maken naar deze naam van deze slide
        /// </summary>
        public bool TonenInVolgende { get; set; }

        /// <summary>
        /// Inhoud van de regel. 1 of meer. Bij 'VerwerkenAlsSlide' is 'False' dan is er geen content.
        /// </summary>
        public IEnumerable<LiturgieRegelContent> Content { get; set; }
    }
}
