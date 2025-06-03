using Microsoft.EntityFrameworkCore;
using UserService.WebApi.Data.Repositories.Interfaces;
using UserService.WebApi.Entities.Users;

namespace UserService.WebApi.Data.Repositories.Realisations;
public class UsersRepository : IUsersRepository
{
    private readonly UserServiceDbContext _context;

    public UsersRepository(UserServiceDbContext context)
    {
        _context = context;
    }
    public async Task<User> AddAsync(User user, CancellationToken cancellationToken)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        return user;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

