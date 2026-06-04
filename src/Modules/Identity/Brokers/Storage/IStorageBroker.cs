using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Identity.Models.Users;

namespace Chatbot.Modules.Identity.Brokers.Storage;

public interface IStorageBroker
{
    ValueTask<User> InsertUserAsync(User user);
    IQueryable<User> SelectAllUsers();
    ValueTask<User?> SelectUserByIdAsync(UserId userId);
    ValueTask<User> UpdateUserAsync(User user);
    ValueTask<User> DeleteUserAsync(User user);
}
