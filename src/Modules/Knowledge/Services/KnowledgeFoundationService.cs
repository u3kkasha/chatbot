using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Knowledge.Models;
using Chatbot.Modules.Knowledge.Models.Errors;
using Chatbot.Shared.Brokers.Ai;
using Chatbot.Shared.Brokers.Logging;
using Chatbot.Shared.Brokers.Vectors;
using OneOf;

namespace Chatbot.Modules.Knowledge.Services;

public class KnowledgeFoundationService(
    IAiBroker aiBroker,
    ISparseVectorService sparseVectorService,
    IQdrantBroker qdrantBroker,
    ILoggingBroker loggingBroker
) : IKnowledgeFoundationService
{
    private const string CollectionName = "knowledge";

    public async ValueTask<OneOf<IReadOnlyList<KnowledgeChunk>, ServiceError, DependencyError>> RetrieveAsync(string query)
    {
        try
        {
            // 1. Get Dense Vector
            float[] denseVector = await aiBroker.GenerateEmbeddingAsync(query);

            // 2. Get Sparse Vector
            var sparseVector = sparseVectorService.GenerateAsync(query);

            // 3. Hybrid Search (Top 50 via RRF)
            var candidates = await qdrantBroker.HybridSearchAsync(
                CollectionName,
                denseVector,
                sparseVector,
                limit: 50);

            if (candidates.Count == 0)
                return new List<KnowledgeChunk>();

            // 4. Rerank (Top 50 -> Top 5 via Cohere)
            var rerankResults = await aiBroker.RerankAsync(
                query,
                candidates.Select(c => c.Content),
                topN: 5);

            // 5. Map to KnowledgeChunk
            var finalChunks = rerankResults.Select(r =>
            {
                var originalCandidate = candidates[r.Index];
                return new KnowledgeChunk(
                    Content: r.Document,
                    DocumentId: originalCandidate.Payload.TryGetValue("documentId", out var docId) ? docId?.ToString() ?? string.Empty : string.Empty,
                    Score: r.Score);
            }).ToList();

            return finalChunks;
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception);

            if (IsDependencyException(exception))
            {
                return new DependencyError("A dependency error occurred during knowledge retrieval.", exception);
            }

            return new ServiceError("A service error occurred during knowledge retrieval.", exception);
        }
    }

    private static bool IsDependencyException(Exception exception) =>
        exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
        exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
        exception is HttpRequestException;
}
