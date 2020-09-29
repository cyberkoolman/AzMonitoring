using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ResourceHealthAlertPOC.Models;
using ResourceHealthAlertPOC.Util;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using ActivityAlertPOC.Models;

namespace ResourceHealthAlertPOC
{
    public static class ProcessAlerts
    {
        private static CosmosClient client = new CosmosClient(GetEnvironmentVariable("CosmosDb_Uri"), GetEnvironmentVariable("CosmosDb_Key"));
        [FunctionName("ProcessAlerts")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log
            //,[Queue("retryresources", Connection = "AzureWebJobsStorage")] ICollector<ResourceHealthDto> retryQueue
            )
        {

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation("requestBodyis");
            log.LogInformation(requestBody);

            //Process ResourceHealth Alert
            if(requestBody.Contains("Resource Health"))
            {
                var resourceHealthAlert = JsonConvert.DeserializeObject<ResourceHealthAlert>(requestBody);
                log.LogInformation(resourceHealthAlert.schemaId);
                var alertObj = CosmosHelper.GetResourceHealthDtoMapping(resourceHealthAlert);
                var alertHistoryObj = CosmosHelper.GetDtoHistoryMapping(resourceHealthAlert);

                log.LogInformation("Resource Health Alert Id: " + alertObj.alertId);
                log.LogInformation("Resource Subscription: " + alertObj.subscriptionId);
                log.LogInformation("Resource Id: " + alertObj.resourceId);
                log.LogInformation("Resource Status: " + alertObj.currentHealthStatus);

                try
                {
                    List<string> subs = new List<string>
                    {
                        alertObj.subscriptionId
                    };
                    string token = AuthHelper.GetTokenAsync().Result;
                    log.LogInformation($"Token Received: {token}");

                    /*  Below is to do the quick testing for resolution of access token
                    string linkId = "subscriptions/a7f5830b-4c53-4a55-a641-bc07ef502ab2/resourcegroups/rp-gmsa-rg/providers/microsoft.compute/virtualmachines/rpwinapp01";
                    string resourceGraphUri = $"https://management.azure.com/{linkId}?api-version=2016-09-01";
                    ResourceGraphResponse resourcesGraph = await ResilientRestClient.GetAsync<ResourceGraphResponse>(resourceGraphUri, token);
                    */

                    string resourceGraphUri = "https://management.azure.com/providers/Microsoft.ResourceGraph/resources?api-version=2019-04-01";
                    string query = $"where id =~ '{alertObj.resourceId}' | project location";
                    Dictionary<string, int> options = new Dictionary<string, int>();
                    options["$skip"] = 0;
                    Dictionary<string, object> resourceObj = new Dictionary<string, object>();

                    resourceObj.Add("subscriptions", subs);
                    resourceObj.Add("query", query);
                    resourceObj.Add("options", options);

                    ResourceGraphResponse resourcesGraph = ResilientRestClient.PostAsync<ResourceGraphResponse>(resourceGraphUri, token, resourceObj).Result;
                    alertObj.location = resourcesGraph.data.rows[0][0].ToString();
                    alertHistoryObj.location = resourcesGraph.data.rows[0][0].ToString();
                }
                catch (System.Exception)
                {
                    alertObj.location = "NA";
                    alertHistoryObj.location = "NA";
                    log.LogError($"Unable to get location for {alertObj.resourceId}.  Set default for NA location");
                }

                var collectionId = GetEnvironmentVariable("CosmosDb_Collection");
                var databaseId = GetEnvironmentVariable("CosmosDb_Database");
                var collectionHistoryId = GetEnvironmentVariable("CosmosDb_HistoryCollection");

                try
                {
                    ItemResponse<ResourceHealthDto> response = await client.GetContainer(databaseId, collectionId).UpsertItemAsync(alertObj, new PartitionKey(alertObj.resourceId));
                    log.LogInformation("Document created in Cosmos: " + response.StatusCode);
                }
                catch(CosmosException ex)    
                {
                    log.LogInformation("error created in Cosmos: " + ex.Message);
                    log.LogInformation("Add Object to Retry Queue: " + ex.Message);
                   // retryQueue.Add(alertObj);  
                }                

                try
                {
                    ItemResponse<ResourceHealthDto> responseHistory = await client.GetContainer(databaseId, collectionHistoryId).CreateItemAsync(alertHistoryObj, new PartitionKey(alertHistoryObj.resourceId));
                    log.LogInformation("Document created in Cosmos History: " + responseHistory.StatusCode);
                }
                catch(CosmosException ex)    
                {
                    log.LogInformation("error created in Cosmos: " + ex.Message);
                    //retryQueue.Add(alertHistoryObj);  
                }    
            }
            else
            {
                var activitylog = JsonConvert.DeserializeObject<ActivityLogAlert>(requestBody);
                log.LogInformation(activitylog.schemaId);                

                var alertObj = CosmosHelper.GetActivityLogAlertDtoMapping(activitylog,log);

                try
                {
                    List<string> subs = new List<string>
                    {
                        alertObj.subscriptionId
                    };
                    string token = AuthHelper.GetTokenAsync().Result;
                    log.LogInformation($"Token Received: {token}");
                    string resourceGraphUri = "https://management.azure.com/providers/Microsoft.ResourceGraph/resources?api-version=2018-09-01-preview";
                    string query = $"where id =~ '{alertObj.resourceId}' | project location, properties.hardwareProfile.vmSize, sku.name ";
                    //, properties.hardwareProfile.vmSize,sku.name";
                    Dictionary<string, int> options = new Dictionary<string, int>();
                    options["$skip"] = 0;
                    Dictionary<string, object> resourceObj = new Dictionary<string, object>();

                    resourceObj.Add("subscriptions", subs);
                    resourceObj.Add("query", query);
                    resourceObj.Add("options", options);

                    ResourceGraphResponse resourcesGraph = ResilientRestClient.PostAsync<ResourceGraphResponse>(resourceGraphUri, token, resourceObj).Result;
                    
                    alertObj.location = resourcesGraph.data.rows[0][0].ToString();
                    
                    if (!(String.IsNullOrEmpty(resourcesGraph.data.rows[0][1])))
                    {
                       alertObj.size = resourcesGraph.data.rows[0][1].ToString();
                    }
                    else
                    {
                        alertObj.size = String.IsNullOrEmpty(resourcesGraph.data.rows[0][2])? "" :resourcesGraph.data.rows[0][2].ToString();
                    }
                }
                catch (System.Exception ex)
                {

                    log.LogError($"error {ex.Message}");

                    alertObj.location = "N/A";
                    alertObj.size = "N/A";
                    log.LogError($"Unable to get location for {alertObj.resourceId}");

                    log.LogInformation($"Unable to get location for {alertObj.resourceId}");
                    log.LogInformation($"error {ex.Message}");
                }
                log.LogInformation("Resource Subscription: " + alertObj.subscriptionId);
                log.LogInformation("Resource Id: " + alertObj.resourceId);
                log.LogInformation("Resource Status: " + alertObj.alertStatus);
                var collectionId = GetEnvironmentVariable("CosmosDb_Activitylogs");
                var databaseId = GetEnvironmentVariable("CosmosDb_Database");

                try
                {
                    ItemResponse<ActivityLogDto> response = await client.GetContainer(databaseId, collectionId).UpsertItemAsync(alertObj, new PartitionKey(alertObj.id));
                    log.LogInformation("Document created in Cosmos: " + response.StatusCode);
                }
                catch(CosmosException ex)    
                {
                    log.LogInformation("error created in Cosmos: " + ex.Message);
                }
            }

            return new OkObjectResult("Resource Health Alert Processed");
        }


        public static string GetEnvironmentVariable(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName);
        }
    }
}
