// Copyright 2016 door Erik de Roos

namespace ConnectTools.Berichten
{
    public class BuilderData
    {
        public int RegelsPerLiedSlide { get; set; }
        public int RegelsPerBijbeltekstSlide { get; set; }
        public StreamToken TemplateThemeBestand { get; set; }
        public StreamToken TemplateLiedBestand { get; set; }
        public StreamToken TemplateBijbeltekstBestand { get; set; }
        public string LabelVolgende { get; set; }
        public string LabelVoorganger { get; set; }
        public string LabelCollecte1 { get; set; }
        public string LabelCollecte2 { get; set; }
        public string LabelCollecte { get; set; }
        public string LabelLezen { get; set; }
        public string LabelTekst { get; set; }
        public string LabelLiturgie { get; set; }
        public string LabelLiturgieLezen { get; set; }
        public string LabelLiturgieTekst { get; set; }
    }
}
