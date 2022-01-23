using iText.Kernel.Pdf;
using iText.Layout;
using System.Collections.Generic;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace LanePackBarcodes
{
    public class DupeObjects
    {
        // Vars for Dupe
        public PdfWriter Writer { get; set; }
        public PdfDocument NewPDF { get; set; }
        public Document NewDoc { get; set; }
        public PdfPage NewPage
        {
            get;
            set;
        }

        // Vars for Master
        public PdfReader Reader { get; set; }
        public PdfDocument MasterPDF { get; set; }
        public iText.Kernel.Pdf.Canvas.Parser.PdfCanvasProcessor Parser { get; set; }

        public PdfDictionary pageResources { get; set; }

        public ICollection<IPdfTextLocation> locationList { get; set; }

        public List<ICollection<IPdfTextLocation>> MasterList { get; set; }
    }
}
