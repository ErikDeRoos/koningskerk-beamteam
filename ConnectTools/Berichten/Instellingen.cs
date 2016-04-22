// Copyright 2016 door Erik de Roos

namespace ConnectTools.Berichten
{
    public class Instellingen
    {
        public int RegelsPerLiedSlide { get; set; }
        public int RegelsPerBijbeltekstSlide { get; set; }
        public StandaardTeksten StandaardTeksten { get; set; }
        public StreamToken TemplateThemeBestand { get; set; }
        public StreamToken TemplateLiedBestand { get; set; }
        public StreamToken TemplateBijbeltekstBestand { get; set; }
    }
}
