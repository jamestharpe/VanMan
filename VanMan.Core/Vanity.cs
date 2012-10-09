using System;
using Microsoft.WindowsAzure.StorageClient;

namespace VanMan.Core
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
