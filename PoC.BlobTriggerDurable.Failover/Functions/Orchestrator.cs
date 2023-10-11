using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace PoC.BlobTriggerDurable.Failover.Functions
{
    public static class Orchestrator
    {
        //Add a Region app setting to each function app, with the value set to the region name (e.g., uks/ukw).
        public static readonly string region = Environment.GetEnvironmentVariable("Region") ?? "No-Region-Specified";

        [FunctionName(nameof(RunOrchestrator))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var content = context.GetInput<BlobContent>();
            //If the secondary region is passive, delete the blob to prevent it being processed again when the secondary region becomes active
            //Be aware that if the configuration is active-active, both regions will be triggered and the blob will be deleted by the first region to process it
            //It is possible that the blob is deleted by the first region before the second region has been triggered
            //To test the effects of not deleting the blob, comment out the following line and deploy the function app to both regions
            await context.CallActivityAsync(nameof(ActivityFunctions.DeleteOriginalBlob), content);

            var outputs = new List<string>();
            foreach (var line in content.Lines)
            {
                //Do some work.
                outputs.Add(await context.CallActivityAsync<string>(nameof(ActivityFunctions.EchoLine), line));
            }

            await context.CallActivityAsync(nameof(ActivityFunctions.WriteOutput),
                                            new BlobContent
                                            {
                                                Filename = $"{region}-{content.Filename}",
                                                Lines = outputs
                                            });
            
            //This log table can validate the number of orchstration instances executed for each blob to ensure no concurrent/duplicate executions.
            await context.CallActivityAsync(
                nameof(ActivityFunctions.WriteToTable),
                new TableLogEntry
                {
                    PartitionKey = content.Filename,
                    RowKey = $"{region}-{context.InstanceId}-{context.NewGuid()}" //Note: use of context.NewGuid() to ensure determinism.
                });

            return outputs;
        }
    }
}