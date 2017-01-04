// Copyright 2017 door Erik de Roos

namespace mppt
{
    /// <summary>
    /// All on-the-fly settings the local classes need
    /// </summary>
    public interface ISettings
    {
        int LengteBerekenaarChar_a_OnARow { get; }
        string LengteBerekenaarFontName { get; }
        float LengteBerekenaarFontPointSize { get; }
    }
}
