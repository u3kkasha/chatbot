using NodaTime;

namespace Chatbot.Modules.Identity.Models.Users;

public class User
{
    public UserId Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Instant CreatedDate { get; set; }
    public Instant UpdatedDate { get; set; }
}
