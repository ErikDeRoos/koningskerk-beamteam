using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NetOffice.PowerPointApi;
using NetOffice.OfficeApi.Enums;
using NetOffice.PowerPointApi.Enums;
using Clipboard = System.Windows.Forms.Clipboard;

namespace mppt
{
    public class MppInterfaceApplication : IDisposable
    {
        private Application _applicatie;
        public MppInterfaceApplication()
        {
            _applicatie = new Application { Visible = MsoTriState.msoTrue };
        }

        public void MinimizeInterface()
        {
            _applicatie.WindowState = PpWindowState.ppWindowMinimized;
        }

        public MppInterfacePresentatie Open(string bestandsnaam, bool metWindow = true)
        {
            var presSet = _applicatie.Presentations;
            return new MppInterfacePresentatie(presSet.Open(bestandsnaam, MsoTriState.msoFalse, MsoTriState.msoTrue, metWindow ? MsoTriState.msoTrue : MsoTriState.msoFalse));
        }

        public void Dispose()
        {
            try
            {
                _applicatie?.Quit();
            }
            finally
            {
                _applicatie = null;
            }
        }
    }

    public class MppInterfacePresentatie : IDisposable
    {
        private _Presentation _presentatie;
        private int slideTeller;

        public MppInterfacePresentatie(Presentation presentatie)
        {
            _presentatie = presentatie;
            slideTeller = 0;
        }

        public MppInterfaceSlide EersteSlide()
        {
            return new MppInterfaceSlide(_presentatie.Slides.First() as Slide);
        }

        public IEnumerable<MppInterfaceSlide> AlleSlides()
        {
            foreach (var slide in _presentatie.Slides)
            {
                yield return new MppInterfaceSlide(slide as Slide);
            }
        }
        /// <summary>
        /// Voeg een slide in in de hoofdpresentatie op de volgende positie (hoofdpresentatie werd aangemaakt bij het maken van deze klasse)
        /// </summary>
        /// <param name="slides">de slide die ingevoegd moet worden (voorwaarde is hierbij dat de presentatie waarvan de slide onderdeel is nog wel geopend is)</param>
        public int SlidesKopieNaarPresentatie(IEnumerable<MppInterfaceSlide> slides, int retryCount = 3)
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

    public class MppInterfaceSlide
    {
        private Slide _slide;

        public MppInterfaceSlide(Slide slide)
        {
            _slide = slide;
        }

        public IEnumerable<IMppInterfaceShape> Shapes()
        {
            var shapes = _slide.Shapes.Cast<Shape>().ToList(); 
            foreach (var shape in shapes.Where(s => s.Type == MsoShapeType.msoTextBox))
            {
                yield return new MppInterfaceShapeTextbox(shape);
            }
            foreach (var shape in shapes.Where(s => s.Type == MsoShapeType.msoTable))
            {
                yield return new MppInterfaceShapeTable(shape);
            }
        }

        public void CopyToClipboard()
        {
            _slide.Copy();
        }
    }

    public interface IMppInterfaceShape
    {

    }

    public class MppInterfaceShapeTextbox : IMppInterfaceShape
    {
        private Shape _shape;
        public string Text { get { return _shape.TextFrame.TextRange.Text; } set { _shape.TextFrame.TextRange.Text = value; } }

        public MppInterfaceShapeTextbox(Shape shape)
        {
            _shape = shape;
        }
    }

    public class MppInterfaceShapeTable : IMppInterfaceShape
    {
        private Shape _shape;

        public MppInterfaceShapeTable(Shape shape)
        {
            _shape = shape;
        }

        public string GetTitel()
        {
            return _shape.Table.Rows[1].Cells[1].Shape.TextFrame.TextRange.Text;
        }

        public void InsertContent(IEnumerable<IMppInterfaceShapeTableContent> content)
        {
            var stack = new Stack<IMppInterfaceShapeTableContent>(content.OrderByDescending(c => c.Index));
            var inTabel = _shape.Table;
            for (var index = 1; index <= inTabel.Rows.Count && stack.Any(); index++)
            {
                var cont = stack.Pop();
                var column3 = cont as MppInterfaceShapeTableContent3Column;
                var column1 = cont as MppInterfaceShapeTableContent1Column;

                if (column3 != null)
                {
                    inTabel.Rows[index].Cells[1].Shape.TextFrame.TextRange.Text = column3.Column1;
                    if (!string.IsNullOrWhiteSpace(column3.Column2))
                        inTabel.Rows[index].Cells[2].Shape.TextFrame.TextRange.Text = column3.Column2;
                    if (!string.IsNullOrWhiteSpace(column3.Column3))
                        inTabel.Rows[index].Cells[3].Shape.TextFrame.TextRange.Text = column3.Column3;
                }
                else if (column1 != null)
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
            }
        } 
    }

    public interface IMppInterfaceShapeTableContent
    {
        int Index { get; }
    }
    public class MppInterfaceShapeTableContent3Column : IMppInterfaceShapeTableContent
    {
        public int Index { get; set; }
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }

        public MppInterfaceShapeTableContent3Column(int index, string column1, string column2, string column3)
        {
            Index = index;
            Column1 = column1;
            Column2 = column2;
            Column3 = column3;
        }

    }
    public class MppInterfaceShapeTableContent1Column : IMppInterfaceShapeTableContent
    {
        public int Index { get; set; }
        public string Column1 { get; set; }
        public bool MergeRemainingColumns { get; set; }

        public MppInterfaceShapeTableContent1Column(int index, string column1, bool mergeRemainingColumns)
        {
            Index = index;
            Column1 = column1;
            MergeRemainingColumns = mergeRemainingColumns;
        }
    }
}