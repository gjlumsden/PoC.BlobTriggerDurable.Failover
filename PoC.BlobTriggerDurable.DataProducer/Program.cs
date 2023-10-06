using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace PoC.BlobTriggerDurable.DataProducer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //Configure use of user secrets
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<Program>();
            var config = builder.Build();
            //Read blob connection string from user secrets
            var connectionString = config["BlobStorageConnectionString"];

            var fn = 0;
            while (true)
            {
                var dt = DateTimeOffset.UtcNow;
                var lines = Enumerable.Range(1, 10)
                                      .Select(i => $"{dt:O} - {i}")
                                      .ToList();
                lines.Insert(0, Guid.NewGuid().ToString());
                //Upload lines as lines of a text file to blob storage in the path input/{fn}.txt
                var fileName = $"{(++fn):000#}.txt";
                var blobClient = new BlobClient(connectionString, "input", fileName);
                await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(string.Join("\n", lines))));
                Console.WriteLine($"Uploaded {fileName}");
                await Task.Delay(3000);
            }
        }
    }
}