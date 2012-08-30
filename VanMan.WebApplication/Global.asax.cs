using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using VanMan.WebApplication.App_Code;

namespace VanMan.WebApplication
{
    public class Global : System.Web.HttpApplication
    {
        CloudTableClient GetTableClient()
        {
            // Code that runs on application startup
            var storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client
            CloudTableClient result = storageAccount.CreateCloudTableClient();

            return result;
        }

        Uri Chop(Uri uri)
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


        protected void Application_Start(object sender, EventArgs e)
        {
            // Create the table if it doesn't exist
            var client = GetTableClient();
            if (client.CreateTableIfNotExist(Vanity.TableName))
            {
                //
                // Must do this voodoo: http://deeperdesign.wordpress.com/2010/03/10/azure-table-storage-what-a-pain-in-the-ass/

                Vanity temp = new Vanity("http://www.rollins.com")
                {
                    Destination = "http://www.rollins.com",
                    Options = (int)RedirectOptions.Default,
                    PartitionKey = string.Empty
                };
                var context = client.GetDataServiceContext();
                context.AddObject(Vanity.TableName, temp);
                context.SaveChanges();
                context.DeleteObject(temp);
                context.SaveChanges(SaveChangesOptions.ContinueOnError);
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var context = GetTableClient().GetDataServiceContext();
            context.IgnoreResourceNotFoundException = true;

            var uri = Request.Url;

            var rowKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(uri.ToString()));

            Vanity vanity = context.CreateQuery<Vanity>(Vanity.TableName)
                .Where(v =>
                    v.PartitionKey == string.Empty
                 && v.RowKey == rowKey)
                .FirstOrDefault();

            while (vanity == null)
            {
                var unChoppedUri = new Uri(uri.ToString());
                uri = Chop(uri);

                if (uri == unChoppedUri)
                    break;

                vanity = context.CreateQuery<Vanity>(Vanity.TableName)
                    .Where(v =>
                        v.PartitionKey == string.Empty
                     && v.RowKey == Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(uri.ToString().ToLower())))
                    .FirstOrDefault();
            }

            if (vanity == null) // Default
            {
                vanity = new Vanity(uri.ToString())
                {
                    Destination = "http://www.rollins.com",
                    Options = (int)RedirectOptions.Default,
                    PartitionKey = string.Empty
                };
                context.AddObject(Vanity.TableName, vanity);
                context.SaveChanges();
            }

            var destination = (((vanity.GetOptions() & RedirectOptions.PreservePath) == RedirectOptions.PreservePath)
                ? vanity.Destination + Request.Url.AbsolutePath
                : vanity.Destination);

            if ((vanity.GetOptions() & RedirectOptions.PreserveQueryString) == RedirectOptions.PreserveQueryString)
                destination += Request.Url.Query;

            if ((vanity.GetOptions() & RedirectOptions.Permanent) == RedirectOptions.Permanent)
                Response.RedirectPermanent(destination, true);
            else
                Response.Redirect(destination, true);
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}