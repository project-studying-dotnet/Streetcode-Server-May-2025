namespace Streetcode.Domain.Models.Media.Images;

public class Art
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public string? Title { get; set; }

    public int ImageId { get; set; }

    public Image? Image { get; set; }

    public List<StreetcodeArt> StreetcodeArts { get; set; } = new();
}