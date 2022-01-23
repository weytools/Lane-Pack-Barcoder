using System;
using System.ComponentModel;
using LanePackBarcodes.Data;
using LanePackBarcodes.Logic;
using System.Windows;
using System.Windows.Forms;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Barcodes;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using System.IO;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Collections.Generic;
using MessageBox = System.Windows.Forms.MessageBox;

namespace LanePackBarcodes
{
    // TODO Create gates to disallow entering blank information

    /// <summary>
    /// View Model for updating everything on screen
    /// </summary>
    public class MainVM : INotifyPropertyChanged
    {
        #region  Property Change Listener
        /// <summary>
        /// Firing this event will update all binded controls
        /// Fire using this code in the setter of the property
        /// <code>PropertyChanged(this, new PropertyChangedEventArgs(nameof(MyProp)));</code></summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };


        /// Make sure all properties are <public>!
        /// Make sure all properties have matching <private var>!
        #endregion
        #region LocalEnums
        public enum FileChoice
        {
            original,
            newlyCreated
        }

        public enum FieldToBarcode
        {
            pid,
            lanePack,
            soa,
            packDate,
            audit
        }

        public enum FieldJustify
        {
            left,
            center,
            right
        }
        #endregion
        #region Properties
        // initialize new data
        public JobInfo CurrentJob { get; set; } = new JobInfo { };
        public LanePack CurrentLanePack { get; set; } = new LanePack { };
        public AlertMessage CurrentAlert { get; set; } = new AlertMessage { };
        public DupeObjects CurrentDupe { get; set; } = new DupeObjects { };

        // Alert Visibilities
        // Can hide all using the method CollapseAllMessages()
        private Visibility visMatch = Visibility.Collapsed;
        private Visibility visNoMatch = Visibility.Collapsed;
        // Match Found message, No Match Found message, Custom Alert message
        public Visibility VisMatchFound
        {
            get { return visMatch; }
            set
            {
                visMatch = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(VisMatchFound)));
            }
        }
        public Visibility VisNoMatchFound
        {
            get { return visNoMatch; }
            set
            {
                visNoMatch = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(VisNoMatchFound)));
            }
        }

        // Field strings to search for
        private string pidString = "PID #:__________________";
        private string lanePackString = "Inspection & Packing Plan";
        private string soaString = "Applied to Order # SOA:__________________";
        private string packDateString = "DATE:____/____/________";
        private string auditString = "Audit";

        #endregion
        #region Methods (UI related)
        #region PDF Generation
        /// <summary>
        /// Button: Main method to call from UI.
        /// <para>Calls all generation methods in order</para>
        /// </summary>
        public void GeneratePDF()
        {
            // reset messages
            CollapseAllMessages();

            // check for fields
            if (CurrentJob.PIDNumber == String.Empty || CurrentJob.SOANumber == String.Empty)
            {
                CurrentAlert.SetAlert("Missing fields.", Visibility.Visible);
                MessageBox.Show(
                    "PID Number and SOA Number are required.",
                    "Empty Fields",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);


                return;
            }

            // create directory, filename, and filepath
            CurrentLanePack.NewPath = SetFilePath();

            // create file
            CreateDupeFile(CurrentLanePack.NewPath);

            // create master list of all locations on PDF
            SetUpLocations();

            // add all barcodes
            AddBarcodes();

            /* TODO: Add quantity text box at bottom right?
            // append Quantity number
            if (CurrentJob.Quantity != 0) { }
            */

            // save and close PDF objects
            CurrentDupe.NewDoc.Close();
            CurrentDupe.NewPDF.Close();

            //sets alert
            CollapseAllMessages();
            CurrentAlert.AlertText = "Generation complete!";
            CurrentAlert.AlertVis = Visibility.Visible;

        }

        /// <summary>
        /// Creates a file path for a renamed copy of the master PDF
        /// </summary>
        /// <returns>proposed filepath for the duplicated PDF</returns>
        public string SetFilePath()
        {
            // create folder
            string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            localPath += "\\Lane Pack Barcoder\\";
            Directory.CreateDirectory(localPath);

            // set up filename
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
            string filename = CurrentJob.PIDNumber + " " + CurrentLanePack.PackName + " " + timestamp + ".pdf";

            // return proposed filepath
            return localPath + filename;
        }

        /// <summary>
        /// Creates iText documents and saves them in the DupeObjects class
        /// </summary>
        /// <param name="filepath">the dupe pdf filename from CopyMaster()</param>
        public void CreateDupeFile(string filepath)
        {
            // vars for new PDF
            CurrentDupe.Writer = new PdfWriter(filepath);
            CurrentDupe.NewPDF = new PdfDocument(CurrentDupe.Writer);
            CurrentDupe.NewDoc = new Document(CurrentDupe.NewPDF);

            // vars for master
            string masterPath = CurrentLanePack.PDFPath;
            CurrentDupe.Reader = new PdfReader(masterPath);
            CurrentDupe.MasterPDF = new PdfDocument(CurrentDupe.Reader);

            // copy master to dupe
            CurrentDupe.MasterPDF.CopyPagesTo(1, 1, CurrentDupe.NewPDF);

            // close master
            CurrentDupe.MasterPDF.Close();
            CurrentDupe.NewPage = CurrentDupe.NewPDF.GetFirstPage();
        }

        /** 
        <summary>
        Saves the PDF locations of the given fields into CurrentDupe.MasterList
        <para>Access each of the location lists using their 0 index from MasterList:</para>
        <list type="number">
            <listheader><term>PID Number</term><description><code>CurrentDupe.MasterList[0]</code></description></listheader>
            <item><term>Lane Pack Number</term><description><code>CurrentDupe.MasterList[1]</code></description></item>
            <item><term>SOA Number</term><description><code>CurrentDupe.MasterList[2]</code></description></item>
            <item><term>Generation Date</term><description><code>CurrentDupe.MasterList[3]</code></description></item>
        </list>
        </summary>
            
            */
        public void SetUpLocations()
        {
            // TODO: Refactor using lists and foreach

            // Extractors
            iText.Kernel.Pdf.Canvas.Parser.Listener.RegexBasedLocationExtractionStrategy pidExtract =
                new iText.Kernel.Pdf.Canvas.Parser.Listener.RegexBasedLocationExtractionStrategy(pidString);

            iText.Kernel.Pdf.Canvas.Parser.Listener.RegexBasedLocationExtractionStrategy lanePackExtract =
    new iText.Kernel.Pdf.Canvas.Parser.Listener.RegexBasedLocationExtractionStrategy(lanePackString);

            iText.Kernel.Pdf.Canvas.Parser.Listener.RegexBasedLocationExtractionStrategy soaExtract =
    new iText.Kernel.Pdf.Canvas.Parser.Listener.RegexBasedLocationExtractionStrategy(soaString);

            iText.Kernel.Pdf.Canvas.Parser.Listener.RegexBasedLocationExtractionStrategy packDateExtract =
    new iText.Kernel.Pdf.Canvas.Parser.Listener.RegexBasedLocationExtractionStrategy(packDateString);

            // Parsers
            PdfCanvasProcessor pidParser = new PdfCanvasProcessor(pidExtract);
            PdfCanvasProcessor lanePackParser = new PdfCanvasProcessor(lanePackExtract);
            PdfCanvasProcessor soaParser = new PdfCanvasProcessor(soaExtract);
            PdfCanvasProcessor packDateParser = new PdfCanvasProcessor(packDateExtract);

            // Execute parse
            pidParser.ProcessPageContent(CurrentDupe.NewPage);
            lanePackParser.ProcessPageContent(CurrentDupe.NewPage);
            soaParser.ProcessPageContent(CurrentDupe.NewPage);
            packDateParser.ProcessPageContent(CurrentDupe.NewPage);

            // Return rectangles
            ICollection<IPdfTextLocation> pidLocationList = pidExtract.GetResultantLocations();
            ICollection<IPdfTextLocation> lanePackLocationList = lanePackExtract.GetResultantLocations();
            ICollection<IPdfTextLocation> soaLocationList = soaExtract.GetResultantLocations();
            ICollection<IPdfTextLocation> packDateLocationList = packDateExtract.GetResultantLocations();

            // Save lists
            CurrentDupe.MasterList = new List<ICollection<IPdfTextLocation>>();
            CurrentDupe.MasterList.Add(pidLocationList);
            CurrentDupe.MasterList.Add(lanePackLocationList);
            CurrentDupe.MasterList.Add(soaLocationList);
            CurrentDupe.MasterList.Add(packDateLocationList);

        }

        /// <summary>
        /// New method of adding barcodes.
        /// Dynimically finds the location of the existing field, covers it with whiteout, and places barcode over it
        /// </summary>
        /// <param name="toBarcode">String to be encoded</param>
        /// <param name="fieldEnum">PDF field it will replace</param>
        public void AddBarcodes()
        {

            // Width of each bar
            // Default is 0.8
            float barWidth = 1.5f;

            // TODO Clean up the if-thens since i hard coded the fields
            // PID
            BarcodeProps pidProps = new BarcodeProps(FieldToBarcode.pid);
            foreach (IPdfTextLocation location in CurrentDupe.MasterList[0])
            {

                //create layer
                PdfCanvas newCanvas = new PdfCanvas(CurrentDupe.NewPage, true);



                // create new rectangle with same size & position
                Rectangle newRect = location.GetRectangle();

                // format rectangle
                newCanvas.SetFillColor(ColorConstants.WHITE).SetStrokeColor(ColorConstants.WHITE).Rectangle(newRect).FillStroke();

                // create the actual barcode
                Barcode1D newcode = new Barcode128(CurrentDupe.NewPDF);
                newcode.SetCodeType(Barcode128.CODE128);
                newcode.SetCode(CurrentJob.PIDNumber);
                newcode.SetX(barWidth);

                // set up placement objects
                Rectangle rect = newcode.GetBarcodeSize();
                PdfFormXObject template = new PdfFormXObject(new Rectangle(rect.GetWidth(), rect.GetHeight()));
                PdfCanvas templateCanvas = new PdfCanvas(template, CurrentDupe.NewPDF);

                // put barcode in object
                newcode.PlaceBarcode(templateCanvas, ColorConstants.BLACK, ColorConstants.BLACK);
                iText.Layout.Element.Image image = new iText.Layout.Element.Image(template);

                // place object in image object and format image
                image.SetRotationAngle((Math.PI / 180) * 270);
                image.SetAutoScale(false);

                // height of final barcode
                int barcodeOutHeight = 26;
                image.SetHeight(barcodeOutHeight);

                // These var names are respective to the final output PDF; landscape mode
                float barcodeUpAmount;
                float barcodeLeftAmount;

                // calculating width of final barcode
                float barcodeRatio = (barcodeOutHeight / image.GetImageHeight());
                float barcodeOutWidth = (image.GetImageWidth() * barcodeRatio);

                // up amount
                barcodeUpAmount = newRect.GetLeft() - (newRect.GetWidth() / pidProps.UpModifier);

                // left amount
                if (pidProps.Justify == FieldJustify.left)
                    barcodeLeftAmount = newRect.GetBottom() + newRect.GetHeight();
                else if (pidProps.Justify == FieldJustify.right)
                    barcodeLeftAmount = newRect.GetBottom() + barcodeOutWidth;
                else   //(props.Justify == FieldJustify.center)
                    barcodeLeftAmount = newRect.GetBottom() + (newRect.GetHeight() / 2) + (barcodeOutWidth / 2);

                // committing final position
                image.SetFixedPosition(barcodeUpAmount, barcodeLeftAmount);

                // adding to document
                CurrentDupe.NewDoc.Add(image);
            }

            // LANE PACK
            BarcodeProps lanePackProps = new BarcodeProps(FieldToBarcode.lanePack);
            foreach (IPdfTextLocation location in CurrentDupe.MasterList[1])
            {

                //create layer
                PdfCanvas newCanvas = new PdfCanvas(CurrentDupe.NewPage, true);



                // create new rectangle with same size & position
                Rectangle newRect = location.GetRectangle();

                // format rectangle
                newCanvas.SetFillColor(ColorConstants.WHITE).SetStrokeColor(ColorConstants.WHITE).Rectangle(newRect).FillStroke();

                // create the actual barcode
                Barcode1D newcode = new Barcode128(CurrentDupe.NewPDF);
                newcode.SetCodeType(Barcode128.CODE128);
                newcode.SetCode(CurrentLanePack.PackName);
                newcode.SetX(barWidth);

                // set up placement objects
                Rectangle rect = newcode.GetBarcodeSize();
                PdfFormXObject template = new PdfFormXObject(new Rectangle(rect.GetWidth(), rect.GetHeight()));
                PdfCanvas templateCanvas = new PdfCanvas(template, CurrentDupe.NewPDF);

                // put barcode in object
                newcode.PlaceBarcode(templateCanvas, ColorConstants.BLACK, ColorConstants.BLACK);
                iText.Layout.Element.Image image = new iText.Layout.Element.Image(template);

                // place object in image object and format image
                image.SetRotationAngle((Math.PI / 180) * 270);
                image.SetAutoScale(false);

                // height of final barcode
                int barcodeOutHeight = 26;
                image.SetHeight(barcodeOutHeight);

                // These var names are respective to the final output PDF; landscape mode
                float barcodeUpAmount;
                float barcodeLeftAmount;

                // calculating width of final barcode
                float barcodeRatio = (barcodeOutHeight / image.GetImageHeight());
                float barcodeOutWidth = (image.GetImageWidth() * barcodeRatio);

                // up amount
                barcodeUpAmount = newRect.GetLeft() - (newRect.GetWidth() / lanePackProps.UpModifier);

                // left amount
                if (lanePackProps.Justify == FieldJustify.left)
                    barcodeLeftAmount = newRect.GetBottom() + newRect.GetHeight();
                else if (lanePackProps.Justify == FieldJustify.right)
                    barcodeLeftAmount = newRect.GetBottom() + barcodeOutWidth;
                else   //(props.Justify == FieldJustify.center)
                    barcodeLeftAmount = newRect.GetBottom() + (newRect.GetHeight() / 2) + (barcodeOutWidth / 2);

                // committing final position
                image.SetFixedPosition(barcodeUpAmount, barcodeLeftAmount);

                // adding to document
                CurrentDupe.NewDoc.Add(image);
            }

            // SOA 
            BarcodeProps soaProps = new BarcodeProps(FieldToBarcode.soa);
            foreach (IPdfTextLocation location in CurrentDupe.MasterList[2])
            {

                //create layer
                PdfCanvas newCanvas = new PdfCanvas(CurrentDupe.NewPage, true);



                // create new rectangle with same size & position
                Rectangle newRect = location.GetRectangle();

                // format rectangle
                newCanvas.SetFillColor(ColorConstants.WHITE).SetStrokeColor(ColorConstants.WHITE).Rectangle(newRect).FillStroke();

                // create the actual barcode
                Barcode1D newcode = new Barcode128(CurrentDupe.NewPDF);
                newcode.SetCodeType(Barcode128.CODE128);
                newcode.SetCode(CurrentJob.SOANumber);
                newcode.SetX(barWidth);

                // set up placement objects
                Rectangle rect = newcode.GetBarcodeSize();
                PdfFormXObject template = new PdfFormXObject(new Rectangle(rect.GetWidth(), rect.GetHeight()));
                PdfCanvas templateCanvas = new PdfCanvas(template, CurrentDupe.NewPDF);

                // put barcode in object
                newcode.PlaceBarcode(templateCanvas, ColorConstants.BLACK, ColorConstants.BLACK);
                iText.Layout.Element.Image image = new iText.Layout.Element.Image(template);

                // place object in image object and format image
                image.SetRotationAngle((Math.PI / 180) * 270);
                image.SetAutoScale(false);

                // height of final barcode
                int barcodeOutHeight = 26;
                image.SetHeight(barcodeOutHeight);

                // These var names are respective to the final output PDF; landscape mode
                float barcodeUpAmount;
                float barcodeLeftAmount;

                // calculating width of final barcode
                float barcodeRatio = (barcodeOutHeight / image.GetImageHeight());
                float barcodeOutWidth = (image.GetImageWidth() * barcodeRatio);

                // up amount
                barcodeUpAmount = newRect.GetLeft() - (newRect.GetWidth() / soaProps.UpModifier);

                // left amount
                if (soaProps.Justify == FieldJustify.left)
                    barcodeLeftAmount = newRect.GetBottom() + newRect.GetHeight();
                else if (soaProps.Justify == FieldJustify.right)
                    barcodeLeftAmount = newRect.GetBottom() + barcodeOutWidth;
                else   //(props.Justify == FieldJustify.center)
                    barcodeLeftAmount = newRect.GetBottom() + (newRect.GetHeight() / 2) + (barcodeOutWidth / 2);

                // committing final position
                image.SetFixedPosition(barcodeUpAmount, barcodeLeftAmount);

                // adding to document
                CurrentDupe.NewDoc.Add(image);
            }

            // DATE
            BarcodeProps packDateProps = new BarcodeProps(FieldToBarcode.packDate);
            foreach (IPdfTextLocation location in CurrentDupe.MasterList[3])
            {

                //create layer
                PdfCanvas newCanvas = new PdfCanvas(CurrentDupe.NewPage, true);

                string theDate = DateTime.Today.ToShortDateString();

                // create new rectangle with same size & position
                Rectangle newRect = location.GetRectangle();

                // format rectangle
                newCanvas.SetFillColor(ColorConstants.WHITE).SetStrokeColor(ColorConstants.WHITE).Rectangle(newRect).FillStroke();

                // create the actual barcode
                Barcode1D newcode = new Barcode128(CurrentDupe.NewPDF);
                newcode.SetCodeType(Barcode128.CODE128);
                newcode.SetCode(theDate);
                newcode.SetX(barWidth);

                // set up placement objects
                Rectangle rect = newcode.GetBarcodeSize();
                PdfFormXObject template = new PdfFormXObject(new Rectangle(rect.GetWidth(), rect.GetHeight()));
                PdfCanvas templateCanvas = new PdfCanvas(template, CurrentDupe.NewPDF);

                // put barcode in object
                newcode.PlaceBarcode(templateCanvas, ColorConstants.BLACK, ColorConstants.BLACK);
                iText.Layout.Element.Image image = new iText.Layout.Element.Image(template);

                // place object in image object and format image
                image.SetRotationAngle((Math.PI / 180) * 270);
                image.SetAutoScale(false);

                // height of final barcode
                int barcodeOutHeight = 26;
                image.SetHeight(barcodeOutHeight);

                // These var names are respective to the final output PDF; landscape mode
                float barcodeUpAmount;
                float barcodeLeftAmount;

                // calculating width of final barcode
                float barcodeRatio = (barcodeOutHeight / image.GetImageHeight());
                float barcodeOutWidth = (image.GetImageWidth() * barcodeRatio);

                // up amount
                barcodeUpAmount = newRect.GetLeft() - (newRect.GetWidth() / packDateProps.UpModifier);

                // left amount
                if (packDateProps.Justify == FieldJustify.left)
                    barcodeLeftAmount = newRect.GetBottom() + newRect.GetHeight();
                else if (packDateProps.Justify == FieldJustify.right)
                    barcodeLeftAmount = newRect.GetBottom() + barcodeOutWidth;
                else   //(props.Justify == FieldJustify.center)
                    barcodeLeftAmount = newRect.GetBottom() + (newRect.GetHeight() / 2) + (barcodeOutWidth / 2);

                // committing final position
                image.SetFixedPosition(barcodeUpAmount, barcodeLeftAmount);

                // adding to document
                CurrentDupe.NewDoc.Add(image);
            }
        }
        #endregion

        /// <summary>
        /// Button: Resets all variables 
        /// </summary>
        public void NewData()
        {
            CurrentJob.PIDNumber = String.Empty;
            CurrentJob.SOANumber = String.Empty;
            CurrentJob.Quantity = 0;
            CurrentLanePack.PDFPath = "No file chosen";
            CurrentLanePack.PackName = String.Empty;
            CurrentLanePack.NewPath = String.Empty;
            FileSearch.FoundMultiple = false;

            CollapseAllMessages();
            CurrentAlert.AlertText = "Fields reset.";
            CurrentAlert.AlertVis = Visibility.Visible;
        }

        /// <summary>
        /// Launches the associated PDF file
        /// </summary>
        /// <param name="file">enum for either the original PDF, or the newly created generated one</param>
        public void OpenFile(FileChoice file)
        {
            if (file == FileChoice.original)
                System.Diagnostics.Process.Start(CurrentLanePack.PDFPath);
            else if (file == FileChoice.newlyCreated)
                System.Diagnostics.Process.Start(CurrentLanePack.NewPath);
        }

        /// <summary>
        /// Button: Launches file picker dialog
        /// </summary>
        public void BrowseForFile()
        {
            CollapseAllMessages();
            OpenFileDialog result = new OpenFileDialog()
            {
                Filter = "PDF files (*.PDF)|*.pdf",
                Title = "Choose a PDF",
                InitialDirectory = FileSearch.networkPath
            };

            if (result.ShowDialog() == DialogResult.OK)
            {
                CurrentLanePack.PDFPath = result.FileName.ToString();
            }

        }

        /// <summary>
        /// Hides all messages. 
        /// </summary>
        public void CollapseAllMessages()
        {
            this.VisMatchFound = Visibility.Collapsed;
            this.VisNoMatchFound = Visibility.Collapsed;
            CurrentAlert.AlertVis = Visibility.Collapsed;
            /* TODO Create static class that with the following methods to mass maniupulate messages
                Messages.ShowMatch
                Messages.ShowNoMatch
                Messages.CollapseMatch
                Messages.CollapseNoMatch
                Messages.CollapseAll
            */
        }

        /// <summary>
        /// Button: Searches for the PDF on the network: retrieves the path on button click
        /// </summary>
        public void RetrievePDF()
        {

            // clear messages
            this.CurrentAlert.TurnOffAlert();
            this.CollapseAllMessages();

            // Check for data
            if (CurrentLanePack.PackName == String.Empty)
            {
                MessageBox.Show(
                    "Lane Pack field cannot be blank before searching.",
                    "Blank Field",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

            // search for matching file
            string getPath = FileSearch.FindPDF(CurrentLanePack.PackName);



            // if it found a file
            if (getPath != String.Empty)
            {
                CurrentLanePack.PDFPath = getPath;
                VisMatchFound = Visibility.Visible;
            }
            // if nothing found
            else
                VisNoMatchFound = Visibility.Visible;

            // multiples found message
            if (FileSearch.FoundMultiple)
            {
                CurrentAlert.AlertText = "Multiple files were found matching this lane pack! \nThe first match has been selected: manual browse recommended.";
                CurrentAlert.AlertVis = Visibility.Visible;
            }
        }

        /// <summary>
        /// Button: Opens the Master PDF
        /// </summary>
        public void PreviewPDF()
        {
            OpenFile(MainVM.FileChoice.original);
        }

        /// <summary>
        /// Button: Opens the Dupe PDF
        /// </summary>
        public void OpenNewPDF()
        {
            OpenFile(MainVM.FileChoice.newlyCreated);
        }

        public void OnLoad()
        {
            this.NewData();
            this.CollapseAllMessages();
            this.CurrentAlert.SetAlert("Startup successful.", Visibility.Visible);
        }
        #endregion
    }
}
