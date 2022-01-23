using LanePackBarcodes.Logic;
using System;
using System.ComponentModel;

/// <summary>
/// Stores the information about the searched Lane Pack
/// </summary>

namespace LanePackBarcodes
{
    public class LanePack : INotifyPropertyChanged
    {
        private string packName = String.Empty;
        private string pdfPath = "No file chosen.";
        private bool foundPDF = true;

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        // str PACK NAME
        public string PackName
        {
            get { return packName; }
            set
            {
                packName = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(PackName)));

            }
        }

        // str PDF PATH
        public string PDFPath
        {
            get { return pdfPath; }
            set
            {
                pdfPath = value;

                string newName = (FileName).Remove(FileName.Length - 4);
                PackName = newName;

                PropertyChanged(this, new PropertyChangedEventArgs(nameof(PDFPath)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(FileName)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(FoundPDF)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasMasterPath)));
            }
        }

        /// <summary>
        /// isolated file name for displaying on UI
        /// </summary>
        public string FileName
        {
            get { return System.IO.Path.GetFileName(PDFPath); }
        }

        // bool FOUND PDF?
        public bool FoundPDF
        {
            get { return DataCheck.IsPathValid(this.PDFPath); }
        }

        // post generated pdf path
        private string newPath = string.Empty;
        public string NewPath
        {
            get { return newPath; }

            set
            {
                newPath = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(NewPath)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasNewPath)));
            }
        }

        // is there a new pdf path?
        // used for setting the open file button
        public bool HasNewPath
        {
            get
            {
                if (NewPath != String.Empty)
                    return true;
                else
                    return false;
            }
        }


        public bool HasMasterPath
        {
            get
            {
                if (PDFPath != "No file chosen")
                    return true;
                else
                    return false;
            }
        }


    }
}
