﻿// Copyright 2017 door Erik de Roos
using System;
using System.Collections.Generic;

namespace ILiturgieDatabase
{
    public interface IVrijZoekresultaat
    {
        string ZoekTerm { get; }
        bool AlsBijbeltekst { get; }
        string VermoedelijkeDatabase { get; }
        string Aanname { get; }
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

    public interface IVrijZoekresultaatMogelijkheid : IEqualityComparer<IVrijZoekresultaatMogelijkheid>
    {
        string Weergave { get; }
        string VeiligeNaam { get; }
        string UitDatabase { get; }
    }
}
