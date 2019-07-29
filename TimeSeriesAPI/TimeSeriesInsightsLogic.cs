using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using TimeSeriesAPI.Models;

namespace TimeSeriesAPI
{
    public class TimeSeriesInsightsLogic
    {

        //To Aquire Token 
        private static async Task<string> AcquireAccessTokenAsync()
        {
            var tenant = ConfigurationManager.AppSettings["tenant"];

            var authenticationContext = new AuthenticationContext(
                $"https://login.microsoftonline.com/{tenant}",
                TokenCache.DefaultShared);

            AuthenticationResult token = await authenticationContext.AcquireTokenAsync(
                // Set the resource URI to the Azure Time Series Insights API
                resource: "https://api.timeseries.azure.com/",
                clientCredential: new ClientCredential(
                    // Application ID of application registered in Azure Active Directory
                    clientId: ConfigurationManager.AppSettings["clientId"],
                    // Application key of the application that's registered in Azure Active Directory
                    clientSecret: ConfigurationManager.AppSettings["clientSecret"]));

            string accessToken = token.AccessToken;

            return accessToken;
        }

        //To get first EnviornmentId
        private static async Task<string> AquireEnvironmentFqdn()
        {
            string accessToken = await AcquireAccessTokenAsync();
            Console.WriteLine(accessToken);
            string environmentFqdn;
            {
                HttpWebRequest request = CreateHttpsWebRequest("api.timeseries.azure.com", "GET", "environments", accessToken);
                JToken responseContent = await GetResponseAsync(request);

                JArray environmentsList = (JArray)responseContent["environments"];
                if (environmentsList.Count == 0)
                {
                    // List of user environments is empty, fallback to sample environment.
                    environmentFqdn = "10000000-0000-0000-0000-100000000108.env.timeseries.azure.com";
                }
                else
                {
                    // Assume the first environment is the environment of interest.
                    JObject firstEnvironment = (JObject)environmentsList[0];
                    environmentFqdn = firstEnvironment["environmentFqdn"].Value<string>();
                }
            }
            return environmentFqdn;
        }

        //To get EnviornmentList
        public static async Task<IEnumerable<EnvironmentListResponse>> GetEnvironmentList()
        {
            string accessToken = await AcquireAccessTokenAsync();
            Console.WriteLine(accessToken);

            List<EnvironmentListResponse> lstEnvironment = new List<EnvironmentListResponse>();
            HttpWebRequest request = CreateHttpsWebRequest("api.timeseries.azure.com", "GET", "environments", accessToken);
            JToken responseContent = await GetResponseAsync(request);

            JArray environmentsList = (JArray)responseContent["environments"];
            if (environmentsList.Count == 0)
            {
                // List of user environments is empty, fallback to sample environment.
                string environmentFqdn = "10000000-0000-0000-0000-100000000108.env.timeseries.azure.com";
                lstEnvironment.Add(new EnvironmentListResponse("Default", environmentFqdn));
            }
            else
            {
                // Assume the first environment is the environment of interest.
                int eventCount = environmentsList.Count;

                for (int i = 0; i < eventCount; i++)
                {
                    JObject ItemEnvironment = (JObject)environmentsList[i];
                    lstEnvironment.Add(new EnvironmentListResponse(ItemEnvironment["displayName"].Value<string>(), ItemEnvironment["environmentFqdn"].Value<string>()));
                }


            }

            return lstEnvironment.ToArray();
        }
        //To get Availability of first Environment
        public static async Task<Dictionary<string, DateTime>> SampleAsync()
        {
            string accessToken = await AcquireAccessTokenAsync();
            string environmentFqdn = await AquireEnvironmentFqdn();

            DateTime fromAvailabilityTimestamp;
            DateTime toAvailabilityTimestamp;
            {
                HttpWebRequest request = CreateHttpsWebRequest(environmentFqdn, "GET", "availability", accessToken);
                JToken responseContent = await GetResponseAsync(request);

                JObject range = (JObject)responseContent["range"];
                fromAvailabilityTimestamp = range["from"].Value<DateTime>();
                toAvailabilityTimestamp = range["to"].Value<DateTime>();
            }
            Dictionary<string, DateTime> Availability = new Dictionary<string, DateTime>();
            Availability.Add("from", fromAvailabilityTimestamp);
            Availability.Add("to", toAvailabilityTimestamp);

            return Availability;
        }

        //To get Availability of Selected Environment
        public static async Task<Dictionary<string, DateTime>> GetAvailability(string EnvironmentId)
        {
            string accessToken = await AcquireAccessTokenAsync();
            string environmentFqdn = EnvironmentId;
            //IEnumerable<EnvironmentListResponse> environmentLists = await GetEnvironmentList();
            //List<EnvironmentListResponse> lstEnvironment = new List<EnvironmentListResponse>();
            //for (int i = 0; i < environmentLists.Count(); i++)
            //{
            //    lstEnvironment.Add(environmentLists.ElementAt(i));
            //    if (lstEnvironment[i].displayName == EnvironmentId)
            //        environmentFqdn = lstEnvironment[i].EnviornmentId;



            //}

            DateTime fromAvailabilityTimestamp;
            DateTime toAvailabilityTimestamp;
            {
                HttpWebRequest request = CreateHttpsWebRequest(environmentFqdn, "GET", "availability", accessToken);
                JToken responseContent = await GetResponseAsync(request);

                JObject range = (JObject)responseContent["range"];
                fromAvailabilityTimestamp = range["from"].Value<DateTime>();
                toAvailabilityTimestamp = range["to"].Value<DateTime>();
            }
            Dictionary<string, DateTime> Availability = new Dictionary<string, DateTime>();
            Availability.Add("from", fromAvailabilityTimestamp);
            Availability.Add("to", toAvailabilityTimestamp);

            return Availability;
            //return environmentFqdn;
        }


        //To get Metadata
        public static async Task<IEnumerable<MetaDataPropertiesResponse>> GetMetaData(string EnvironmentId, DateTime from, DateTime to)
        {
            string accessToken = await AcquireAccessTokenAsync();
            string environmentFqdn = EnvironmentId;
            //IEnumerable<EnvironmentListResponse> environmentLists = await GetEnvironmentList();
            //List<EnvironmentListResponse> lstEnvironment = new List<EnvironmentListResponse>();
            //for (int i = 0; i < environmentLists.Count(); i++)
            //{
            //    lstEnvironment.Add(environmentLists.ElementAt(i));
            //    if (lstEnvironment[i].displayName == EnvironmentId)
            //        environmentFqdn = lstEnvironment[i].EnviornmentId;



            //}
            JObject inputPayload = new JObject(
                    new JProperty("searchSpan", new JObject(
                        new JProperty("from", from),
                        new JProperty("to", to))));

            HttpWebRequest request = CreateHttpsWebRequest(environmentFqdn, "POST", "metadata", accessToken);
            await WriteRequestStreamAsync(request, inputPayload);
            JToken responseContent = await GetResponseAsync(request);
            JArray PropertiesArray = (JArray)responseContent["properties"];
            List<MetaDataPropertiesResponse> ResultData = new List<MetaDataPropertiesResponse>();
            int eventCount = PropertiesArray.Count;
            for (int i = 0; i < eventCount; i++)
            {
                JObject currentEvent = (JObject)PropertiesArray[i];
                string values = currentEvent["name"].ToString();
                ResultData.Add(new MetaDataPropertiesResponse(values));
            }
            return ResultData;
        }


        //To get EvenData  
        public static async Task<IEnumerable<TimeSeriesResponse>> getEventData(string EnvironmentId, DateTime from, DateTime to, string TagName)
        {
            string accessToken = await AcquireAccessTokenAsync();
            string environmentFqdn = EnvironmentId;
            Dictionary<string, DateTime> Availability = new Dictionary<string, DateTime>();
            //Availability = await SampleAsync();
            //from =Availability["from"];
            //to = Availability["to"];
            string fromDate, Todate;
            fromDate = DateTime.Now.ToString("dd/mm/yyyy");
            Todate = DateTime.Now.ToString("dd/mm/yyyy");


            JObject contentInputPayload = new JObject(

                    new JProperty("searchSpan", new JObject(
                        new JProperty("from", from.ToUniversalTime()),
                        new JProperty("to", to.ToUniversalTime()))),

                    new JProperty("top", new JObject(
                    new JProperty("sort", new JArray(
                        new JObject(new JProperty("input", new JObject(new JProperty("builtInProperty", "$ts"))),
                                    new JProperty("order", "asc")))),
                    new JProperty("count", 10))));

            //List<string> CompData = new List<string>();

            List<TimeSeriesResponse> ResultData = new List<TimeSeriesResponse>();
            HttpWebRequest request = CreateHttpsWebRequest(environmentFqdn, "POST", "events", accessToken);
            await WriteRequestStreamAsync(request, contentInputPayload);
            JToken responseContent = await GetResponseAsync(request);
            JArray events = (JArray)responseContent["events"];
            int eventCount = events.Count;
            int? Requiredvalueindex = null; // For telemetry data
            for (int i = 0; i < eventCount; i++)
            {
                JObject currentEvent = (JObject)events[i];

                JArray values = (JArray)currentEvent["values"];
                //For telemetry data start
                if (i == 0)
                {
                    JObject schema = (JObject)currentEvent["schema"];
                    JArray properties = (JArray)schema["properties"];
                    int j = 0;
                    foreach (JObject content in properties.Children<JObject>())
                    {

                        foreach (JProperty prop in content.Properties())
                        {
                            if (prop.Value.ToString() == TagName)
                            {
                                Requiredvalueindex = j;
                                // string tempvalue = prop.value.tostring();
                                //    compdata.add(tempvalue);
                            }

                        }
                        j++;
                    }


                }
                if (Requiredvalueindex != null)
                    ResultData.Add(new TimeSeriesResponse(currentEvent["$ts"].ToString(), values[Requiredvalueindex].ToString()));
                // for telemetry data end

                if (TagName == values[1].ToString())
                {
                    ResultData.Add(new TimeSeriesResponse(currentEvent["$ts"].ToString(), values[2].ToString()));
                }

            }


            return ResultData.Take(10);
        }
        private static async Task WriteRequestStreamAsync(HttpWebRequest request, JObject inputPayload)
        {
            using (var stream = await request.GetRequestStreamAsync())
            using (var streamWriter = new StreamWriter(stream))
            {
                await streamWriter.WriteAsync(inputPayload.ToString());
                await streamWriter.FlushAsync();
                streamWriter.Close();
            }
        }
        private static HttpWebRequest CreateHttpsWebRequest(string host, string method, string path, string accessToken, string[] queryArgs = null)
        {
            string query = "api-version=2016-12-12";
            if (queryArgs != null && queryArgs.Any())
            {
                query += "&" + String.Join("&", queryArgs);
            }

            Uri uri = new UriBuilder("https", host)
            {
                Path = path,
                Query = query
            }.Uri;
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.Method = method;
            request.Headers.Add("x-ms-client-application-name", "TimeSeriesInsightsQuerySample");
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            return request;
        }
        private static async Task<JToken> GetResponseAsync(HttpWebRequest request)
        {
            using (WebResponse webResponse = await request.GetResponseAsync())
            using (var sr = new StreamReader(webResponse.GetResponseStream()))
            {
                string result = await sr.ReadToEndAsync();
                return JsonConvert.DeserializeObject<JToken>(result);
            }
        }
    }
}