using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.News;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.Services.News;

public class NewsService : INewsService
{
    private readonly IBlobService _blobService;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;

    public NewsService(
        IBlobService blobService,
        IRepositoryWrapper repositoryWrapper,
        IMapper mapper)
    {
        _blobService = blobService;
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
    }

    public async Task<NewsDTO?> GetNewsByUrlAsync(string url)
    {
        var newsDTO = _mapper.Map<NewsDTO>(await _repositoryWrapper.NewsRepository.GetFirstOrDefaultAsync(
            predicate: sc => sc.URL == url,
            include: scl => scl
                .Include(sc => sc.Image)));

        if(newsDTO is null)
        {
            return null;
        }

        if (newsDTO.Image is not null)
        {
            newsDTO.Image.Base64 = _blobService.FindFileInStorageAsBase64(newsDTO.Image.BlobName);
        }

        return newsDTO;
    }

    public async Task<NewsDTOWithURLs?> GetNewsWithURLsAsync(string url)
    {
        var newsDTO = await GetNewsByUrlAsync(url);

        if(newsDTO is null)
        {
            return null;
        }

        var news = (await _repositoryWrapper.NewsRepository.GetAllAsync()).ToList();
        var newsIndex = news.FindIndex(x => x.Id == newsDTO.Id);

        var prevNewsLink = await GetPrevNewsLink(news, newsIndex);
        var nextNewsLink = await GetNextNewsLink(news, newsIndex);
        RandomNewsDTO randomNewsTitleAndLink = await GetRandomNewsDTO(news, newsIndex);

        return new NewsDTOWithURLs
        {
            RandomNews = randomNewsTitleAndLink,
            News = newsDTO,
            NextNewsUrl = nextNewsLink,
            PrevNewsUrl = prevNewsLink
        };
    }

    private async Task<string?> GetPrevNewsLink(List<DAL.Entities.News.News> news, int newsIndex)
    {
        if (newsIndex != 0)
        {
            return news[newsIndex - 1].URL;
        }

        return null;
    }

    private async Task<string?> GetNextNewsLink(List<DAL.Entities.News.News> news, int newsIndex)
    {
        if(newsIndex != news.Count - 1)
        {
            return news[newsIndex + 1].URL;
        }

        return null;
    }

    private async Task<RandomNewsDTO> GetRandomNewsDTO(List<DAL.Entities.News.News> news, int newsIndex)
    {
        var randomNewsDTO = new RandomNewsDTO();

        var arrCount = news.Count;
        if (arrCount > 3)
        {
            if (newsIndex + 1 == arrCount - 1 || newsIndex == arrCount - 1)
            {
                randomNewsDTO.RandomNewsUrl = news[newsIndex - 2].URL;
                randomNewsDTO.Title = news[newsIndex - 2].Title;
            }
            else
            {
                randomNewsDTO.RandomNewsUrl = news[arrCount - 1].URL;
                randomNewsDTO.Title = news[arrCount - 1].Title;
            }
        }
        else
        {
            randomNewsDTO.RandomNewsUrl = news[newsIndex].URL;
            randomNewsDTO.Title = news[newsIndex].Title;
        }

        return randomNewsDTO;
    }
}