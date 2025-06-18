namespace UserService.WebApi.DTO.Auth.Requests;

public class LogoutRequestDTO
{
    public string RefreshToken { get; set; } = null!;
}