using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chatbot.Shared.Brokers.Vectors;

public interface IQdrantBroker
{
    ValueTask CreateHybridCollectionIfNotExistsAsync(string collectionName, int denseDimension);

    ValueTask UpsertHybridVectorsAsync(
        string collectionName,
        IEnumerable<QdrantHybridPoint> points);

    ValueTask<IReadOnlyList<QdrantSearchPoint>> HybridSearchAsync(
        string collectionName,
        float[] denseVector,
        (uint[] indices, float[] values) sparseVector,
        int limit = 10);
}

public record QdrantHybridPoint(
    string Content,
    float[] DenseVector,
    (uint[] indices, float[] values) SparseVector,
    Dictionary<string, object?> Payload);

public record QdrantSearchPoint(ulong Id, string Content, Dictionary<string, object?> Payload, float Score);
