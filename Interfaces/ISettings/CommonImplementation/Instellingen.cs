// Copyright 2020 door Remco Veurink en Erik de Roos
using System.Collections.Generic;
using System;
using System.Linq;

namespace ISettings.CommonImplementation
{
    public class Instellingen : IInstellingen
    {
        private const int DefaultRegelsperslide = 6;
        private const int DefaultRegelsperbijbeltekstslide = 9;
        private const int DefaultTekstChar_a_OnARow = 41;
        private const string DefaultTekstFontName = "Verdana";
        private const float DefaultTekstFontPointSize = 28;
        private const bool DefaultEen2eCollecte = true;
        private const bool DefaultDeTekstVraag = true;
        private const bool DefaultDeLezenVraag = true;
        private const bool DefaultGebruikDisplayNameVoorZoeken = true;
        private const bool DefaultToonBijbeltekstenInLiturgie = true;
        private const bool DefaultVerkortVerzenBijVolledigeContent = true;
        private const bool DefaultVersOnderbrekingOverSlidesHeen = false;

        public static readonly Instellingen Default = GetDefault();

        public string DatabasePad { get; set; }
        public string BijbelPad { get; set; }
        public string TemplateTheme { get; set; }
        public string TemplateLied { get; set; }
        public string TemplateBijbeltekst { get; set; }
        public int TekstChar_a_OnARow { get; set; }
        public string TekstFontName { get; set; }
        public float TekstFontPointSize { get; set; }
        public int RegelsPerLiedSlide { get; set; }
        public int RegelsPerBijbeltekstSlide { get; set; }
        public bool Een2eCollecte { get; set; }
        public bool DeTekstVraag { get; set; }
        public bool DeLezenVraag { get; set; }
        public bool GebruikDisplayNameVoorZoeken { get; set; }
        public bool ToonBijbeltekstenInLiturgie { get; set; }
        public bool ToonGeenVersenBijVolledigeContent { get; set; }
        public bool VersOnderbrekingOverSlidesHeen { get; set; }

        private readonly List<IMapmask> _lijstmasks = new List<IMapmask>();
        public StandaardTeksten StandaardTeksten { get; set; }

        public Instellingen(StandaardTeksten standaardTeksten = null, IEnumerable<IMapmask> masks = null)
        {
            if (standaardTeksten != null)
                StandaardTeksten = standaardTeksten;
            masks?.ToList().ForEach(m => AddMask(m));
        }

        public bool AddMask(IMapmask mask)
        {
            if (_lijstmasks.Contains(mask)) return false;
            _lijstmasks.Add(mask);
            return true;
        }
        public void ClearMasks()
        {
            _lijstmasks.Clear();
        }

        public string FullDatabasePath => DatabasePad.StartsWith(".") ? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabasePad.Remove(0, 1)) : DatabasePad;
        public string FullBijbelPath => BijbelPad.StartsWith(".") ? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BijbelPad.Remove(0, 1)) : BijbelPad;
        public string FullTemplateTheme => TemplateTheme.StartsWith(".") ? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemplateTheme.Remove(0, 1)) : TemplateTheme;
        public string FullTemplateLied => TemplateLied.StartsWith(".") ? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemplateLied.Remove(0, 1)) : TemplateLied;
        public string FullTemplateBijbeltekst => TemplateBijbeltekst.StartsWith(".") ? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemplateBijbeltekst.Remove(0, 1)) : TemplateBijbeltekst;

        public IEnumerable<IMapmask> Masks => _lijstmasks;

        public static Instellingen GetDefault() {
            return new Instellingen()
            {
                DatabasePad = @".Resources\Database",
                BijbelPad = @".Resources\Bijbels\NBV",
                TemplateTheme = @".Resources\Database\Achtergrond.pptx",
                TemplateLied = @".Resources\Database\Template Liederen.pptx",
                TemplateBijbeltekst = @".Resources\Database\Template Bijbeltekst.pptx",
                TekstChar_a_OnARow = DefaultTekstChar_a_OnARow,
                TekstFontName = DefaultTekstFontName,
                TekstFontPointSize = DefaultTekstFontPointSize,
                RegelsPerLiedSlide = DefaultRegelsperslide,
                RegelsPerBijbeltekstSlide = DefaultRegelsperbijbeltekstslide,
                Een2eCollecte = DefaultEen2eCollecte,
                DeLezenVraag = DefaultDeLezenVraag,
                DeTekstVraag = DefaultDeTekstVraag,
                GebruikDisplayNameVoorZoeken = DefaultGebruikDisplayNameVoorZoeken,
                ToonBijbeltekstenInLiturgie = DefaultToonBijbeltekstenInLiturgie,
                ToonGeenVersenBijVolledigeContent = DefaultVerkortVerzenBijVolledigeContent,
                VersOnderbrekingOverSlidesHeen = DefaultVersOnderbrekingOverSlidesHeen,
                StandaardTeksten = new StandaardTeksten()
                {
                    Volgende = "Straks :",
                    Voorganger = "Voorganger :",
                    Collecte1 = "1e collecte :",
                    Collecte2 = "2e collecte :",
                    Collecte = "Collecte :",
                    Lezen = "Lezen :",
                    Tekst = "Tekst :",
                    Liturgie = "liturgie",
                    LiturgieLezen = "L ",
                    LiturgieTekst = "T "
                }
            };
        }

        public override string ToString()
        {
            return
                $"databasepad: {FullDatabasePath}\n templateliederen: {FullTemplateLied}\n templatetheme: {FullTemplateTheme}\n regels per slide: {RegelsPerLiedSlide}\n";
        }
    }
}
