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
