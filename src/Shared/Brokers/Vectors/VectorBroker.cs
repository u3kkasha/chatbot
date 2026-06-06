using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chatbot.Shared.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Chatbot.Shared.Brokers.Vectors;

public class VectorBroker(IOptions<QdrantOptions> qdrantOptions) : IVectorBroker
{
    private readonly QdrantOptions options = qdrantOptions.Value;

    public async ValueTask CreateCollectionIfNotExistsAsync(string collectionName, int vectorSize)
    {
        var client = new QdrantClient(options.Host, options.Port, options.UseHttps);
        var collections = await client.ListCollectionsAsync();

        if (!collections.Contains(collectionName))
        {
            await client.CreateCollectionAsync(
                collectionName,
                new VectorParams { Size = (ulong)vectorSize, Distance = Distance.Cosine }
            );
        }
    }

    public async ValueTask UpsertVectorsAsync(
        string collectionName,
        IEnumerable<VectorPoint> points
    )
    {
        var client = new QdrantClient(options.Host, options.Port, options.UseHttps);

        var pointStructs = points
            .Select(p => new PointStruct
            {
                Id = p.Id,
                Vectors = p.Vector,
                Payload = { p.Payload.ToDictionary(k => k.Key, v => v.Value.ToValue()) },
            })
            .ToList();

        await client.UpsertAsync(collectionName, pointStructs);
    }

    public async ValueTask<IEnumerable<VectorPoint>> SearchVectorsAsync(
        string collectionName,
        float[] vector,
        int limit = 10
    )
    {
        var client = new QdrantClient(options.Host, options.Port, options.UseHttps);

        var results = await client.SearchAsync(collectionName, vector, limit: (uint)limit);

        return results.Select(r => new VectorPoint(
            r.Id.Num,
            r.Vectors.Vector.Dense.Data.ToArray(),
            r.Payload.ToDictionary(k => k.Key, v => v.Value.ToValue())
        ));
    }
}

internal static class QdrantExtensions
{
    public static Value ToValue(this object obj) =>
        obj switch
        {
            string s => new Value { StringValue = s },
            int i => new Value { IntegerValue = i },
            long l => new Value { IntegerValue = l },
            float f => new Value { DoubleValue = f },
            double d => new Value { DoubleValue = d },
            bool b => new Value { BoolValue = b },
            _ => new Value { StringValue = obj?.ToString() ?? string.Empty },
        };

    public static object ToValue(this Value value) =>
        value.KindCase switch
        {
            Value.KindOneofCase.StringValue => value.StringValue,
            Value.KindOneofCase.IntegerValue => value.IntegerValue,
            Value.KindOneofCase.DoubleValue => value.DoubleValue,
            Value.KindOneofCase.BoolValue => value.BoolValue,
            _ => value.ToString(),
        };
}
