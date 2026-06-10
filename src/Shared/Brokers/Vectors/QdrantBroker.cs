using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Chatbot.Shared.Brokers.Vectors;

public class QdrantBroker(QdrantClient qdrantClient) : IQdrantBroker
{
    public async ValueTask<IReadOnlyList<QdrantSearchPoint>> HybridSearchAsync(
        string collectionName,
        float[] denseVector,
        (uint[] indices, float[] values) sparseVector,
        int limit = 10
    )
    {
        var results = await qdrantClient.QueryAsync(
            collectionName: collectionName,
            prefetch: new List<PrefetchQuery>
            {
                new() { Query = denseVector, Using = "dense", Limit = 50 },
                new() { Query = (sparseVector.values, sparseVector.indices), Using = "sparse", Limit = 50 }
            },
            query: Fusion.Rrf,
            limit: (ulong)limit
        );

        return results.Select(r => new QdrantSearchPoint(
            Id: r.Id.HasNum ? r.Id.Num : 0,
            Content: r.Payload.TryGetValue("content", out var content) ? content.StringValue : string.Empty,
            Payload: r.Payload.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value.ToString()),
            Score: r.Score
        )).ToList();
    }
}
