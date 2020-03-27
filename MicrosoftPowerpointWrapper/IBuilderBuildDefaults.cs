// Copyright 2020 door Erik de Roos

namespace mppt
{
    /// <summary>
    /// Instellingen die normaliter niet wijzigen tussen slide generaties
    /// </summary>
    public interface IBuilderBuildDefaults
    {
        int RegelsPerLiedSlide { get; }
        int RegelsPerBijbeltekstSlide { get; }
        string LabelVolgende { get; set; }
        string LabelVoorganger { get; set; }
        string LabelCollecte1 { get; set; }
        string LabelCollecte2 { get; set; }
        string LabelCollecte { get; set; }
        string LabelLezen { get; set; }
        string LabelTekst { get; set; }
        string LabelLiturgie { get; set; }
        string LabelLiturgieLezen { get; set; }
        string LabelLiturgieTekst { get; set; }
        bool VerkortVerzenBijVolledigeContent { get; set; }
        bool VersOnderbrekingOverSlidesHeen { get; set; }
    }
}
