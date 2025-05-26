using Streetcode.BLL.DTO.News;

namespace Streetcode.BLL.Interfaces.News;

public interface INewsService
{
    Task<NewsDTO?> GetNewsByUrlAsync(string url);
    Task<NewsDTOWithURLs?> GetNewsWithURLsAsync(string url);
}