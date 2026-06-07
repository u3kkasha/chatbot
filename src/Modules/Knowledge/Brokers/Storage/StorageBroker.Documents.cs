using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Knowledge.Models.Documents;
using Microsoft.EntityFrameworkCore;

namespace Chatbot.Modules.Knowledge.Brokers.Storage;

public partial class StorageBroker
{
    public DbSet<KnowledgeDocument> KnowledgeDocuments { get; set; } = null!;

    public async ValueTask<KnowledgeDocument> InsertKnowledgeDocumentAsync(
        KnowledgeDocument knowledgeDocument
    )
    {
        this.Entry(knowledgeDocument).State = EntityState.Added;
        await this.SaveChangesAsync();

        return knowledgeDocument;
    }

    public IQueryable<KnowledgeDocument> SelectAllKnowledgeDocuments() =>
        this.Set<KnowledgeDocument>();

    public async ValueTask<KnowledgeDocument?> SelectKnowledgeDocumentByIdAsync(
        KnowledgeDocumentId knowledgeDocumentId
    ) => await this.Set<KnowledgeDocument>().FirstOrDefaultAsync(d => d.Id == knowledgeDocumentId);

    public async ValueTask<KnowledgeDocument> UpdateKnowledgeDocumentAsync(
        KnowledgeDocument knowledgeDocument
    )
    {
        this.Entry(knowledgeDocument).State = EntityState.Modified;
        await this.SaveChangesAsync();

        return knowledgeDocument;
    }

    public async ValueTask<KnowledgeDocument> DeleteKnowledgeDocumentAsync(
        KnowledgeDocument knowledgeDocument
    )
    {
        await this.KnowledgeDocuments
            .Where(d => d.Id == knowledgeDocument.Id)
            .ExecuteDeleteAsync();

        return knowledgeDocument;
    }
}
