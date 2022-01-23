using System;
using System.ComponentModel;

/// <summary>
/// Stores the job information for the current PID
/// </summary>

namespace LanePackBarcodes.Data
{
    public class JobInfo : INotifyPropertyChanged
    {
        private string pidNumber = String.Empty;
        private string soaNumber = String.Empty;
        private int quantity = 0;

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        //str PID NUMBER
        public string PIDNumber
        {
            get { return pidNumber; }
            set
            {
                pidNumber = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(PIDNumber)));
            }
        }
        //str SOA NUMBER
        public string SOANumber
        {
            get { return soaNumber; }
            set
            {
                soaNumber = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(SOANumber)));
            }
        }

        //int QUANTITY
        public int Quantity
        {
            get { return quantity; }
            set
            {
                quantity = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Quantity)));
            }

        }
    }
}
