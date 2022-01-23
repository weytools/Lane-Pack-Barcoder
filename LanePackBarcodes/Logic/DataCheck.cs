using System.Windows;

namespace LanePackBarcodes.Logic
{
    /// <summary>
    /// Helper functions for checking data values
    /// </summary>
    public class DataCheck
    {


        /// <summary>
        /// Checks if the path ends in .PDF
        /// </summary>
        /// <param name="path">full file path</param>
        /// <returns>true if it has .PDF, false if anything else</returns>
        public static bool IsPathValid(string path)
        {
            string normalizedPath = path.ToUpper();
            bool HasPDF = normalizedPath.Contains(".PDF");
            return HasPDF;
        }

        /// <summary>
        /// gets the visibility enum (collapsed or visible) based on if PDF was found
        /// </summary>
        /// <param name="isFound">pass in foundPDF from lanePack</param>
        /// <returns>int</returns>
        public static int GetVisibility(bool isFound)
        {
            if (!isFound)
            {
                return (int)Visibility.Collapsed;
            }
            else
            {
                return (int)Visibility.Visible;
            }
        }


    }
}
