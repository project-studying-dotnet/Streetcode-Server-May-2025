namespace UserService.WebApi.DTO.Auth.Responses;

public class TokenResponseDTO
{
    public string AccessToken { get; set; } = null!;
    public DateTime AccessTokenExpiresAt { get; set; }

    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiresAt { get; set; }
}