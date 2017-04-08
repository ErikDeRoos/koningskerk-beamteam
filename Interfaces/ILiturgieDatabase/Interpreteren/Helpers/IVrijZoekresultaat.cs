// Copyright 2017 door Erik de Roos
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface IVrijZoekresultaat
    {
        string ZoekTerm { get; }
        IEnumerable<string> AlleMogelijkheden { get; }
        VrijZoekresultaatAanpassingType ZoeklijstAanpassing { get; }
        IEnumerable<string> DeltaMogelijkhedenVerwijderd { get; }
        IEnumerable<string> DeltaMogelijkhedenToegevoegd { get; }
    }

    public enum VrijZoekresultaatAanpassingType
    {
        Geen,
        Alles,
        Deel
    }
}
