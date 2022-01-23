using System;
using System.IO;

namespace LanePackBarcodes.Logic
{
    /// <summary>
    /// Helper functions for checking data values
    /// </summary>
    public class FileSearch
    {
        public static string networkPath = "{redacted}";

        /// <summary>
        /// Searches the LanePacks folder for the specified lane pack name using wildcards on either side
        /// </summary>
        /// <param name="lanePackName">The lane pack name</param>
        /// <returns>The filepath of the first found matching. Check for FoundMultiple!=False to see if multiples</returns>
        public static string FindPDF(string lanePackName)
        {
            string[] files = Directory.GetFiles(networkPath, $"*{lanePackName}*");
            if (files.Length > 1)
            {
                FileSearch.FoundMultiple = true;
                return files[0];
            }
            else if (files.Length == 1)
            {
                FileSearch.FoundMultiple = false;
                return files[0];
            }
            else
            { 
            FileSearch.FoundMultiple = false;
            return String.Empty;
            }
        }

        public static bool FoundMultiple { get; set; } = false;



    }
}
