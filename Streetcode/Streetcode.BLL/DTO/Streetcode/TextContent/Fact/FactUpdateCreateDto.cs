using System.ComponentModel.DataAnnotations;
using Streetcode.DAL.Entities.Media.Images;

namespace Streetcode.BLL.DTO.Streetcode.TextContent.Fact;

public class FactUpdateCreateDTO : FactDTO
{
    [Required]
    [MaxLength(68)]
    public string Title { get; set; }

    [Required]
    [MaxLength(600)]
    public string FactContent { get; set; }

    [Required]
    public Image Image { get; set; }

    [MaxLength(200)]
    public string? ImageDescription { get; set; }
}