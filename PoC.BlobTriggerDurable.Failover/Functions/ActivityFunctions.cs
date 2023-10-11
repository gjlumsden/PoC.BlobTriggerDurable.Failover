using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;

namespace PoC.BlobTriggerDurable.Failover.Functions
{
    public static class ActivityFunctions
    {
        //This class contains simple actions that the orchstrator uses.

        [FunctionName(nameof(EchoLine))]
        public static string EchoLine([ActivityTrigger] string line, ILogger log)
        {
            log.LogInformation($"echo {line}");
            return $"Echo {line}";
        }

        [FunctionName(nameof(WriteOutput))]
        public static async Task WriteOutput(
            [ActivityTrigger] BlobContent output,
            [Blob("output/{output.Filename}", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream blobContent)
        {
            await blobContent.WriteAsync(Encoding.UTF8.GetBytes(string.Join("\n", output.Lines)));
        }

        //A function that takes a TableLogEntry as input and writes it to a table
        [FunctionName(nameof(WriteToTable))]
        [return: Table("logs", Connection = "AzureWebJobsStorage")]
        public static TableLogEntry WriteToTable([ActivityTrigger] TableLogEntry input)
        {
            return input;
        }

        [FunctionName(nameof(DeleteOriginalBlob))]
        public static async Task DeleteOriginalBlob(
            [ActivityTrigger] BlobContent original)
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var blobClient = new BlobClient(connectionString, "input", original.Filename);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
