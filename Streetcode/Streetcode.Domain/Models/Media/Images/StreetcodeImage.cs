namespace Streetcode.Domain.Models.Media.Images;

public class StreetcodeImage
{
    public int StreetcodeId { get; set; }

    public int ImageId { get; set; }

    public Image? Image { get; set; }

    public StreetcodeContent? Streetcode { get; set; }
}