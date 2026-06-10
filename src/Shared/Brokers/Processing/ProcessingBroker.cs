using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chatbot.Shared.Brokers.Processing.Models;

namespace Chatbot.Shared.Brokers.Processing;

public class ProcessingBroker(IDoclingClient doclingClient) : IProcessingBroker
{
    public async ValueTask<IReadOnlyList<DocumentChunk>> ChunkDocumentAsync(Stream document, string fileName)
    {
        var response = await doclingClient.ProcessAsync(document, fileName);
        return response.Chunks;
    }
}
