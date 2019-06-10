// Copyright 2019 door Erik de Roos
using System;
using System.Collections.Generic;
using System.Linq;
using NetOffice.PowerPointApi;
using NetOffice.OfficeApi.Enums;
using NetOffice.PowerPointApi.Enums;
using Clipboard = System.Windows.Forms.Clipboard;

namespace mppt.Connect
{
    public class MppApplication : IMppApplication
    {
        private Application _applicatie;
        public MppApplication()
        {
            _applicatie = new Application { Visible = MsoTriState.msoTrue };
        }

        public void MinimizeInterface()
        {
            _applicatie.WindowState = PpWindowState.ppWindowMinimized;
        }

        public IMppPresentatie Open(string bestandsnaam, bool metWindow = true)
        {
            var presSet = _applicatie.Presentations;
            return new MppPresentatie(presSet.Open(bestandsnaam, MsoTriState.msoFalse, MsoTriState.msoTrue, metWindow ? MsoTriState.msoTrue : MsoTriState.msoFalse));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    if (_applicatie != null)
                    {
                        try
                        {
                            _applicatie.Quit();
                        }
                        finally
                        {
                            _applicatie = null;
                        }
                    }
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

    class MppPresentatie : IMppPresentatie
    {
        private _Presentation _presentatie;
        private int slideTeller;

        public MppPresentatie(Presentation presentatie)
        {
            _presentatie = presentatie;
            slideTeller = 0;
        }

        public IMppSlide EersteSlide()
        {
            return new MppSlide(_presentatie.Slides.First() as Slide);
        }

        public IEnumerable<IMppSlide> AlleSlides()
        {
            foreach (var slide in _presentatie.Slides)
            {
                yield return new MppSlide(slide as Slide);
            }
        }
        /// <summary>
        /// Voeg een slide in in de hoofdpresentatie op de volgende positie (hoofdpresentatie werd aangemaakt bij het maken van deze klasse)
        /// </summary>
        /// <param name="slides">de slide die ingevoegd moet worden (voorwaarde is hierbij dat de presentatie waarvan de slide onderdeel is nog wel geopend is)</param>
        public int SlidesKopieNaarPresentatie(IEnumerable<IMppSlide> slides, int retryCount = 3)
        {
            var itemsGemist = 0;
            foreach (var slide in slides.ToList())
            {
                if (slideTeller == 0)
                    _presentatie.Slides[1].Delete();  // Er zit (bijna) altijd een eerste slide in
                slide.CopyToClipboard();
                var gelukt = false;
                for (int currentTry = 1; currentTry < retryCount && gelukt == false; currentTry++)
                {
                    try
                    {
                        gelukt = ExecuteWhen(HasClipboardPowerpointSlideContent, () =>
                        {
                            _presentatie.Slides.Paste();
                            slideTeller++;
                        });
                    }
                    catch (System.Runtime.InteropServices.COMException)
                    {

                    }
                }
                if (!gelukt)
                    itemsGemist++;
            }
            return itemsGemist;
        }
        private static bool ExecuteWhen(Func<bool> isTrue, Action action, int minWaitTime = 5, int maxWaitTime = 250, int waitStep = 25, int waitBeforeExecute = 5)
        {
            var waited = 0;
            if (minWaitTime > 0)
                System.Threading.Thread.Sleep(minWaitTime);
            waited += minWaitTime;
            while (waited < maxWaitTime && waitStep > 0 && !isTrue.Invoke())
            {
                System.Threading.Thread.Sleep(waitStep);
                waited += waitStep;
            }
            if (isTrue.Invoke())
            {
                if (waitBeforeExecute > 0)
                    System.Threading.Thread.Sleep(waitBeforeExecute);
                action.Invoke();
                return true;
            }
            return false;
        }

        private static bool HasClipboardPowerpointSlideContent()
        {
            var data = Clipboard.GetDataObject();
            if (data == null)
                return false;
            var formats = data.GetFormats();
            return formats.Any(f => f.StartsWith("PowerPoint"));
        }

        public void OpslaanAls(string bestandsnaam)
        {
            _presentatie.SaveAs(bestandsnaam);
        }

        public void Dispose()
        {
            try
            {
                _presentatie?.Close();
                _presentatie?.Dispose();
            }
            finally
            {
                _presentatie = null;
            }
        }
    }

    class MppSlide : IMppSlide
    {
        private Slide _slide;

        public MppSlide(Slide slide)
        {
            _slide = slide;
        }

        public IEnumerable<IMppShape> Shapes()
        {
            var shapes = _slide.Shapes.Cast<Shape>().ToList(); 
            foreach (var shape in shapes.Where(s => s.Type == MsoShapeType.msoTextBox))
            {
                yield return new MppShapeTextbox(shape);
            }
            foreach (var shape in shapes.Where(s => s.Type == MsoShapeType.msoTable))
            {
                yield return new MppShapeTable(shape);
            }
        }

        public void CopyToClipboard()
        {
            _slide.Copy();
        }
    }

    public class MppShapeTextbox : IMppShapeTextbox
    {
        private Shape _shape;
        public string Text { get { return _shape.TextFrame.TextRange.Text; } set { _shape.TextFrame.TextRange.Text = value; } }

        public MppShapeTextbox(Shape shape)
        {
            _shape = shape;
        }
    }

    class MppShapeTable : IMppShapeTable
    {
        private Shape _shape;
        private int _amountOfRowsFilled;

        public MppShapeTable(Shape shape)
        {
            _shape = shape;
        }

        public string GetTitelFromFirstRowCell()
        {
            return _shape.Table.Rows[1].Cells[1].Shape.TextFrame.TextRange.Text;
        }

        public void SetRowsContent(IEnumerable<IMppShapeTableContent> content)
        {
            var stack = new Stack<IMppShapeTableContent>(content.OrderByDescending(c => c.Index));
            var inTabel = _shape.Table;
            _amountOfRowsFilled = 0;
            for (var index = 1; index <= inTabel.Rows.Count && stack.Any(); index++)
            {
                var cont = stack.Pop();

                if (cont is MppShapeTableContent3Column column3)
                {
                    inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text = column3.Column1;
                    if (string.IsNullOrWhiteSpace(column3.Column3) && !string.IsNullOrWhiteSpace(column3.Column2) && NeedForColumnMergeBecauseOfPossibleOverflowColumn2(column3.Column2))
                    {
                        inTabel.Rows[index].Cells[2].Merge(inTabel.Rows[index].Cells[3]);  // row 3 has (probably) better aligning
                        inTabel.Rows[index].Cells[2].Shape.TextFrame.TextRange.ParagraphFormat.Alignment = PpParagraphAlignment.ppAlignLeft;
                    }
                    if (!string.IsNullOrWhiteSpace(column3.Column2))
                        inTabel.Rows[index].Cells[2].Shape.TextFrame.TextRange.Text = column3.Column2;
                    if (!string.IsNullOrWhiteSpace(column3.Column3))
                        inTabel.Rows[index].Cells[3].Shape.TextFrame.TextRange.Text = column3.Column3;
                }
                else if (cont is MppShapeTableContent1Column column1)
                {
                    if (column1.MergeRemainingColumns && inTabel.Rows[index].Cells.Count >= 2)
                    {
                        inTabel.Rows[index].Cells[1].Merge(inTabel.Rows[index].Cells[2]);
                        if (inTabel.Rows[index].Cells.Count >= 3)
                            inTabel.Rows[index].Cells[2].Merge(inTabel.Rows[index].Cells[3]);
                    }
                    inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text = column1.Column1;
                    inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.ParagraphFormat.Alignment = PpParagraphAlignment.ppAlignLeft;
                }
                _amountOfRowsFilled++;
            }
        }
        
        private bool NeedForColumnMergeBecauseOfPossibleOverflowColumn2(string column2)
        {
            var testValue = (column2 ?? "").Trim();
            return testValue.Contains(" ") || testValue.Length > 5;
        } 

        public void TrimRows()
        {
            var inTabel = _shape.Table;
            if (inTabel.Rows.Count <= _amountOfRowsFilled)
                return;

            for (int teller = inTabel.Rows.Count; teller > _amountOfRowsFilled; teller--)
            {
                inTabel.Rows[teller].Delete();
            }
        }
    }

    class MppShapeTableContent3Column : IMppShapeTableContent
    {
        public int Index { get; set; }
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }

        public MppShapeTableContent3Column(int index, string column1, string column2, string column3)
        {
            Index = index;
            Column1 = column1;
            Column2 = column2;
            Column3 = column3;
        }
    }
    class MppShapeTableContent1Column : IMppShapeTableContent
    {
        public int Index { get; set; }
        public string Column1 { get; set; }
        public bool MergeRemainingColumns { get; set; }

        public MppShapeTableContent1Column(int index, string column1, bool mergeRemainingColumns)
        {
            Index = index;
            Column1 = column1;
            MergeRemainingColumns = mergeRemainingColumns;
        }
    }
}