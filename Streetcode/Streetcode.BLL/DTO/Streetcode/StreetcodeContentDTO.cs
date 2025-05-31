using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Entities.Transactions;
using Streetcode.DAL.Enums;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.DTO.Analytics;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Toponyms;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.DTO.Media;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.DTO.Streetcode.RelatedFigure;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.DTO.Media.Art;

namespace Streetcode.BLL.DTO.Streetcode;

public class StreetcodeContentDTO
{
    public int Id { get; set; }
    public int Index { get; set; }
    public string? Teaser { get; set; }
    public string? DateString { get; set; }
    public string? Alias { get; set; }

    public StreetcodeStatus Status { get; set; }
    public string? Title { get; set; }
    public string? TransliterationUrl { get; set; }

    public int ViewCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime EventStartOrPersonBirthDate { get; set; }

    public DateTime? EventEndOrPersonDeathDate { get; set; }

    public int? AudioId { get; set; }

    public TextDTO? Text { get; set; }

    public AudioDTO? Audio { get; set; }

    public List<StatisticRecordDTO> StatisticRecords { get; set; } = new();

    public List<StreetcodeCoordinateDTO> Coordinates { get; set; } = new();

    public TransactionLink? TransactionLink { get; set; }

    public List<ToponymDTO> Toponyms { get; set; } = new();

    public List<ImageDTO> Images { get; set; } = new();

    public List<StreetcodeTagIndex> StreetcodeTagIndices { get; set; } = new();

    public List<TagDTO> Tags { get; set; } = new();

    public List<SubtitleDTO> Subtitles { get; set; } = new();

    public List<FactDTO> Facts { get; set; } = new();

    public List<VideoDTO> Videos { get; set; } = new();

    public List<SourceLinkCategoryDTO> SourceLinkCategories { get; set; } = new();

    public List<TimelineItemDTO> TimelineItems { get; set; } = new();

    public List<RelatedFigureDTO> Observers { get; set; } = new();

    public List<RelatedFigureDTO> Targets { get; set; } = new();

    public List<PartnerDTO> Partners { get; set; } = new();

    public List<StreetcodeArtDTO> StreetcodeArts { get; set; } = new();

    public List<StreetcodeCategoryContentDTO> StreetcodeCategoryContents { get; set; } = new();
}
