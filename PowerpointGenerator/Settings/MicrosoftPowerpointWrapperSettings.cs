// Copyright 2017 door Erik de Roos
using ISettings;

namespace PowerpointGenerator.Settings
{
    class MicrosoftPowerpointWrapperSettings : mppt.ISettings
    {
        public int LengteBerekenaarChar_a_OnARow { get; }
        public string LengteBerekenaarFontName { get; }
        public float LengteBerekenaarFontPointSize { get; }

        public MicrosoftPowerpointWrapperSettings(IInstellingenFactory instellingenOplosser)
        {
            var settings = instellingenOplosser.LoadFromXmlFile();
            LengteBerekenaarChar_a_OnARow = settings.TekstChar_a_OnARow;
            LengteBerekenaarFontName = settings.TekstFontName;
            LengteBerekenaarFontPointSize = settings.TekstFontPointSize;
        }
    }
}
