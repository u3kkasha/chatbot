using System.Threading.Tasks;

namespace Chatbot.Shared.Brokers.Events;

public interface IEventBroker
{
    ValueTask PublishAsync<T>(T @event) where T : class;
}
