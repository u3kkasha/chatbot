namespace Chatbot.Shared.Brokers.Pii;

public interface IPiiBroker
{
    string MaskSensitiveData(string text);
}
