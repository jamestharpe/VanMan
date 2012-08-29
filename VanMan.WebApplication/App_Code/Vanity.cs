using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace VanMan.WebApplication.App_Code
{
    public class Vanity : TableServiceEntity
    {
        public const string TableName = "Vanities";

        public Vanity(string source)
        {
            SetSource(source);
        }

        public RedirectOptions Options { get; set; }

        // PartitionKey = ""

        public string Destination { get; set; }


        public string GetSource()
        {
            return HttpUtility.UrlDecode(RowKey);
        }

        public string SetSource(string value)
        {
            RowKey = HttpUtility.UrlEncode(RowKey);
            return RowKey;
        }
    }
}
