using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Chatbot.Shared.Brokers.DistributedLock;

public class DistributedLockBroker(IConnectionMultiplexer connectionMultiplexer) : IDistributedLockBroker
{
    private readonly IDatabase database = connectionMultiplexer.GetDatabase();

    public async ValueTask<bool> AcquireLockAsync(string lockKey, string lockValue, TimeSpan expiration)
    {
        return await this.database.LockTakeAsync(lockKey, lockValue, expiration);
    }

    public async ValueTask<bool> ReleaseLockAsync(string lockKey, string lockValue)
    {
        return await this.database.LockReleaseAsync(lockKey, lockValue);
    }
}
