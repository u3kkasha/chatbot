using System;
using System.Threading.Tasks;

namespace Chatbot.Shared.Brokers.DistributedLock;

public interface IDistributedLockBroker
{
    ValueTask<bool> AcquireLockAsync(string lockKey, string lockValue, TimeSpan expiration);
    ValueTask<bool> ReleaseLockAsync(string lockKey, string lockValue);
}
