using System;

namespace Chatbot.Modules.Chat.Models.Sessions.Exceptions;

public class ChatSessionDependencyException : Exception
{
    public ChatSessionDependencyException(Exception innerException)
        : base("Chat session dependency error occurred, contact support.", innerException)
    {
    }
}
