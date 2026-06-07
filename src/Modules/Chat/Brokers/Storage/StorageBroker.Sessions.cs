using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Models.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Chatbot.Modules.Chat.Brokers.Storage;

public partial class StorageBroker
{
    public DbSet<ChatSession> ChatSessions { get; set; } = null!;

    public async ValueTask<ChatSession> InsertChatSessionAsync(ChatSession chatSession)
    {
        this.Entry(chatSession).State = EntityState.Added;
        await this.SaveChangesAsync();

        return chatSession;
    }

    public IQueryable<ChatSession> SelectAllChatSessions() => this.Set<ChatSession>().AsNoTracking();

    public async ValueTask<ChatSession?> SelectChatSessionByIdAsync(ChatSessionId chatSessionId) =>
        await this.Set<ChatSession>().FindAsync(chatSessionId);

    public async ValueTask<ChatSession> UpdateChatSessionAsync(ChatSession chatSession)
    {
        var local = this.Set<ChatSession>()
            .Local
            .FirstOrDefault(entry => entry.Id == chatSession.Id);

        if (local != null)
        {
            this.Entry(local).State = EntityState.Detached;
        }

        this.Entry(chatSession).State = EntityState.Modified;
        await this.SaveChangesAsync();

        return chatSession;
    }

    public async ValueTask<ChatSession> DeleteChatSessionAsync(ChatSession chatSession)
    {
        await this.ChatSessions
            .Where(s => s.Id == chatSession.Id)
            .ExecuteDeleteAsync();

        return chatSession;
    }
}
