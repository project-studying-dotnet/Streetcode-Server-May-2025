using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetByFilter;

public class GetStreetcodeByFilterHandler : IRequestHandler<GetStreetcodeByFilterQuery, Result<List<StreetcodeFilterResultDTO>>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public GetStreetcodeByFilterHandler(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<Result<List<StreetcodeFilterResultDTO>>> Handle(GetStreetcodeByFilterQuery request, CancellationToken cancellationToken)
    {
        string searchQuery = request.Filter.SearchQuery;
        var results = new List<StreetcodeFilterResultDTO>();

        async Task CollectAsync(Func<string, Task<IEnumerable<StreetcodeFilterResultDTO>>> search) =>
            results.AddRange(await search(searchQuery));

        await CollectAsync(SearchStreetcodesAsync);
        await CollectAsync(SearchTextsAsync);
        await CollectAsync(SearchFactsAsync);
        await CollectAsync(SearchTimelineItemsAsync);
        await CollectAsync(SearchArtGalleryAsync);

        return results;
    }

    private async Task<IEnumerable<StreetcodeFilterResultDTO>> SearchStreetcodesAsync(string query)
    {
        var streetcodes = await _repositoryWrapper.StreetcodeRepository.GetAllAsync(
            predicate: sc => sc.Status == DAL.Enums.StreetcodeStatus.Published &&
                              (sc.Title.ContainsIgnoreCase(query) ||
                               sc.Alias.ContainsIgnoreCase(query) ||
                               sc.Teaser.ContainsIgnoreCase(query) ||
                               sc.TransliterationUrl.ContainsIgnoreCase(query)));

        return streetcodes.SelectMany(sc => MatchStreetcodeFields(sc, query));
    }

    private async Task<IEnumerable<StreetcodeFilterResultDTO>> SearchTextsAsync(string query)
    {
        var texts = await _repositoryWrapper.TextRepository.GetAllAsync(
            include: q => q.Include(t => (object) t.Streetcode!),
            predicate: t => t.Streetcode != null &&
                            t.Streetcode.Status == DAL.Enums.StreetcodeStatus.Published &&
                            (t.Title.ContainsIgnoreCase(query) ||
                             t.TextContent.ContainsIgnoreCase(query)));

        return texts.Select(t =>
        {
            string content = t.Title.ContainsIgnoreCase(query) ? t.Title! : t.TextContent ?? string.Empty;
            return CreateFilterResult(t.Streetcode!, content, "Текст", "text");
        });
    }

    private async Task<IEnumerable<StreetcodeFilterResultDTO>> SearchFactsAsync(string query)
    {
        var facts = await _repositoryWrapper.FactRepository.GetAllAsync(
            include: q => q.Include(f => (object) f.Streetcode!),
            predicate: f => f.Streetcode != null &&
                            f.Streetcode.Status == DAL.Enums.StreetcodeStatus.Published &&
                            (f.Title.ContainsIgnoreCase(query) ||
                             f.FactContent.ContainsIgnoreCase(query)));

        return facts.Select(f => CreateFilterResult(f.Streetcode!, f.Title ?? string.Empty, "Wow-факти", "wow-facts"));
    }

    private async Task<IEnumerable<StreetcodeFilterResultDTO>> SearchTimelineItemsAsync(string query)
    {
        var items = await _repositoryWrapper.TimelineRepository.GetAllAsync(
            include: q => q.Include(i => (object) i.Streetcode!),
            predicate: i => i.Streetcode != null &&
                            i.Streetcode.Status == DAL.Enums.StreetcodeStatus.Published &&
                            (i.Title.ContainsIgnoreCase(query) ||
                             i.Description.ContainsIgnoreCase(query)));

        return items.Select(i => CreateFilterResult(i.Streetcode!, i.Title ?? string.Empty, "Хронологія", "timeline"));
    }

    private async Task<IEnumerable<StreetcodeFilterResultDTO>> SearchArtGalleryAsync(string query)
    {
        var arts = await _repositoryWrapper.ArtRepository.GetAllAsync(
            include: q => q.Include(a => a.StreetcodeArts),
            predicate: a => a.Description.ContainsIgnoreCase(query) &&
                             a.StreetcodeArts.Any(link => link.Streetcode != null &&
                                                         link.Streetcode.Status == DAL.Enums.StreetcodeStatus.Published));

        return from art in arts
               from link in art.StreetcodeArts
               where link.Streetcode != null
               select CreateFilterResult(link.Streetcode, art.Description ?? string.Empty, "Арт-галерея", "art-gallery");
    }

    private static IEnumerable<StreetcodeFilterResultDTO> MatchStreetcodeFields(StreetcodeContent sc, string query)
    {
        if (sc.Title.ContainsIgnoreCase(query))
        {
            yield return CreateFilterResult(sc, sc.Title ?? string.Empty);
        }
        
        if (sc.Alias.ContainsIgnoreCase(query))
        {
            yield return CreateFilterResult(sc, sc.Alias ?? string.Empty);
        }
        
        if (sc.Teaser.ContainsIgnoreCase(query))
        {
            yield return CreateFilterResult(sc, sc.Teaser ?? string.Empty);
        }
        
        if (sc.TransliterationUrl.ContainsIgnoreCase(query))
        {
            yield return CreateFilterResult(sc, sc.TransliterationUrl ?? string.Empty);
        }
    }

    private static StreetcodeFilterResultDTO CreateFilterResult(StreetcodeContent streetcode, string content, string? sourceName = null, string? blockName = null) =>
        new()
        {
            StreetcodeId = streetcode.Id,
            StreetcodeTransliterationUrl = streetcode.TransliterationUrl ?? string.Empty,
            StreetcodeIndex = streetcode.Index,
            BlockName = blockName ?? string.Empty,
            Content = content,
            SourceName = sourceName ?? string.Empty,
        };
}

internal static class StringExtensions
{
    public static bool ContainsIgnoreCase(this string? source, string value) =>
        !string.IsNullOrEmpty(source) &&
        source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
}