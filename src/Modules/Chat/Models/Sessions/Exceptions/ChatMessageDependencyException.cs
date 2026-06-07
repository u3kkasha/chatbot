using System;

namespace Chatbot.Modules.Chat.Models.Sessions.Exceptions;

public class ChatMessageDependencyException : Exception
{
    public ChatMessageDependencyException(Exception innerException)
        : base("Chat message dependency error occurred, contact support.", innerException)
    {
    }
}
