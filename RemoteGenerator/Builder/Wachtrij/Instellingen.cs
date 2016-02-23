using ISettings;
using ISettings.CommonImplementation;
using System;

namespace RemoteGenerator.Builder.Wachtrij
{
    class Instellingen : IInstellingenBase
    {
        public int Regelsperslide { get; set; }
        public StandaardTeksten StandaardTeksten { get; set; }
        public string FullTemplatetheme { get { return TemplateThemeBestand.LinkOpFilesysteem; } }
        public string FullTemplateliederen { get { return TemplateLiederenBestand.LinkOpFilesysteem; } }

        public BestandStreamToken TemplateThemeBestand { get; set; }
        public BestandStreamToken TemplateLiederenBestand { get; set; }

        public Instellingen(ConnectTools.Berichten.Instellingen vanInstellingen, Func<ConnectTools.Berichten.StreamToken, BestandStreamToken> bestandStreamTokenFactory)
        {
            Regelsperslide = vanInstellingen.Regelsperslide;
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
            TemplateLiederenBestand = bestandStreamTokenFactory(vanInstellingen.TemplateLiederenBestand);
        }
    }
}
