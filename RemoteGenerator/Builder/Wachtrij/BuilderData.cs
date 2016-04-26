// Copyright 2016 door Erik de Roos
using ISlideBuilder;
using System;

namespace RemoteGenerator.Builder.Wachtrij
{
    class BuilderData : ConnectTools.Berichten.BuilderData, IBuilderBuildDefaults, IBuilderDependendFiles
    {
        public string FullTemplateTheme { get { return TemplateThemeBestand.LinkOpFilesysteem; } }
        public string FullTemplateLied { get { return TemplateLiedBestand.LinkOpFilesysteem; } }
        public string FullTemplateBijbeltekst { get { return TemplateBijbeltekstBestand.LinkOpFilesysteem; } }

        public new BestandStreamToken TemplateThemeBestand { get; set; }
        public new BestandStreamToken TemplateLiedBestand { get; set; }
        public new BestandStreamToken TemplateBijbeltekstBestand { get; set; }

        public BuilderData(ConnectTools.Berichten.BuilderData vanBuilderData, Func<ConnectTools.Berichten.StreamToken, BestandStreamToken> bestandStreamTokenFactory)
        {
            RegelsPerLiedSlide = vanBuilderData.RegelsPerLiedSlide;
            RegelsPerBijbeltekstSlide = vanBuilderData.RegelsPerBijbeltekstSlide;
            LabelVolgende = vanBuilderData.LabelVolgende;
            LabelVoorganger = vanBuilderData.LabelVoorganger;
            LabelCollecte1 = vanBuilderData.LabelCollecte1;
            LabelCollecte2 = vanBuilderData.LabelCollecte2;
            LabelCollecte = vanBuilderData.LabelCollecte;
            LabelLezen = vanBuilderData.LabelLezen;
            LabelTekst = vanBuilderData.LabelTekst;
            LabelLiturgie = vanBuilderData.LabelLiturgie;
            LabelLiturgieLezen = vanBuilderData.LabelLiturgieLezen;
            LabelLiturgieTekst = vanBuilderData.LabelLiturgieTekst;
            TemplateThemeBestand = bestandStreamTokenFactory(vanBuilderData.TemplateThemeBestand);
            TemplateLiedBestand = bestandStreamTokenFactory(vanBuilderData.TemplateLiedBestand);
            TemplateBijbeltekstBestand = bestandStreamTokenFactory(vanBuilderData.TemplateBijbeltekstBestand);
        }
    }
}
