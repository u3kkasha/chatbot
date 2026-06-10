using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chatbot.Shared.Brokers.Processing.Models;

namespace Chatbot.Shared.Brokers.Processing;

public interface IProcessingBroker
{
    ValueTask<IReadOnlyList<DocumentChunk>> ChunkDocumentAsync(Stream document, string fileName);
}
