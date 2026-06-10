using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chatbot.Shared.Brokers.Vectors;

public interface IQdrantBroker
{
    ValueTask<IReadOnlyList<QdrantSearchPoint>> HybridSearchAsync(
        string collectionName,
        float[] denseVector,
        (uint[] indices, float[] values) sparseVector,
        int limit = 10);
}

public record QdrantSearchPoint(ulong Id, string Content, Dictionary<string, object?> Payload, float Score);
