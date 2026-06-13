using System.Threading.Tasks;
using Coravel.Events.Interfaces;
using Coravel.Queuing.Interfaces;

namespace Chatbot.Shared.Brokers.Events;

public class EventBroker(IQueue queue) : IEventBroker
{
    private readonly IQueue queue = queue;

    public ValueTask PublishAsync<T>(T @event) where T : class, IEvent
    {
        this.queue.QueueBroadcast(@event);
        return ValueTask.CompletedTask;
    }
}
