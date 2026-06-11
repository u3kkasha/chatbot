using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Chatbot.Shared.Brokers.Vectors;

public class QdrantBroker(QdrantClient qdrantClient) : IQdrantBroker
{
    public async ValueTask CreateHybridCollectionIfNotExistsAsync(string collectionName, int denseDimension)
    {
        var collections = await qdrantClient.ListCollectionsAsync();
        if (collections.Contains(collectionName)) return;

        await qdrantClient.CreateCollectionAsync(
            collectionName: collectionName,
            vectorsConfig: new VectorParamsMap
            {
                Map =
                {
                    ["dense"] = new VectorParams
                    {
                        Size = (ulong)denseDimension,
                        Distance = Distance.Cosine
                    }
                }
            },
            sparseVectorsConfig: new SparseVectorConfig
            {
                Map =
                {
                    ["sparse"] = new SparseVectorParams()
                }
            }
        );
    }

    public async ValueTask UpsertHybridVectorsAsync(string collectionName, IEnumerable<QdrantHybridPoint> points)
    {
        var pointStructs = points.Select(p =>
        {
            var point = new PointStruct
            {
                Id = Guid.NewGuid(),
                Vectors = new Dictionary<string, Vector>
                {
                    ["dense"] = p.DenseVector,
                    ["sparse"] = (p.SparseVector.values, p.SparseVector.indices)
                },
                Payload =
                {
                    ["content"] = p.Content
                }
            };

            foreach (var kvp in p.Payload)
            {
                if (kvp.Value != null)
                {
                    point.Payload[kvp.Key] = kvp.Value.ToString();
                }
            }

            return point;
        }).ToList();

        await qdrantClient.UpsertAsync(collectionName, pointStructs);
    }

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
            Payload: r.Payload.ToDictionary(
                kvp => kvp.Key,
                kvp => (object?)(kvp.Value.KindCase == Value.KindOneofCase.StringValue ? kvp.Value.StringValue : kvp.Value.ToString())),
            Score: r.Score
        )).ToList();
    }
}
