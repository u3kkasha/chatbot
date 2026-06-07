using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Brokers.Storage;
using Chatbot.Modules.Chat.Models.Sessions;
using Chatbot.Modules.Chat.Models.Sessions.Exceptions;
using Chatbot.Shared.Brokers.Events;
using Chatbot.Shared.Events;
using Chatbot.Shared.Infrastructure.Errors;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Chatbot.Modules.Chat.Features.Messages;

public class ChatMessageService(IStorageBroker storageBroker, IEventBroker eventBroker) : IChatMessageService
{
    private readonly IStorageBroker storageBroker = storageBroker;
    private readonly IEventBroker eventBroker = eventBroker;

    public async ValueTask<OneOf<ChatMessage, ValidationError>> AddChatMessageAsync(ChatMessage chatMessage)
    {
        try
        {
            if (chatMessage is null)
            {
                return new ValidationError("Chat message is null.");
            }

            var errors = new Dictionary<string, List<string>>();

            if (chatMessage.Id.Value == Guid.Empty)
            {
                AddError(errors, nameof(ChatMessage.Id), "Id is invalid.");
            }
            if (chatMessage.SessionId.Value == Guid.Empty)
            {
                AddError(errors, nameof(ChatMessage.SessionId), "SessionId is invalid.");
            }
            if (chatMessage.TenantId.Value == Guid.Empty)
            {
                AddError(errors, nameof(ChatMessage.TenantId), "TenantId is invalid.");
            }
            if (string.IsNullOrWhiteSpace(chatMessage.Content))
            {
                AddError(errors, nameof(ChatMessage.Content), "Content is invalid.");
            }

            if (errors.Count > 0)
            {
                return new ValidationError("Chat message validation failed.", errors);
            }

            var insertedMessage = await this.storageBroker.InsertChatMessageAsync(chatMessage);

            var messageAddedEvent = new ChatMessageAddedEvent(
                insertedMessage.Id.Value,
                insertedMessage.SessionId.Value,
                insertedMessage.TenantId,
                insertedMessage.Sender.ToString(),
                insertedMessage.Content,
                insertedMessage.IsAiGenerated,
                insertedMessage.CreatedDate
            );

            await this.eventBroker.PublishAsync(messageAddedEvent);

            return insertedMessage;
        }
        catch (DbUpdateException dbUpdateException)
        {
            throw new ChatMessageDependencyException(dbUpdateException);
        }
        catch (Exception exception)
        {
            throw new ChatMessageServiceException(exception);
        }
    }

    public OneOf<IQueryable<ChatMessage>, ValidationError> RetrieveAllChatMessages()
    {
        try
        {
            return OneOf<IQueryable<ChatMessage>, ValidationError>.FromT0(
                this.storageBroker.SelectAllChatMessages()
            );
        }
        catch (Exception exception)
        {
            throw new ChatMessageServiceException(exception);
        }
    }

    public async ValueTask<OneOf<ChatMessage, NotFoundError>> RetrieveChatMessageByIdAsync(ChatMessageId chatMessageId)
    {
        try
        {
            var message = await this.storageBroker.SelectChatMessageByIdAsync(chatMessageId);

            if (message is null)
            {
                return new NotFoundError($"Chat message with id '{chatMessageId.Value}' was not found.");
            }

            return message;
        }
        catch (DbUpdateException dbUpdateException)
        {
            throw new ChatMessageDependencyException(dbUpdateException);
        }
        catch (Exception exception)
        {
            throw new ChatMessageServiceException(exception);
        }
    }

    public async ValueTask<OneOf<ChatMessage, ValidationError, NotFoundError>> ModifyChatMessageAsync(ChatMessage chatMessage)
    {
        try
        {
            if (chatMessage is null)
            {
                return ValidationError.From(nameof(ChatMessage), "Chat message is null.");
            }

            var errors = new Dictionary<string, List<string>>();

            if (chatMessage.Id.Value == Guid.Empty)
            {
                AddError(errors, nameof(ChatMessage.Id), "Id is invalid.");
            }
            if (chatMessage.SessionId.Value == Guid.Empty)
            {
                AddError(errors, nameof(ChatMessage.SessionId), "SessionId is invalid.");
            }
            if (chatMessage.TenantId.Value == Guid.Empty)
            {
                AddError(errors, nameof(ChatMessage.TenantId), "TenantId is invalid.");
            }
            if (string.IsNullOrWhiteSpace(chatMessage.Content))
            {
                AddError(errors, nameof(ChatMessage.Content), "Content is invalid.");
            }

            if (errors.Count > 0)
            {
                return new ValidationError("Chat message validation failed.", errors);
            }

            var existingMessage = await this.storageBroker.SelectChatMessageByIdAsync(chatMessage.Id);
            if (existingMessage is null)
            {
                return new NotFoundError($"Chat message with id '{chatMessage.Id.Value}' was not found.");
            }

            return await this.storageBroker.UpdateChatMessageAsync(chatMessage);
        }
        catch (DbUpdateException dbUpdateException)
        {
            throw new ChatMessageDependencyException(dbUpdateException);
        }
        catch (Exception exception)
        {
            throw new ChatMessageServiceException(exception);
        }
    }

    public async ValueTask<OneOf<ChatMessage, NotFoundError>> RemoveChatMessageByIdAsync(ChatMessageId chatMessageId)
    {
        try
        {
            var existingMessage = await this.storageBroker.SelectChatMessageByIdAsync(chatMessageId);
            if (existingMessage is null)
            {
                return new NotFoundError($"Chat message with id '{chatMessageId.Value}' was not found.");
            }

            return await this.storageBroker.DeleteChatMessageAsync(existingMessage);
        }
        catch (DbUpdateException dbUpdateException)
        {
            throw new ChatMessageDependencyException(dbUpdateException);
        }
        catch (Exception exception)
        {
            throw new ChatMessageServiceException(exception);
        }
    }

    private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var list))
        {
            list = [];
            errors[key] = list;
        }
        list.Add(message);
    }
}
