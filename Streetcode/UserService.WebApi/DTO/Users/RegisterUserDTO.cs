using System.ComponentModel.DataAnnotations;
using UserService.WebApi.Entities.Users;

namespace UserService.WebApi.DTO.Users;
public class RegisterUserDTO
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    [Required]
    [MaxLength(50)]
    public string Surname { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    [MaxLength(20)]
    public string Password { get; set; }
}

