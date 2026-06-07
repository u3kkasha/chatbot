using Chatbot.Shared.Models;
using NodaTime;

namespace Chatbot.Modules.Identity.Models.Users;

public record User(
    UserId Id,
    TenantId TenantId,
    string Username,
    string Email,
    string PasswordHash,
    string Role,
    Instant CreatedDate,
    Instant UpdatedDate
);
