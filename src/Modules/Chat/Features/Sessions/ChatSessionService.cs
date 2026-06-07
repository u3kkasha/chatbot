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

namespace Chatbot.Modules.Chat.Features.Sessions;

public class ChatSessionService(IStorageBroker storageBroker, IEventBroker eventBroker) : IChatSessionService
{
    private readonly IStorageBroker storageBroker = storageBroker;
    private readonly IEventBroker eventBroker = eventBroker;

    public async ValueTask<OneOf<ChatSession, ValidationError>> AddChatSessionAsync(ChatSession chatSession)
    {
        try
        {
            if (chatSession is null)
            {
                return new ValidationError("Chat session is null.");
            }

            var errors = new Dictionary<string, List<string>>();

            if (chatSession.Id.Value == Guid.Empty)
            {
                AddError(errors, nameof(ChatSession.Id), "Id is invalid.");
            }
            if (chatSession.TenantId.Value == Guid.Empty)
            {
                AddError(errors, nameof(ChatSession.TenantId), "TenantId is invalid.");
            }
            if (string.IsNullOrWhiteSpace(chatSession.CustomerIdentifier))
            {
                AddError(errors, nameof(ChatSession.CustomerIdentifier), "CustomerIdentifier is required.");
            }
            if (chatSession.OperatorId?.Value == Guid.Empty)
            {
                AddError(errors, nameof(ChatSession.OperatorId), "OperatorId is invalid.");
            }

            if (errors.Count > 0)
            {
                return new ValidationError("Chat session validation failed.", errors);
            }

            return await this.storageBroker.InsertChatSessionAsync(chatSession);
        }
        catch (DbUpdateException dbUpdateException)
        {
            throw new ChatSessionDependencyException(dbUpdateException);
        }
        catch (Exception exception)
        {
            throw new ChatSessionServiceException(exception);
        }
    }

    public OneOf<IQueryable<ChatSession>, ValidationError> RetrieveAllChatSessions()
    {
        try
        {
            return OneOf<IQueryable<ChatSession>, ValidationError>.FromT0(
                this.storageBroker.SelectAllChatSessions()
            );
        }
        catch (Exception exception)
        {
            throw new ChatSessionServiceException(exception);
        }
    }

    public async ValueTask<OneOf<ChatSession, NotFoundError>> RetrieveChatSessionByIdAsync(ChatSessionId chatSessionId)
    {
        try
        {
            var session = await this.storageBroker.SelectChatSessionByIdAsync(chatSessionId);

            if (session is null)
            {
                return new NotFoundError($"Chat session with id '{chatSessionId.Value}' was not found.");
            }

            return session;
        }
        catch (DbUpdateException dbUpdateException)
        {
            throw new ChatSessionDependencyException(dbUpdateException);
        }
        catch (Exception exception)
        {
            throw new ChatSessionServiceException(exception);
        }
    }

    public async ValueTask<OneOf<ChatSession, ValidationError, NotFoundError>> ModifyChatSessionAsync(ChatSession chatSession)
    {
        try
        {
            if (chatSession is null)
            {
                return ValidationError.From(nameof(ChatSession), "Chat session is null.");
            }

            var errors = new Dictionary<string, List<string>>();

            if (chatSession.Id.Value == Guid.Empty)
            {
                AddError(errors, nameof(ChatSession.Id), "Id is invalid.");
            }
            if (chatSession.TenantId.Value == Guid.Empty)
            {
                AddError(errors, nameof(ChatSession.TenantId), "TenantId is invalid.");
            }
            if (string.IsNullOrWhiteSpace(chatSession.CustomerIdentifier))
            {
                AddError(errors, nameof(ChatSession.CustomerIdentifier), "CustomerIdentifier is required.");
            }
            if (chatSession.OperatorId?.Value == Guid.Empty)
            {
                AddError(errors, nameof(ChatSession.OperatorId), "OperatorId is invalid.");
            }

            if (errors.Count > 0)
            {
                return new ValidationError("Chat session validation failed.", errors);
            }

            var existingSession = await this.storageBroker.SelectChatSessionByIdAsync(chatSession.Id);
            if (existingSession is null)
            {
                return new NotFoundError($"Chat session with id '{chatSession.Id.Value}' was not found.");
            }

            var statusChanged = existingSession.Status != chatSession.Status;
            var oldStatus = existingSession.Status;

            var updatedSession = await this.storageBroker.UpdateChatSessionAsync(chatSession);

            if (statusChanged)
            {
                var @event = new ChatSessionStatusChangedEvent(
                    updatedSession.Id.Value,
                    updatedSession.TenantId,
                    oldStatus.ToString(),
                    updatedSession.Status.ToString(),
                    updatedSession.UpdatedDate
                );

                await this.eventBroker.PublishAsync(@event);
            }

            return updatedSession;
        }
        catch (DbUpdateException dbUpdateException)
        {
            throw new ChatSessionDependencyException(dbUpdateException);
        }
        catch (Exception exception)
        {
            throw new ChatSessionServiceException(exception);
        }
    }

    public async ValueTask<int> BulkUpdateSessionsStatusAsync(
        OperatorId operatorId,
        ChatSessionStatus fromStatus,
        ChatSessionStatus toStatus)
    {
        try
        {
            return await this.storageBroker.UpdateChatSessionsStatusByOperatorAsync(operatorId, fromStatus, toStatus);
        }
        catch (DbUpdateException dbUpdateException)
        {
            throw new ChatSessionDependencyException(dbUpdateException);
        }
        catch (Exception exception)
        {
            throw new ChatSessionServiceException(exception);
        }
    }

    public async ValueTask<OneOf<ChatSession, NotFoundError>> RemoveChatSessionByIdAsync(ChatSessionId chatSessionId)
    {
        try
        {
            var existingSession = await this.storageBroker.SelectChatSessionByIdAsync(chatSessionId);
            if (existingSession is null)
            {
                return new NotFoundError($"Chat session with id '{chatSessionId.Value}' was not found.");
            }

            return await this.storageBroker.DeleteChatSessionAsync(existingSession);
        }
        catch (DbUpdateException dbUpdateException)
        {
            throw new ChatSessionDependencyException(dbUpdateException);
        }
        catch (Exception exception)
        {
            throw new ChatSessionServiceException(exception);
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
