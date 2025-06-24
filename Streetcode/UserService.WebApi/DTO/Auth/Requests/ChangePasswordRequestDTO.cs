namespace UserService.WebApi.DTO.Auth.Requests;

public class ChangePasswordRequestDTO
{
    public string Email { get; set; } = string.Empty;
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;

}