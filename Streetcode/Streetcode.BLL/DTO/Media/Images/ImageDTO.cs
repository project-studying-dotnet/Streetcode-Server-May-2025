namespace Streetcode.BLL.DTO.Media.Images;

public class ImageDTO
{
    public int Id { get; set; }

    public string? BlobName { get; set; }
    public string? Base64 { get; set; }
    public string? MimeType { get; set; }
    public ImageDetailsDTO? ImageDetails { get; set; }
}