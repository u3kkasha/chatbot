using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Models.Sessions;

namespace Chatbot.Modules.Chat.Brokers.Storage;

public interface IStorageBroker
{
    ValueTask<ChatSession> InsertChatSessionAsync(ChatSession chatSession);
    IQueryable<ChatSession> SelectAllChatSessions();
    ValueTask<ChatSession?> SelectChatSessionByIdAsync(ChatSessionId chatSessionId);
    ValueTask<ChatSession> UpdateChatSessionAsync(ChatSession chatSession);
    ValueTask<int> UpdateChatSessionsStatusByOperatorAsync(OperatorId operatorId, ChatSessionStatus fromStatus, ChatSessionStatus toStatus);
    ValueTask<ChatSession> DeleteChatSessionAsync(ChatSession chatSession);

    ValueTask<ChatMessage> InsertChatMessageAsync(ChatMessage chatMessage);
    IQueryable<ChatMessage> SelectAllChatMessages();
    ValueTask<ChatMessage?> SelectChatMessageByIdAsync(ChatMessageId chatMessageId);
    ValueTask<ChatMessage> UpdateChatMessageAsync(ChatMessage chatMessage);
    ValueTask<ChatMessage> DeleteChatMessageAsync(ChatMessage chatMessage);
}
