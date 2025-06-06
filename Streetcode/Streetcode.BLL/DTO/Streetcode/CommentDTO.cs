using Streetcode.BLL.DTO.Users;

namespace Streetcode.BLL.DTO.Streetcode;

public class CommentDTO
{
    public int Id { get; set; }
    public string Text { get; set; }
    public UserDTO User { get; set; }
    public int StreetcodeId { get; set; }
    public int? ParentCommentId { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CommentDTO> Replies { get; set; } = new();
} 