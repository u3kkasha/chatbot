using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;

namespace Chatbot.Shared.Brokers.Processing;

public class ProcessingBroker(IBackgroundJobClient backgroundJobClient) : IProcessingBroker
{
    public void Enqueue(Expression<Action> methodCall) => backgroundJobClient.Enqueue(methodCall);

    public void Enqueue(Expression<Func<Task>> methodCall) =>
        backgroundJobClient.Enqueue(methodCall);

    public void Schedule(Expression<Action> methodCall, TimeSpan delay) =>
        backgroundJobClient.Schedule(methodCall, delay);

    public void Schedule(Expression<Func<Task>> methodCall, TimeSpan delay) =>
        backgroundJobClient.Schedule(methodCall, delay);
}
