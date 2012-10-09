using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using VanMan.Core;

namespace VanMan.Test
{
    class Program
    {
        enum VanityStatus
        {
            Pass,
            Fail,
            NoLocation,
            InvalidLocation,
            DomainNoResponse // Domain didn't get a response
        }

        static CloudTableClient GetTableClient()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=vanman;AccountKey=2fTP12iOtftSpCWXqOjJz9U2x4qLCVXrckuSQcRG4XPeuYCEDG5xfwe/AYi6QE9+OLu/2lxvCuyidJqhvFg4eQ==";

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = cloudStorageAccount.CreateCloudTableClient();

            cloudTableClient.Timeout = TimeSpan.FromMinutes(30);
            cloudTableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(3));

            return cloudTableClient;
        }

        static void Main(string[] args)
        {
            using (FileStream resultsFs = new FileStream("results.txt", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter resultsSw = new StreamWriter(resultsFs))
                {
                    var context = GetTableClient().GetDataServiceContext();
                    context.IgnoreResourceNotFoundException = true;

                    List<Vanity> vanities = null;
                    try
                    {
                        // Use AsTableServiceQuery() as a simple way to get all records, not just the first 1000
                        vanities = context.CreateQuery<Vanity>(Vanity.TableName).AsTableServiceQuery().ToList();

                        int total = 0, totalPass = 0, totalFail = 0, totalDomainNoResponse = 0, totalNoLocation = 0, totalInvalidLocation = 0;
                        VanityStatus result = VanityStatus.Fail;
                        
                        foreach (var vanity in vanities)
                        {
                            string domainIP = "69.20.10.146";
                            string domain = Encoding.UTF8.GetString(Convert.FromBase64String(vanity.RowKey));

                            HttpWebRequest request = WebRequest.Create(domain) as HttpWebRequest;
                            request.Proxy = WebRequest.DefaultWebProxy;
                            request.Timeout = 30000;
                            request.AllowAutoRedirect = false;

                            Uri domainUri = new Uri(domain);

                            HttpWebResponse response = null;

                            try
                            {
                                domainIP = Dns.GetHostAddresses(domainUri.Authority).FirstOrDefault().ToString();
                                response = (HttpWebResponse)request.GetResponse();
                            }
                            catch
                            {
                                totalDomainNoResponse++;
                                result = VanityStatus.DomainNoResponse;
                            }
                            finally
                            {
                                if (response != null)
                                {
                                    if (response.Headers.AllKeys.Contains("Location"))
                                    {
                                        try
                                        {
                                            string responseHeaderLocation = HttpUtility.UrlEncode(new Uri(response.Headers["Location"]).ToString().TrimEnd('/'));
                                            string vanityDestination = HttpUtility.UrlEncode(new Uri(vanity.Destination).ToString().TrimEnd('/'));
                                            if (responseHeaderLocation == vanityDestination)
                                            {
                                                result = VanityStatus.Pass;
                                                totalPass++;
                                            }
                                            else
                                            {
                                                result = VanityStatus.Fail;
                                                totalFail++;
                                            }
                                        }
                                        catch (UriFormatException ex)
                                        {
                                            result = VanityStatus.InvalidLocation;
                                            totalInvalidLocation++;
                                        }
                                    }
                                    else
                                    {
                                        result = VanityStatus.NoLocation;
                                        totalNoLocation++;
                                    }
                                    response.Close();
                                }
                            }
                            Console.Clear();
                            Console.Write("Total: {0}\nPass: {1}\nFail: {2}\nDomain No Response: {3}\nNo Location: {4}\nInvalid Location: {5}\nDomain: {6}\nDestination: {7}",
                                total++, totalPass, totalFail, totalDomainNoResponse, totalNoLocation, totalInvalidLocation, domain, vanity.Destination);

                            if (result != VanityStatus.Pass)
                                resultsSw.WriteLine("{0} {1} {2} {3}", domain, vanity.Destination, domainIP, result.ToString());
                            resultsSw.Flush();
                        }

                    }
                    catch (System.Data.Services.Client.DataServiceQueryException ex)
                    {
                        System.Diagnostics.Trace.WriteLine("Error retrieving vanities.");
                    }
                    Console.ReadKey();
                }
            }
        }
    }
}
