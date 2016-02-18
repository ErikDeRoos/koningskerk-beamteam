
namespace ILiturgieDatabase
{
    /// <summary>
    /// Resultaatset van omzetten interpretatie naar resultaat
    /// </summary>
    public interface ILiturgieOplossing
    {
        ILiturgieInterpretatie VanInterpretatie { get; }
        ILiturgieRegel Regel { get; }
        LiturgieOplossingResultaat Resultaat { get; }
    }

}
