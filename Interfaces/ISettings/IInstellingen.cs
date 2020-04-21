// Copyright 2016 door Erik de Roos
using System.Collections.Generic;

namespace ISettings
{
    public interface IInstellingen : IInstellingenBase
    {
        int TekstChar_a_OnARow { get; }
        string TekstFontName { get; }
        float TekstFontPointSize { get; }
        bool VersOnderbrekingOverSlidesHeen { get; }

        string DatabasePad { get; }
        string BijbelPad { get; }
        string TemplateLied { get; }
        string TemplateTheme { get; }
        string TemplateBijbeltekst { get; }

        bool Een2eCollecte { get; }
        bool DeTekstVraag { get; }
        bool DeLezenVraag { get; }
        bool GebruikDisplayNameVoorZoeken { get; }

        bool ToonBijbeltekstenInLiturgie { get; }
        bool ToonGeenVersenBijVolledigeContent { get; }

        IEnumerable<IMapmask> Masks { get; }
        bool AddMask(IMapmask mask);
        void ClearMasks();

        string FullDatabasePath { get; }
        string FullBijbelPath { get; }
        bool ToonAlsLiedOnderbrokenWordt { get; set; }
    }
}
