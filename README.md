# Lane Pack Barcoder
## Usage
- Users select a *Lane Pack Number* either by partial typing, or scanning a barcode.  
  - **Retrieve PDF** finds the original lane pack template on the internal file system. 
- *Lane Pack PDF* gives positive feedback to the user on which template has been found.
  - The user can **Preview** the PDF to verify the correct file has been chosen. 
  - > *Note: Error messages at the top will indicate if more than 1 matching template has been found - typically for a partial search.*
- *PID Number* and *SOA Number* are user entered data that will be encoded onto the final PDF. Typically entered using a barcode scanner.
- **Generate PDF** will create the new file enabling the **Open new PDF** button.
  - **Open new PDF** opens the file to allow for printing.  

## POI
- Window UI along with target bindings is defined in [/MainWindow.xaml](./LanePackBarcodes/MainWindow.xaml)
  - Binding logic is in the codebehind, in [/MainWindow.xaml](./LanePackBarcodes/MainWindow.xaml.cs)
- The context of each new PDF is in the [/MainVM.cs](./LanePackBarcodes/ViewModels/MainVM.cs) ViewModel. This class contains the bulk of the application logic.
- Data & helper functions in [/Data/](./LanePackBarcodes/Data/) & [/Logic/](./LanePackBarcodes/Logic/)
