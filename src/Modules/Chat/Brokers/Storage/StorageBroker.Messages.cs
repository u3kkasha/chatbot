using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Models.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Chatbot.Modules.Chat.Brokers.Storage;

public partial class StorageBroker
{
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;

    public async ValueTask<ChatMessage> InsertChatMessageAsync(ChatMessage chatMessage)
    {
        this.Entry(chatMessage).State = EntityState.Added;
        await this.SaveChangesAsync();

        return chatMessage;
    }

    public IQueryable<ChatMessage> SelectAllChatMessages() => this.Set<ChatMessage>();

    public async ValueTask<ChatMessage?> SelectChatMessageByIdAsync(ChatMessageId chatMessageId) =>
        await this.Set<ChatMessage>().FindAsync(chatMessageId);

    public async ValueTask<ChatMessage> UpdateChatMessageAsync(ChatMessage chatMessage)
    {
        this.Entry(chatMessage).State = EntityState.Modified;
        await this.SaveChangesAsync();

        return chatMessage;
    }

    public async ValueTask<ChatMessage> DeleteChatMessageAsync(ChatMessage chatMessage)
    {
        this.Entry(chatMessage).State = EntityState.Deleted;
        await this.SaveChangesAsync();

        return chatMessage;
    }
}
