using Microsoft.EntityFrameworkCore;
using UserService.WebApi.Entities.Users;

namespace UserService.WebApi.Data;

public class UserServiseDbContext : DbContext
{
    public UserServiseDbContext()
    {
    }

    public UserServiseDbContext(DbContextOptions<UserServiseDbContext> options)
    : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

