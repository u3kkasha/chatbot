using System;
using Chatbot.Modules.Chat.Models.Sessions;
using Stateless;

namespace Chatbot.Modules.Chat.Features.Sessions;

public static class ChatSessionStateMachineFactory
{
    public static StateMachine<ChatSessionStatus, ChatSessionTrigger> Create(
        ChatSessionStatus initialState,
        Action<ChatSessionStatus> onTransitioned)
    {
        var machine = new StateMachine<ChatSessionStatus, ChatSessionTrigger>(initialState);

        machine.Configure(ChatSessionStatus.Open)
            .Permit(ChatSessionTrigger.AssignOperator, ChatSessionStatus.Pending)
            .Permit(ChatSessionTrigger.Resolve, ChatSessionStatus.Resolved);

        machine.Configure(ChatSessionStatus.Pending)
            .Permit(ChatSessionTrigger.UnassignOperator, ChatSessionStatus.Open)
            .Permit(ChatSessionTrigger.Resolve, ChatSessionStatus.Resolved);

        machine.OnTransitioned(transition => onTransitioned(transition.Destination));

        return machine;
    }
}
