using NodaTime;

namespace Chatbot.Modules.Knowledge.Models.Documents;

public record KnowledgeDocument(
    KnowledgeDocumentId Id,
    TenantId TenantId,
    string Title,
    string FileName,
    string BlobPath,
    long SizeInBytes,
    string ContentType,
    Instant CreatedDate,
    Instant UpdatedDate
);
