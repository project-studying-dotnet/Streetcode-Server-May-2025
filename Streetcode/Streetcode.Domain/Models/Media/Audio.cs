namespace Streetcode.Domain.Models.Media;

public class Audio
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? BlobName { get; set; }

    public string? MimeType { get; set; }

    public string? Base64 { get; set; }

    public StreetcodeContent? Streetcode { get; set; }
}