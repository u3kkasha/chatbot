using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Chatbot.Shared.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Chatbot.Shared.Brokers.Blobs;

public class BlobBroker(IOptions<ConnectionStringsOptions> connectionStringsOptions) : IBlobBroker
{
    private readonly string connectionString = connectionStringsOptions.Value.BlobStorage;

    public async ValueTask UploadBlobAsync(string containerName, string blobName, Stream content)
    {
        var blobServiceClient = new BlobServiceClient(this.connectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(
            containerName
        );
        await containerClient.CreateIfNotExistsAsync();
        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(content, overwrite: true);
    }

    public async ValueTask<Stream> DownloadBlobAsync(string containerName, string blobName)
    {
        var blobServiceClient = new BlobServiceClient(this.connectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(
            containerName
        );
        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.DownloadAsync();

        return response.Value.Content;
    }

    public async ValueTask DeleteBlobAsync(string containerName, string blobName)
    {
        var blobServiceClient = new BlobServiceClient(this.connectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(
            containerName
        );
        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}
