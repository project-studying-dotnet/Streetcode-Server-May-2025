namespace Streetcode.BLL.DTO.Streetcode;

public class CommentDto
{
    public int Id { get; set; }
    public int StreetcodeId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Author { get; set; }
    public DateTime CreatedAt { get; set; }
} 