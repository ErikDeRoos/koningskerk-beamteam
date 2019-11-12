// Copyright 2016 door Erik de Roos

namespace mppt
{
    /// <summary>
    /// Instellingen die wijzigen tussen slide generaties
    /// </summary>
    public interface IBuilderBuildSettings
    {
        string Voorganger { get; }
        string Collecte1 { get; }
        string Collecte2 { get; }
        string Lezen { get; }
        string Tekst { get; }

        bool Een2eCollecte { get; }
    }
}
