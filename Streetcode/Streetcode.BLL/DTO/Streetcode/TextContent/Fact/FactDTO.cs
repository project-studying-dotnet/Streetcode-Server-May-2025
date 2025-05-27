namespace Streetcode.BLL.DTO.Streetcode.TextContent.Fact;

public class FactDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int ImageId { get; set; }
    public int StreetcodeId { get; set; }
    public string FactContent { get; set; }
    public int? Position { get; set; }
}
