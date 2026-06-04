using NodaTime;

namespace Chatbot.Shared.Brokers.DateTimes;

public interface IDateTimeBroker
{
    Instant GetCurrentInstant();
}
