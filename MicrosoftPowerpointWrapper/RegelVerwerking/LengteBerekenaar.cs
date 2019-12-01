// Copyright 2017 door Erik de Roos
using Generator.LiturgieInterpretator.Models;
using System;
using System.Linq;
using System.Windows.Forms;

namespace mppt.RegelVerwerking
{
    public class LengteBerekenaar : ILengteBerekenaar, IDisposable
    {
        private const int PadMesureShort = 8;  // Arbitrary number
        private const int PadMesureLong = PadMesureShort * 2;

        private System.Drawing.Font _font;
        private System.Drawing.Size _baseSize;
        private int _padSize;

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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _font.Dispose();
                    _font = null;
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
