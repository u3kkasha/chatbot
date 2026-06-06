using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Knowledge.Models.Documents;
using Microsoft.EntityFrameworkCore;

namespace Chatbot.Modules.Knowledge.Brokers.Storage;

public partial class StorageBroker
{
    public DbSet<DocumentChunk> DocumentChunks { get; set; } = null!;

    public async ValueTask<DocumentChunk> InsertDocumentChunkAsync(DocumentChunk documentChunk)
    {
        this.Entry(documentChunk).State = EntityState.Added;
        await this.SaveChangesAsync();

        return documentChunk;
    }

    public IQueryable<DocumentChunk> SelectAllDocumentChunks() => this.Set<DocumentChunk>();

    public async ValueTask<DocumentChunk?> SelectDocumentChunkByIdAsync(
        DocumentChunkId documentChunkId
    ) => await this.Set<DocumentChunk>().FindAsync(documentChunkId);

    public async ValueTask<DocumentChunk> UpdateDocumentChunkAsync(DocumentChunk documentChunk)
    {
        this.Entry(documentChunk).State = EntityState.Modified;
        await this.SaveChangesAsync();

        return documentChunk;
    }

    public async ValueTask<DocumentChunk> DeleteDocumentChunkAsync(DocumentChunk documentChunk)
    {
        this.Entry(documentChunk).State = EntityState.Deleted;
        await this.SaveChangesAsync();

        return documentChunk;
    }
}
