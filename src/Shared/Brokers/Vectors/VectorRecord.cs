using Microsoft.Extensions.VectorData;

namespace Chatbot.Shared.Brokers.Vectors;

/// <summary>
/// Internal vector store record used by VectorBroker.
/// Annotated for Microsoft.Extensions.VectorData to handle
/// automatic mapping to/from Qdrant without manual gRPC conversion.
/// </summary>
public sealed class VectorRecord
{
    [VectorStoreKey]
    public ulong Id { get; set; }

    [VectorStoreVector(4096, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }

    [VectorStoreData]
    public string? Content { get; set; }

    [VectorStoreData]
    public string? DocumentId { get; set; }

    [VectorStoreData]
    public string? TenantId { get; set; }
}
