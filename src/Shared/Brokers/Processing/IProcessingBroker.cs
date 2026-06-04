using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Chatbot.Shared.Brokers.Processing;

public interface IProcessingBroker
{
    void Enqueue(Expression<Action> methodCall);
    void Enqueue(Expression<Func<Task>> methodCall);
    void Schedule(Expression<Action> methodCall, TimeSpan delay);
    void Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);
}
