using Chatbot.Modules.Chat.Features.Sessions;
using Chatbot.Modules.Chat.Models.Sessions;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Unit.Modules.Chat.Sessions;

public class ChatSessionStateMachineTests
{
    [Theory]
    [InlineData(ChatSessionStatus.Open, ChatSessionTrigger.AssignOperator, ChatSessionStatus.Pending)]
    [InlineData(ChatSessionStatus.Open, ChatSessionTrigger.Resolve, ChatSessionStatus.Resolved)]
    [InlineData(ChatSessionStatus.Pending, ChatSessionTrigger.UnassignOperator, ChatSessionStatus.Open)]
    [InlineData(ChatSessionStatus.Pending, ChatSessionTrigger.Resolve, ChatSessionStatus.Resolved)]
    public void ShouldTransitionCorrectly(
        ChatSessionStatus initialState,
        ChatSessionTrigger trigger,
        ChatSessionStatus expectedState)
    {
        // given
        ChatSessionStatus resultState = initialState;
        var machine = ChatSessionStateMachineFactory.Create(initialState, state => resultState = state);

        // when
        machine.Fire(trigger);

        // then
        resultState.ShouldBe(expectedState);
        machine.State.ShouldBe(expectedState);
    }

    [Theory]
    [InlineData(ChatSessionStatus.Resolved, ChatSessionTrigger.AssignOperator)]
    [InlineData(ChatSessionStatus.Resolved, ChatSessionTrigger.Resolve)]
    [InlineData(ChatSessionStatus.Open, ChatSessionTrigger.UnassignOperator)]
    public void ShouldThrowExceptionOnInvalidTransition(
        ChatSessionStatus initialState,
        ChatSessionTrigger trigger)
    {
        // given
        var machine = ChatSessionStateMachineFactory.Create(initialState, _ => { });

        // when / then
        Should.Throw<System.InvalidOperationException>(() => machine.Fire(trigger));
    }
}
