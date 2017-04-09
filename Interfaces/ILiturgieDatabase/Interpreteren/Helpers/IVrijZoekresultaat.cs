// Copyright 2017 door Erik de Roos
using System;
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface IVrijZoekresultaat
    {
        string ZoekTerm { get; }
        IEnumerable<IVrijZoekresultaatMogelijkheid> AlleMogelijkheden { get; }
        VrijZoekresultaatAanpassingType ZoeklijstAanpassing { get; }
        IEnumerable<IVrijZoekresultaatMogelijkheid> DeltaMogelijkhedenVerwijderd { get; }
        IEnumerable<IVrijZoekresultaatMogelijkheid> DeltaMogelijkhedenToegevoegd { get; }
    }

    public enum VrijZoekresultaatAanpassingType
    {
        Geen,
        Alles,
        Deel
    }

    public interface IVrijZoekresultaatMogelijkheid : IComparable<IVrijZoekresultaatMogelijkheid>, IEqualityComparer<IVrijZoekresultaatMogelijkheid>
    {
        string Weergave { get; }
        string UitDatabase { get; }
    }
}
