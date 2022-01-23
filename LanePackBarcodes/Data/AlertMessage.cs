using System;
using System.ComponentModel;
using System.Windows;

namespace LanePackBarcodes
{
    public class AlertMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        private Visibility alertVis = Visibility.Visible;
        private string alertText = "Startup success.";
        public Visibility AlertVis
        {
            get { return alertVis; }
            set
            {
                alertVis = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(AlertVis)));

            }
        }

        public string AlertText
        {
            get { return alertText; }
            set
            {
                alertText = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(AlertText)));
            }
        }

        public void TurnOffAlert()
        {
            AlertText = String.Empty;
            AlertVis = Visibility.Hidden;
        }

        public void SetAlert(string message, Visibility visibility)
        {
            AlertText = message;
            AlertVis = visibility;
        }
    }
}
