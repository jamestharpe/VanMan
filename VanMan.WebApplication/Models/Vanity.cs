using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Data.Linq;

namespace VanMan.WebApplication.App_Code
{
    public class Vanity : TableServiceEntity
    {
        public const string TableName = "Vanities";

        public Vanity()
        {
        }


        public Vanity(string source)
        {
            SetSource(source);
        }

        public int Options { get; set; }

        // PartitionKey = ""

        public string Destination { get; set; }


        public string GetSource()
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(RowKey));
        }

        public string SetSource(string value)
        {
            RowKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));
            return RowKey;
        }

        public RedirectOptions GetOptions()
        {
            return (RedirectOptions)Options;
        }

        public void SetOptions(RedirectOptions options)
        {
            Options = (int)options;
        }
    }
}
