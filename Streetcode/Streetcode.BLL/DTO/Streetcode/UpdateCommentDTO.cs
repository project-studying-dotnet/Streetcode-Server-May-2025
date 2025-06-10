using System.ComponentModel.DataAnnotations;

namespace Streetcode.BLL.DTO.Streetcode;

public class UpdateCommentDTO
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;
}