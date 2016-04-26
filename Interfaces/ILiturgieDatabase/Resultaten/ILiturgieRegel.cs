// Copyright 2016 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase {
 
    /// <summary>
    /// Liturgie oplossing
    /// </summary>
    public interface ILiturgieRegel
    {
        /// <summary>
        /// Tekstuele presentatie
        /// </summary>
        ILiturgieDisplay Display { get; }
        /// <summary>
        /// Of deze regel in het liturgie overzicht moet komen
        /// </summary>
        bool TonenInOverzicht { get; }
        /// <summary>
        /// Of de inhoud verwerkt moet worden tot een slide
        /// </summary>
        bool VerwerkenAlsSlide { get; }
        /// <summary>
        /// Of een voorgaande slide een 'volgende' vermelding moet maken naar deze naam van deze slide
        /// </summary>
        bool TonenInVolgende { get; }
        /// <summary>
        /// Of de inhoud van een afwijkend type is
        /// </summary>
        VerwerkingType VerwerkenAlsType { get; }

        /// <summary>
        /// Inhoud van de regel. 1 of meer. Bij 'VerwerkenAlsSlide' is 'False' dan is er geen content.
        /// </summary>
        IEnumerable<ILiturgieContent> Content { get; }
    }
}
