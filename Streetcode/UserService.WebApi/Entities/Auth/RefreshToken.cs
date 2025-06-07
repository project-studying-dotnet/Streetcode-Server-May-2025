using UserService.WebApi.Entities.Users;

namespace UserService.WebApi.Entities.Auth;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}