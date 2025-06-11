namespace Streetcode.BLL.DTO.Streetcode.Comment;

public class CreateCommentDTO
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int UserId { get; set; }
    public int StreetcodeId { get; set; }
}
