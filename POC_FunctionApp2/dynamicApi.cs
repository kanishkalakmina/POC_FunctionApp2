using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using POC_FunctionApp2.models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;

namespace POC_FunctionApp2
{
    public static class DynamicApi
    {
        private static readonly string _cosmosDbConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private static readonly string _cosmosDbDatabaseName = "poc-functionapp1";
        private static readonly string _cosmosDbContainerName = "PocContainer1";

        private static readonly CosmosClient _cosmosClient = new CosmosClient(_cosmosDbConnectionString);
        private static readonly Container _cosmosContainer = _cosmosClient.GetContainer(_cosmosDbDatabaseName, _cosmosDbContainerName);

        [FunctionName("GetCustomFieldCollection")]
        public static async Task<IActionResult> GetCustomFieldCollection(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customfields/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation($"Getting Fields items with ID: {id}");

            var query = $"SELECT * FROM c WHERE c.id = '{id}'";
            var iterator = _cosmosContainer.GetItemQueryIterator<dynamic>(new QueryDefinition(query));
            if (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();
                if (results.Count > 0)
                {
                    return new OkObjectResult(results.First());
                }
            }
            return new NotFoundResult();
        }

        [FunctionName("CreateCustomFieldCollection")]
        public static async Task<IActionResult> CreateCustomFieldCollection(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customfields")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("creating dynamic item");
            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<CustomFieldCollection>(requestData);
            
            var item = new CustomFieldCollection
            {
                id = data.id,
                CustomField = new List<CustomFields>(),
            };

            foreach (var field in data.CustomField)
            {
                var newField = new CustomFields
                {
                    Name = field.Name,
                    DataType = field.DataType,
                    Order = field.Order,
                    Rule = field.Rule,
                };
                item.CustomField.Add(newField);
            }

            var result = await _cosmosContainer.CreateItemAsync(item);
            return new OkObjectResult(result.Resource);

        }

        [FunctionName("UpdateCustomFieldCollection")]
        public static async Task<IActionResult> UpdateCustomFieldCollection(
    [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "customfields/{id}")] HttpRequest req,
    ILogger log, string id)
        {
            log.LogInformation($"Updating Fields item with ID: {id}");

            try
            {
                // Parse the request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                CustomFieldCollection newCustomFieldCollection = JsonConvert.DeserializeObject<CustomFieldCollection>(requestBody);

                log.LogInformation($"Request body: {requestBody}");

                // Get the existing item from the Cosmos DB container
                var query = $"SELECT * FROM c WHERE c.id = '{id}'";
                var iterator = _cosmosContainer.GetItemQueryIterator<CustomFieldCollection>(new QueryDefinition(query));
                var results = await iterator.ReadNextAsync();
                var existingItem = results.FirstOrDefault();

                if (existingItem == null)
                {
                    log.LogInformation($"Item with ID {id} not found");
                    return new NotFoundResult();
                }

                existingItem.Update(newCustomFieldCollection);

                await _cosmosContainer.ReplaceItemAsync(existingItem, existingItem.id);

                log.LogInformation($"Item with ID {id} updated successfully");

                return new OkObjectResult(existingItem);
            }
            catch (Exception ex)
            {
                log.LogError($"Error updating item with ID {id}: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }




        [FunctionName("DeleteCustomFieldCollection")]
        public static async Task<IActionResult> DeleteCustomFieldCollection(
    [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "customfields/{id}")] HttpRequest req,
    ILogger log, string id)
        {
            log.LogInformation($"Deleting items with ID: {id}");

            //get items from cosmos
            var partitionKey = new PartitionKey(id);

            try
            {
                //query for all items with the given ID
                var query = $"SELECT * FROM c WHERE c.id = '{id}'";
                var iterator = _cosmosContainer.GetItemQueryIterator<dynamic>(new QueryDefinition(query));

                //delete each item one by one
                while (iterator.HasMoreResults)
                {
                    foreach (var item in await iterator.ReadNextAsync())
                    {
                        await _cosmosContainer.DeleteItemAsync<CustomFieldCollection>(item.id, partitionKey);
                    }
                }
                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogInformation($"DeleteCustomFieldCollection | Exception: {ex}");
                return new NotFoundResult();
            }
        }

    }
}
