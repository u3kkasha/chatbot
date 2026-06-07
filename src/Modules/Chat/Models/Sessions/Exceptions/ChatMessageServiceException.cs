using System;

namespace Chatbot.Modules.Chat.Models.Sessions.Exceptions;

public class ChatMessageServiceException : Exception
{
    public ChatMessageServiceException(Exception innerException)
        : base("Chat message service error occurred, contact support.", innerException)
    {
    }
}
