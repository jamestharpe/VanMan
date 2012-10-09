using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.WindowsAzure.StorageClient;

namespace VanMan.Core
{
    public static class Utils
    {

        public static Uri Chop(Uri uri)
        {
            var delims = new char[] { '&', '?', '/' };
            var schemeAndDelim = uri.Scheme + Uri.SchemeDelimiter; // e.g. 'http://'
            var result = uri.ToString();

            foreach (var delim in delims)
            {
                var delimPos = result.LastIndexOf(delim);
                if (delimPos > schemeAndDelim.Length)
                {
                    result = result.Substring(0, delimPos);
                    break;
                }
            }

            return new Uri(result);
        }

        public static Vanity GetVanityFromUri(TableServiceContext context, Uri uri)
        {
            var rowKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(uri.ToString().ToLower()));

            Trace.WriteLine(string.Format("Handling {0} ({1})", uri, rowKey));

            Vanity result = null;
            try
            {
                result = context.CreateQuery<Vanity>(Vanity.TableName)
                    .Where(v =>
                        v.PartitionKey == string.Empty
                     && v.RowKey == rowKey)
                    .FirstOrDefault();
            }
            catch (System.Data.Services.Client.DataServiceQueryException ex)
            {
                Trace.WriteLine(string.Format("Url {0} Base64Ecoded {1} is invalid. {2}", uri.ToString(), rowKey, ex.Message));
            }

            if (result != null)
            {
                Trace.WriteLine(string.Format("Uri: {0} Vanity.RowKey: {1} Vanity.Destation: {2} Vanity.Options: {3}",
                    uri.ToString(),
                    rowKey,
                    result.Destination,
                    result.Options));
            }
            return result;
        }
    }
}