// Copyright 2017 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    /// <summary>
    /// Ruwe liturgie regels, zoals ze ingevoerd zijn
    /// </summary>
    public interface ILiturgieInterpretatie
    {
        string Benaming { get; }
        string Deel { get; }
        IEnumerable<string> Verzen { get; }
        string VerzenZoalsIngevoerd { get; }
        IEnumerable<string> Opties { get; }
    }
}
