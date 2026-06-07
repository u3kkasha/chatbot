using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Models.Sessions;
using Chatbot.Shared.Infrastructure.Errors;
using OneOf;

namespace Chatbot.Modules.Chat.Features.Sessions;

public interface IChatSessionService
{
    ValueTask<OneOf<ChatSession, ValidationError>> AddChatSessionAsync(ChatSession chatSession);
    OneOf<IQueryable<ChatSession>, ValidationError> RetrieveAllChatSessions();
    ValueTask<OneOf<ChatSession, NotFoundError>> RetrieveChatSessionByIdAsync(ChatSessionId chatSessionId);
    ValueTask<OneOf<ChatSession, ValidationError, NotFoundError>> ModifyChatSessionAsync(ChatSession chatSession);
    ValueTask<OneOf<ChatSession, NotFoundError>> RemoveChatSessionByIdAsync(ChatSessionId chatSessionId);
}
