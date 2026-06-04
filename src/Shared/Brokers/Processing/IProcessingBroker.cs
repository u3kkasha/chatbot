using System;
using System.Threading.Tasks;

namespace Chatbot.Shared.Brokers.Processing;

public interface IProcessingBroker
{
    void Enqueue(Action action);
    void Enqueue(Func<Task> function);
}
