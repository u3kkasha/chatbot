using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.VectorData;

namespace Chatbot.Shared.Brokers.Vectors;

public class VectorBroker(VectorStore vectorStore) : IVectorBroker
{
    // IL2026/IL3050: GetCollection<TKey, TRecord> uses reflection for mapping.
    // Suppressed because Native AOT is not the current compilation target;
    // the project is AOT-ready by design but not compiled with PublishAot=true.
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Not compiled with PublishAot=true")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Not compiled with PublishAot=true")]
    public async ValueTask CreateCollectionIfNotExistsAsync(string collectionName, int vectorSize)
    {
        var collection = vectorStore.GetCollection<ulong, VectorRecord>(collectionName);
        await collection.EnsureCollectionExistsAsync();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Not compiled with PublishAot=true")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Not compiled with PublishAot=true")]
    public async ValueTask UpsertVectorsAsync(string collectionName, IEnumerable<VectorPoint> points)
    {
        var collection = vectorStore.GetCollection<ulong, VectorRecord>(collectionName);

        var records = points.Select(p => new VectorRecord
        {
            Id = p.Id,
            Vector = p.Vector,
            Content = p.Payload.TryGetValue("content", out var content) ? content?.ToString() : null,
            DocumentId = p.Payload.TryGetValue("documentId", out var docId) ? docId?.ToString() : null,
            TenantId = p.Payload.TryGetValue("tenantId", out var tid) ? tid?.ToString() : null,
        });

        await collection.UpsertAsync(records);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Not compiled with PublishAot=true")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Not compiled with PublishAot=true")]
    public async ValueTask<IEnumerable<VectorPoint>> SearchVectorsAsync(
        string collectionName,
        float[] vector,
        int limit = 10
    )
    {
        var collection = vectorStore.GetCollection<ulong, VectorRecord>(collectionName);

        var results = collection.SearchAsync(
            new ReadOnlyMemory<float>(vector),
            top: limit
        );

        var list = new List<VectorPoint>();
        await foreach (var r in results)
        {
            list.Add(new VectorPoint(
                r.Record.Id,
                r.Record.Vector.ToArray(),
                new Dictionary<string, object?>
                {
                    ["content"] = r.Record.Content,
                    ["documentId"] = r.Record.DocumentId,
                    ["tenantId"] = r.Record.TenantId,
                    ["score"] = r.Score,
                }
            ));
        }

        return list;
    }
}
