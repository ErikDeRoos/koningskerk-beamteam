// Copyright 2016 door Erik de Roos
namespace ISettings
{
    public interface IInstellingenBase
    {
        int Regelsperslide { get; }
        CommonImplementation.StandaardTeksten StandaardTeksten { get; }

        string FullTemplatetheme { get; }

        string FullTemplateliederen { get; }
    }
}
