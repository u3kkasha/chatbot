using System.Threading.Tasks;
using Chatbot.Modules.Chat.Brokers.Storage;
using Chatbot.Modules.Chat.Features.Sessions.Jobs;
using Chatbot.Shared.Brokers.Logging;
using NSubstitute;
using Xunit;

namespace Chatbot.Tests.Unit.Modules.Chat.Sessions.Jobs;

public class ChatSessionCleanupJobTests
{
    private readonly IStorageBroker _storageBrokerMock;
    private readonly ILoggingBroker _loggingBrokerMock;
    private readonly ChatSessionCleanupJob _sut;

    public ChatSessionCleanupJobTests()
    {
        _storageBrokerMock = Substitute.For<IStorageBroker>();
        _loggingBrokerMock = Substitute.For<ILoggingBroker>();

        _sut = new ChatSessionCleanupJob(_storageBrokerMock, _loggingBrokerMock);
    }

    [Fact]
    public async Task ShouldInvokeCleanupJobAndLog()
    {
        // when
        await _sut.Invoke();

        // then
        _loggingBrokerMock.Received(1).LogInformation("Running background job: ChatSessionCleanupJob");
    }
}
