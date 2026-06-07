using System;
using System.Threading.Tasks;
using Chatbot.Shared.Brokers.DistributedLock;
using NSubstitute;
using StackExchange.Redis;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Unit.Brokers.DistributedLock;

public class DistributedLockBrokerTests
{
    private readonly IConnectionMultiplexer _connectionMultiplexerMock;
    private readonly IDatabase _databaseMock;
    private readonly DistributedLockBroker _sut;

    public DistributedLockBrokerTests()
    {
        _connectionMultiplexerMock = Substitute.For<IConnectionMultiplexer>();
        _databaseMock = Substitute.For<IDatabase>();

        _connectionMultiplexerMock.GetDatabase(Arg.Any<int>(), Arg.Any<object>())
            .Returns(_databaseMock);

        _sut = new DistributedLockBroker(_connectionMultiplexerMock);
    }

    [Fact]
    public async Task ShouldAcquireLockSuccessfully()
    {
        // given
        string lockKey = "my-lock-key";
        string lockValue = "my-lock-value";
        var expiration = TimeSpan.FromSeconds(5);

        _databaseMock.LockTakeAsync(lockKey, lockValue, expiration)
            .Returns(true);

        // when
        bool result = await _sut.AcquireLockAsync(lockKey, lockValue, expiration);

        // then
        result.ShouldBeTrue();
        await _databaseMock.Received(1).LockTakeAsync(lockKey, lockValue, expiration);
    }

    [Fact]
    public async Task ShouldReleaseLockSuccessfully()
    {
        // given
        string lockKey = "my-lock-key";
        string lockValue = "my-lock-value";

        _databaseMock.LockReleaseAsync(lockKey, lockValue)
            .Returns(true);

        // when
        bool result = await _sut.ReleaseLockAsync(lockKey, lockValue);

        // then
        result.ShouldBeTrue();
        await _databaseMock.Received(1).LockReleaseAsync(lockKey, lockValue);
    }
}
