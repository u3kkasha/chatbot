using System;
using System.Threading.Tasks;
using Coravel.Queuing.Interfaces;

namespace Chatbot.Shared.Brokers.Processing;

public class ProcessingBroker(IQueue queue) : IProcessingBroker
{
    public void Enqueue(Action action) => queue.QueueTask(action);

    public void Enqueue(Func<Task> function) => queue.QueueAsyncTask(function);
}
