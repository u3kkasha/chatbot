namespace Chatbot.Shared.Models;

public interface ITenantProvider
{
    Guid? GetTenantId();
}
