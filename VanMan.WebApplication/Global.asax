<%@ Application Language="C#" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Data.Linq" %>
<%@ Import Namespace="System.Data.Services.Client" %>
<%@ Import Namespace="Microsoft.WindowsAzure" %>
<%@ Import Namespace="Microsoft.WindowsAzure.StorageClient" %>
<%@ Import Namespace="VanMan.WebApplication.App_Code" %>

<script runat="server">

    CloudTableClient GetTableClient()
    {
        // Code that runs on application startup
        var storageAccount = CloudStorageAccount.Parse(
            CloudConfigurationManager.GetSetting("StorageConnectionString"));

        // Create the table client
        CloudTableClient result = storageAccount.CreateCloudTableClient();

        return result;
    }

    void Application_Start(object sender, EventArgs e) 
    {
        // Create the table if it doesn't exist
        GetTableClient().CreateTableIfNotExist(Vanity.TableName);
    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown

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

    void Application_BeginRequest(object sender, EventArgs e) 
    {
        var context = GetTableClient().GetDataServiceContext();

        var uri = Request.Url;
        
        Vanity vanity = context.CreateQuery<Vanity>(Vanity.TableName)
            .Where(e => 
                e.PartitionKey == string.Empty 
             && e.RowKey == HttpUtility.UrlEncode(uri.ToString().ToLower()))
            .FirstOrDefault();

        while (vanity == null)
        {
            var unChoppedUri = new Uri(uri.ToString());
            uri = Chop(uri);

            if (uri == unChoppedUri)
                break;
            
            vanity = context.CreateQuery<Vanity>(Vanity.TableName)
                .Where(e =>
                    e.PartitionKey == string.Empty
                 && e.RowKey == HttpUtility.UrlEncode(uri.ToString().ToLower()))
                .FirstOrDefault();
        }

        if (vanity == null) // Default
            vanity = new Vanity(uri.ToString())
            {
                Destination = "http://www.rollins.com",
                Options = RedirectOptions.Default,
                PartitionKey = string.Empty
            };

        var destination = (((vanity.Options & RedirectOptions.PreservePath) == RedirectOptions.PreservePath)
            ? vanity.Destination + Request.Url.AbsolutePath
            : vanity.Destination);
        
        if ((vanity.Options & RedirectOptions.PreserveQueryString) == RedirectOptions.PreserveQueryString)
            destination += Request.Url.Query;

        if ((vanity.Options & RedirectOptions.Permanent) == RedirectOptions.Permanent)
            Response.RedirectPermanent(vanity.Destination, true);
        else
            Response.Redirect(vanity.Destination, true);
        
    }

    void Session_Start(object sender, EventArgs e) 
    {
        // Code that runs when a new session is started

    }

    void Session_End(object sender, EventArgs e) 
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }
       
</script>
