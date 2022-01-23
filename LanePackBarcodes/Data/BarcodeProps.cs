using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanePackBarcodes.MainVM;

namespace LanePackBarcodes
{
    class BarcodeProps
    {
        public FieldJustify Justify { get; set; }
        public int UpModifier { get; set; } = 2;
        public string SearchTerm { get; set; }


        // Constructor
        public BarcodeProps(FieldToBarcode field)
        {

            switch (field)
            {
                case FieldToBarcode.pid:
                    SearchTerm = "PID #:__________________";
                    Justify = FieldJustify.left;
                    break;
                case FieldToBarcode.lanePack:
                    SearchTerm = "Inspection & Packing Plan";
                    Justify = FieldJustify.center;
                    UpModifier = 1;
                    break;
                case FieldToBarcode.soa:
                    SearchTerm = "Applied to Order # SOA:__________________";
                    Justify = FieldJustify.right;
                    break;
                case FieldToBarcode.packDate:
                    SearchTerm = "DATE:____/____/________";
                    Justify = FieldJustify.left;
                    UpModifier = 1;
                    break;
                case FieldToBarcode.audit:
                    SearchTerm = "Audit";
                    Justify = FieldJustify.left;
                    break;
                default:
                    SearchTerm = "";
                    Justify = FieldJustify.left;
                    break;
            }

        }
    }
}
