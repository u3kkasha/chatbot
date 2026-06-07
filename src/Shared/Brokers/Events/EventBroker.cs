using System.Threading.Tasks;
using MassTransit;

namespace Chatbot.Shared.Brokers.Events;

public class EventBroker(IPublishEndpoint publishEndpoint) : IEventBroker
{
    private readonly IPublishEndpoint publishEndpoint = publishEndpoint;

    public async ValueTask PublishAsync<T>(T @event) where T : class
    {
        await this.publishEndpoint.Publish(@event);
    }
}
