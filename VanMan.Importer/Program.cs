using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VanMan.Importer
{
    class Program
    {
        private static string VanitiesFile = "vanities.txt";
        static void Main(string[] args)
        {
            if (File.Exists(VanitiesFile))
            {
                IEnumerable<string> vanities = File.ReadLines(VanitiesFile);
                using (FileStream resultsFs = new FileStream("VanitiesToUpload.csv", FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter resultsSw = new StreamWriter(resultsFs))
                    {
                        foreach (var vanity in vanities)
                        {
                            // Sample input line
                            // rollins.com,http://www.rollins.com
                            var splitVanity = vanity.Split(',');
                            if (splitVanity.Length == 2)
                            {
                                string url = splitVanity[0];
                                byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(url);
                                string options = "0"; // TODO: Parse the destination URL to determin options
                                resultsSw.WriteLine(string.Format(",{0},{1},{2}", Convert.ToBase64String(toEncodeAsBytes), splitVanity[1], options));
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine(string.Format("File {0} not found. Aborting.", VanitiesFile));
            }
        }
    }
}
