using System.Threading;
using UserService.WebApi.Entities.Users;

namespace UserService.WebApi.Data.Repositories.Interfaces;
public interface IUsersRepository
{
    Task<User> AddAsync(User newUser, CancellationToken cancellationToken);
    Task<int> SaveChangesAsync();
}

