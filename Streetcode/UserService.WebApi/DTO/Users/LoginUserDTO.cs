namespace UserService.WebApi.DTO.Users;

public class LoginUserDTO
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}