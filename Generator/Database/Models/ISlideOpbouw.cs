// Copyright 2019 door Erik de Roos
using System.Collections.Generic;

namespace Generator.Database.Models
{
 
    /// <summary>
    /// Instructies hoe de slide opgebouwd moet worden
    /// </summary>
    public interface ISlideOpbouw
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
        /// Of een voorgaande slide naar de eerstvolgende moet kijken als hij wil verwijzen naar 'volgende'
        /// </summary>
        bool OverslaanInVolgende { get; }
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
