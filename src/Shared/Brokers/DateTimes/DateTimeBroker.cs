using NodaTime;

namespace Chatbot.Shared.Brokers.DateTimes;

public class DateTimeBroker(IClock clock) : IDateTimeBroker
{
    public Instant GetCurrentInstant() => clock.GetCurrentInstant();
}
