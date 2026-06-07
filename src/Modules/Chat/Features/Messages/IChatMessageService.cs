using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Models.Sessions;
using Chatbot.Shared.Infrastructure.Errors;
using OneOf;

namespace Chatbot.Modules.Chat.Features.Messages;

public interface IChatMessageService
{
    ValueTask<OneOf<ChatMessage, ValidationError>> AddChatMessageAsync(ChatMessage chatMessage);
    OneOf<IQueryable<ChatMessage>, ValidationError> RetrieveAllChatMessages();
    ValueTask<OneOf<ChatMessage, NotFoundError>> RetrieveChatMessageByIdAsync(ChatMessageId chatMessageId);
    ValueTask<OneOf<ChatMessage, ValidationError, NotFoundError>> ModifyChatMessageAsync(ChatMessage chatMessage);
    ValueTask<OneOf<ChatMessage, NotFoundError>> RemoveChatMessageByIdAsync(ChatMessageId chatMessageId);
}
