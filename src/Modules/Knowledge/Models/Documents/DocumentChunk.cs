using System;
using NodaTime;

namespace Chatbot.Modules.Knowledge.Models.Documents;

public record DocumentChunk(
    DocumentChunkId Id,
    KnowledgeDocumentId DocumentId,
    TenantId TenantId,
    int Index,
    string Content,
    Guid VectorId,
    Instant CreatedDate,
    Instant UpdatedDate
);
