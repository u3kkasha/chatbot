using System.IO;
using System.Threading.Tasks;

namespace Chatbot.Shared.Brokers.Blobs;

public interface IBlobBroker
{
    ValueTask UploadBlobAsync(string containerName, string blobName, Stream content);
    ValueTask<Stream> DownloadBlobAsync(string containerName, string blobName);
    ValueTask DeleteBlobAsync(string containerName, string blobName);
}
