// Copyright 2017 door Erik de Roos
using ILiturgieDatabase;
using System.Linq;
using System.Windows.Forms;

namespace mppt.RegelVerwerking
{
    public class LengteBerekenaar : ILengteBerekenaar
    {
        private const int PadMesureShort = 8;  // Arbitrary number
        private const int PadMesureLong = PadMesureShort * 2;

        private System.Drawing.Font _font { get; }
        private System.Drawing.Size _baseSize { get; }
        private int _padSize { get; }

        public LengteBerekenaar(ISettings instellingen)
        {
            var initString = new string(Enumerable.Repeat('a', instellingen.LengteBerekenaarChar_a_OnARow).ToArray());
            _font = new System.Drawing.Font(instellingen.LengteBerekenaarFontName, instellingen.LengteBerekenaarFontPointSize);
            _baseSize = TextRenderer.MeasureText(initString, _font);

            // There is a pad to the end of a string that is automatically added. Calculate the length of this pad.
            var longSize = TextRenderer.MeasureText(new string(Enumerable.Repeat('a', PadMesureLong).ToArray()), _font).Width;
            var shortSize = TextRenderer.MeasureText(new string(Enumerable.Repeat('a', PadMesureShort).ToArray()), _font).Width;
            _padSize = ((shortSize * 2) - longSize) / 2;
        }

        public float VerbruiktPercentageVanRegel(string tekst, bool needsPad)
        {
            var size = TextRenderer.MeasureText(tekst, _font, new System.Drawing.Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.NoClipping);
            if (size.IsEmpty)
                return 0;
            var correctedSize = size.Width - (2 * _padSize);
            if (needsPad)
                correctedSize += _padSize;
            return (float)correctedSize * 100 / _baseSize.Width;
        }
    }
}
