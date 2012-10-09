using System;
using System.Data.Services.Client;
using System.Diagnostics;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using VanMan.Core;

namespace VanMan.WebApplication
{
    public class Global : System.Web.HttpApplication
    {

        public static CloudTableClient GetTableClient()
        {
            // Code that runs on application startup
            var storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client
            CloudTableClient result = storageAccount.CreateCloudTableClient();

            return result;
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            // Create the table if it doesn't exist
            var client = GetTableClient();
            if (client.CreateTableIfNotExist(Vanity.TableName))
            {
                //
                // Must do this voodoo: http://deeperdesign.wordpress.com/2010/03/10/azure-table-storage-what-a-pain-in-the-ass/

                Vanity temp = new Vanity(Guid.NewGuid().ToString())
                {
                    Destination = CloudConfigurationManager.GetSetting("DefaultDestination") ?? "https://github.com/jamestharpe/VanMan",
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
            Trace.WriteLine("Session Start");
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            Trace.WriteLine("Application_BeginRequest");

            var context = GetTableClient().GetDataServiceContext();
            context.IgnoreResourceNotFoundException = true;

            var uri = Request.Url;
            Vanity vanity = null;

            do
            {
                vanity = Utils.GetVanityFromUri(context, uri);

                var unChoppedUri = new Uri(uri.ToString());
                uri = Utils.Chop(uri);

                if (uri == unChoppedUri)
                    break;
            }
            while (vanity == null);

            if (vanity == null) // Default
            {
                vanity = new Vanity(uri.ToString())
                {
                    Destination = CloudConfigurationManager.GetSetting("DefaultDestination") ?? "https://github.com/jamestharpe/VanMan",
                    Options = (int)RedirectOptions.Default,
                    PartitionKey = string.Empty
                };

                //Task.Run(() =>
                {
                    context.AddObject(Vanity.TableName, vanity);
                    context.SaveChanges();
                    //Trace.WriteLine(string.Format("Added record for {0} ({1})", uri, rowKey));
                }//);
            }

            var destination = (((vanity.GetOptions() & RedirectOptions.PreservePath) == RedirectOptions.PreservePath)
                ? vanity.Destination + Request.Url.AbsolutePath
                : vanity.Destination);

            if ((vanity.GetOptions() & RedirectOptions.PreserveQueryString) == RedirectOptions.PreserveQueryString)
                destination += Request.Url.Query;

            if ((vanity.GetOptions() & RedirectOptions.Permanent) == RedirectOptions.Permanent)
            {
                Trace.WriteLine(string.Format("301 Redirecting from {0} to {1}", uri, destination));
                Response.RedirectPermanent(destination, true);
            }
            else
            {
                Trace.WriteLine(string.Format("302 Redirecting from {0} to {1}", uri, destination));
                Response.Redirect(destination, true);
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            Trace.WriteLine("Application_AuthenticateRequest");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            Trace.WriteLine(string.Format("Application_Error Message: {0} Stack: {1}", ex.Message, ex.StackTrace));
        }

        protected void Session_End(object sender, EventArgs e)
        {
            Trace.WriteLine("Session_End");
        }

        protected void Application_End(object sender, EventArgs e)
        {
            Trace.WriteLine("Application_End");
        }
    }
}