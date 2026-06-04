using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Identity.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Chatbot.Modules.Identity.Brokers.Storage;

public partial class StorageBroker
{
    public DbSet<User> Users { get; set; }

    public async ValueTask<User> InsertUserAsync(User user)
    {
        this.Entry(user).State = EntityState.Added;
        await this.SaveChangesAsync();

        return user;
    }

    public IQueryable<User> SelectAllUsers() => this.Set<User>();

    public async ValueTask<User?> SelectUserByIdAsync(UserId userId) =>
        await this.Set<User>().FindAsync(userId);

    public async ValueTask<User> UpdateUserAsync(User user)
    {
        this.Entry(user).State = EntityState.Modified;
        await this.SaveChangesAsync();

        return user;
    }

    public async ValueTask<User> DeleteUserAsync(User user)
    {
        this.Entry(user).State = EntityState.Deleted;
        await this.SaveChangesAsync();

        return user;
    }
}
