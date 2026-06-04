using NodaTime;

namespace Chatbot.Modules.Identity.Models.Users;

public record User(
    UserId Id,
    string Username,
    string Email,
    string PasswordHash,
    Instant CreatedDate,
    Instant UpdatedDate
);
