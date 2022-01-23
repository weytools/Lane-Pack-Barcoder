using System.Windows;

namespace LanePackBarcodes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initialize and set up view model
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainVM();
            ((MainVM)DataContext).OnLoad();
        }

        #region Buttons
        private void GetPDFButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainVM)DataContext).RetrievePDF();
        }
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainVM)DataContext).BrowseForFile();
        }
        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainVM)DataContext).PreviewPDF();
        }
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainVM)DataContext).GeneratePDF();
        }
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainVM)DataContext).OpenNewPDF();
        }
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainVM)DataContext).NewData();
        }

        #endregion
        #region Local Functions
        // All moved to the View Model! Yay!
        #endregion

        /* DEBUGGING CODE. KEEPING FOR FUTURE REFERENCE.
        /// <summary>
        /// returns the converted enum from the combobox
        /// </summary>
        private MainVM.FieldToBarcode GetFieldEnum()
        {
            Enum.TryParse(((ComboBoxItem)writestyle.SelectedItem).Content.ToString(), out MainVM.FieldToBarcode comboField);
            return comboField;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainVM)DataContext).TestingTextBlock(GetFieldEnum());
        }
        */
    }

    }





        