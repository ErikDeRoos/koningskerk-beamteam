// Copyright 2016 door Erik de Roos
using ISettings;
using ISettings.CommonImplementation;
using System;

namespace RemoteGenerator.Builder.Wachtrij
{
    class Instellingen : IInstellingenBase
    {
        public int RegelsPerLiedSlide { get; set; }
        public int RegelsPerBijbeltekstSlide { get; set; }
        public StandaardTeksten StandaardTeksten { get; set; }
        public string FullTemplateTheme { get { return TemplateThemeBestand.LinkOpFilesysteem; } }
        public string FullTemplateLied { get { return TemplateLiedBestand.LinkOpFilesysteem; } }
        public string FullTemplateBijbeltekst { get { return TemplateBijbeltekstBestand.LinkOpFilesysteem; } }

        public BestandStreamToken TemplateThemeBestand { get; set; }
        public BestandStreamToken TemplateLiedBestand { get; set; }
        public BestandStreamToken TemplateBijbeltekstBestand { get; set; }

        public Instellingen(ConnectTools.Berichten.Instellingen vanInstellingen, Func<ConnectTools.Berichten.StreamToken, BestandStreamToken> bestandStreamTokenFactory)
        {
            RegelsPerLiedSlide = vanInstellingen.RegelsPerLiedSlide;
            RegelsPerBijbeltekstSlide = vanInstellingen.RegelsPerBijbeltekstSlide;
            StandaardTeksten = StandaardTeksten = new StandaardTeksten()
            {
                Volgende = vanInstellingen.StandaardTeksten.Volgende,
                Voorganger = vanInstellingen.StandaardTeksten.Voorganger,
                Collecte1 = vanInstellingen.StandaardTeksten.Collecte1,
                Collecte2 = vanInstellingen.StandaardTeksten.Collecte2,
                Collecte = vanInstellingen.StandaardTeksten.Collecte,
                Lezen = vanInstellingen.StandaardTeksten.Lezen,
                Tekst = vanInstellingen.StandaardTeksten.Tekst,
                Liturgie = vanInstellingen.StandaardTeksten.Liturgie,
                LiturgieLezen = vanInstellingen.StandaardTeksten.LiturgieLezen,
                LiturgieTekst = vanInstellingen.StandaardTeksten.LiturgieTekst,
            };
            TemplateThemeBestand = bestandStreamTokenFactory(vanInstellingen.TemplateThemeBestand);
            TemplateLiedBestand = bestandStreamTokenFactory(vanInstellingen.TemplateLiedBestand);
            TemplateBijbeltekstBestand = bestandStreamTokenFactory(vanInstellingen.TemplateBijbeltekstBestand);
        }
    }
}
