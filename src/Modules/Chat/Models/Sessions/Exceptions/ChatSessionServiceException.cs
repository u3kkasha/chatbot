using System;

namespace Chatbot.Modules.Chat.Models.Sessions.Exceptions;

public class ChatSessionServiceException : Exception
{
    public ChatSessionServiceException(Exception innerException)
        : base("Chat session service error occurred, contact support.", innerException)
    {
    }
}
