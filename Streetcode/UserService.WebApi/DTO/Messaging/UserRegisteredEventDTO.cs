namespace UserService.WebApi.DTO.Messaging;

public class UserRegisteredEventDTO
{
    public string UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;

    public DateTime RegisteredAt { get; set; }
}