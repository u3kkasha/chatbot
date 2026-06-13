using System.Threading.Tasks;
using Coravel.Events.Interfaces;

namespace Chatbot.Shared.Brokers.Events;

public interface IEventBroker
{
    ValueTask PublishAsync<T>(T @event) where T : class, IEvent;
}
