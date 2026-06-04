using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Chatbot.Shared.Brokers.Vectors;

public class VectorBroker(IConfiguration configuration) : IVectorBroker
{
    private readonly string host = configuration["Qdrant:Host"] ?? "localhost";
    private readonly int port = int.Parse(configuration["Qdrant:Port"] ?? "6334");
    private readonly bool useHttps = bool.Parse(configuration["Qdrant:UseHttps"] ?? "false");

    public async ValueTask CreateCollectionIfNotExistsAsync(string collectionName, int vectorSize)
    {
        var client = new QdrantClient(this.host, this.port, this.useHttps);
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
        var client = new QdrantClient(this.host, this.port, this.useHttps);

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
        var client = new QdrantClient(this.host, this.port, this.useHttps);

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
