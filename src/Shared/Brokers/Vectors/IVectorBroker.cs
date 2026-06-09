using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chatbot.Shared.Brokers.Vectors;

public interface IVectorBroker
{
    ValueTask UpsertVectorsAsync(string collectionName, IEnumerable<VectorPoint> points);
    ValueTask<IEnumerable<VectorPoint>> SearchVectorsAsync(
        string collectionName,
        float[] vector,
        int limit = 10
    );
    ValueTask CreateCollectionIfNotExistsAsync(string collectionName, int vectorSize);
}

public record VectorPoint(ulong Id, float[] Vector, Dictionary<string, object?> Payload);
