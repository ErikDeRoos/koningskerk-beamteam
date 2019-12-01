// Copyright 2019 door Erik de Roos
using System.Collections.Generic;

namespace Generator.LiturgieInterpretator.Models
{
    /// <summary>
    /// Ruwe liturgie regels, zoals ze ingevoerd zijn
    /// </summary>
    public interface ILiturgieTekstObject
    {
        string Benaming { get; }
        string Deel { get; }
        string TeTonenNaam { get; }
        string TeTonenNaamOpOverzicht { get; }
        IEnumerable<string> Verzen { get; }
        string VerzenZoalsIngevoerd { get; }
        LiturgieOptiesGebruiker OptiesGebruiker { get; }
    }
}
