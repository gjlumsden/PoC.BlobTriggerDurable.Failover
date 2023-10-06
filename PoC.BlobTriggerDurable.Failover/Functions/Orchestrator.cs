using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace PoC.BlobTriggerDurable.Failover.Functions
{
    public static class Orchestrator
    {
        public static readonly string region = Environment.GetEnvironmentVariable("Region");

        [FunctionName(nameof(RunOrchestrator))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var content = context.GetInput<BlobContent>();
            //We can delete the blob now since we have all the information required for the orchestration to run.
            await context.CallActivityAsync(nameof(ActivityFunctions.DeleteOriginalBlob), content);

            var outputs = new List<string>();
            foreach (var line in content.Lines)
            {
                outputs.Add(await context.CallActivityAsync<string>(nameof(ActivityFunctions.EchoLine), line));
            }

            await context.CallActivityAsync(nameof(ActivityFunctions.WriteOutput), new BlobContent { Filename = $"{region}-{content.Filename}", Lines = outputs });
            await context.CallActivityAsync(nameof(ActivityFunctions.WriteToTable), new TableLogEntry { PartitionKey = content.Filename, RowKey = $"{region}-{context.InstanceId}-{context.NewGuid()}" });

            return outputs;
        }
    }
}