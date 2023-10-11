using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PoC.BlobTriggerDurable.Failover.Functions
{
    public static class Starter
    {
        [FunctionName(nameof(BlobStart))]
        public static async Task BlobStart(
            [BlobTrigger("input/{name}", Connection = "AzureWebJobsStorage", Source = BlobTriggerSource.LogsAndContainerScan)] Stream myBlob,
            string name,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            //Read each line in myBlob into a list
            var lines = new List<string>();
            using (var reader = new StreamReader(myBlob))
            {
                while (reader.Peek() >= 0)
                {
                    lines.Add(await reader.ReadLineAsync());
                }
            }

            //It is possible that the same blob triggers multiple instances of the same orchestrator; once in each region.
            //This can happen using active-active configuration, or if the secondary region is passive and becomes
            //active without the blob being deleted when processed.
            //If we can derive the instance ID deterministically, we can avoid starting the same instance twice.
            //https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-singletons?tabs=csharp

            string instanceId = GenerateInstanceId(name, lines, log);
            var existingInstance = await starter.GetStatusAsync(instanceId);

            //To prevent completed instances being processed again, this line was removed from the if statement:
            //|| existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Completed

            if (existingInstance == null
            || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Failed
            || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Terminated)
            {
                await starter.StartNewAsync(nameof(Orchestrator.RunOrchestrator), instanceId, new BlobContent { Filename = name, Lines = lines });

                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            }
            else log.LogWarning($"Duplicate orchstration detected with ID '{instanceId}'.");
        }

        private static string GenerateInstanceId(string name, List<string> lines, ILogger logger)
        {
            //lines[0] contains a GUID, which is unique per blob (Created by PoC.BlobTriggerDurable.DataProducer)
            //It is important that whatever is used for the instanceId is deterministically unique.
            //In this case, a GUID is used, but it could be a hash of the blob content, or a combination of the blob name and the first line.
            //Whatever provides deterministic uniqueness.
            //Note: GetHashCode is not deterministic across executions, app domains, runtimes, etc. in .NET Core/.NET 5+, so it should not be used.
            var instanceId = $"{name}-{lines[0]}";
            logger.LogInformation($"Using instance ID '{instanceId}' in region {Orchestrator.region}");
            return instanceId;
        }
    }
}
