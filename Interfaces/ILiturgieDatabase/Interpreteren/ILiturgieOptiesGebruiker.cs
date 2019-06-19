// Copyright 2017 door Erik de Roos
namespace ILiturgieDatabase
{

    public interface ILiturgieOptiesGebruiker
    {
        bool NietVerwerkenViaDatabase { get; }
        bool? ToonInOverzicht { get; }
        bool? ToonInVolgende { get; }
        bool AlsBijbeltekst { get; }
        string AlternatieveNaamOverzicht { get; }
        string AlternatieveNaam { get; }
    }
}
