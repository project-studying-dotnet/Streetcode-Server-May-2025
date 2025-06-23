namespace Streetcode.Domain.Models.Media.Images;

public class Image
{
    public int Id { get; set; }

    // This property is not mapped to the DB — leave it here
    public string? Base64 { get; set; }

    public string? BlobName { get; set; }

    public string? MimeType { get; set; }

    public ImageDetails? ImageDetails { get; set; }

    public List<StreetcodeContent> Streetcodes { get; set; } = new();

    public List<Fact> Facts { get; set; } = new();

    public Art? Art { get; set; }

    public Partner? Partner { get; set; }

    public List<SourceLinkCategory> SourceLinkCategories { get; set; } = new();

    public News.News? News { get; set; }

    public TeamMember? TeamMember { get; set; }
}