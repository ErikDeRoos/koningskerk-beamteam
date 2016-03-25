using System.Collections.Generic;

namespace ILiturgieDatabase
{
    /// <summary>
    /// Ruwe liturgie regels voor bijbelteksten, zoals ze ingevoerd zijn
    /// </summary>
    public interface ILiturgieInterpretatieBijbeltekst : ILiturgieInterpretatie
    {
        IEnumerable<ILiturgieInterpretatieBijbeltekstDeel> PerDeelVersen { get; }
    }
    public interface ILiturgieInterpretatieBijbeltekstDeel
    {
        string Deel { get; }
        string Versen { get; }
    }
}
