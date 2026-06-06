using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Knowledge.Models.Documents;

namespace Chatbot.Modules.Knowledge.Brokers.Storage;

public interface IStorageBroker
{
    ValueTask<KnowledgeDocument> InsertKnowledgeDocumentAsync(KnowledgeDocument knowledgeDocument);
    IQueryable<KnowledgeDocument> SelectAllKnowledgeDocuments();
    ValueTask<KnowledgeDocument?> SelectKnowledgeDocumentByIdAsync(
        KnowledgeDocumentId knowledgeDocumentId
    );
    ValueTask<KnowledgeDocument> UpdateKnowledgeDocumentAsync(KnowledgeDocument knowledgeDocument);
    ValueTask<KnowledgeDocument> DeleteKnowledgeDocumentAsync(KnowledgeDocument knowledgeDocument);

    ValueTask<DocumentChunk> InsertDocumentChunkAsync(DocumentChunk documentChunk);
    IQueryable<DocumentChunk> SelectAllDocumentChunks();
    ValueTask<DocumentChunk?> SelectDocumentChunkByIdAsync(DocumentChunkId documentChunkId);
    ValueTask<DocumentChunk> UpdateDocumentChunkAsync(DocumentChunk documentChunk);
    ValueTask<DocumentChunk> DeleteDocumentChunkAsync(DocumentChunk documentChunk);
}
