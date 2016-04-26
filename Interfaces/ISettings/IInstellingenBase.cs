// Copyright 2016 door Erik de Roos
namespace ISettings
{
    public interface IInstellingenBase
    {
        int RegelsPerLiedSlide { get; }
        int RegelsPerBijbeltekstSlide { get; }
        CommonImplementation.StandaardTeksten StandaardTeksten { get; }

        string FullTemplateTheme { get; }
        string FullTemplateLied { get; }
        string FullTemplateBijbeltekst { get; }
    }
}
