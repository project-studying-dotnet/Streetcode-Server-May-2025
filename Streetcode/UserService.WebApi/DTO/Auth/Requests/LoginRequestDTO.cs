namespace UserService.WebApi.DTO.Auth.Requests;

public class LoginRequestDTO
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}