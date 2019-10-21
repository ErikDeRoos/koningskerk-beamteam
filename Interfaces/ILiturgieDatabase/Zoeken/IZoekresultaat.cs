// Copyright 2017 door Erik de Roos

namespace ILiturgieDatabase
{
    public interface IZoekresultaat
    {
        IZoekresultaatEntry Resultaat { get; }
        IZoekresultaatBron Database { get; }
    }

    public interface IZoekresultaatEntry
    {
        string Weergave { get; }
        string VeiligeNaam { get; }
    }

    public interface IZoekresultaatBron
    {
        string Weergave { get; }
    }
}
