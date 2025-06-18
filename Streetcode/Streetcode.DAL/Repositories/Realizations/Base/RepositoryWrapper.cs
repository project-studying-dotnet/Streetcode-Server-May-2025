using System.Transactions;
using Repositories.Interfaces;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Analytics;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Newss;
using Streetcode.DAL.Repositories.Interfaces.Partners;
using Streetcode.DAL.Repositories.Interfaces.Source;
using Streetcode.DAL.Repositories.Interfaces.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Team;
using Streetcode.DAL.Repositories.Interfaces.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Toponyms;
using Streetcode.DAL.Repositories.Interfaces.Transactions;
using Streetcode.DAL.Repositories.Interfaces.Users;
using Streetcode.DAL.Repositories.Realizations.AdditionalContent;
using Streetcode.DAL.Repositories.Realizations.Analytics;
using Streetcode.DAL.Repositories.Realizations.Media;
using Streetcode.DAL.Repositories.Realizations.Media.Images;
using Streetcode.DAL.Repositories.Realizations.Newss;
using Streetcode.DAL.Repositories.Realizations.Partners;
using Streetcode.DAL.Repositories.Realizations.Source;
using Streetcode.DAL.Repositories.Realizations.Streetcode;
using Streetcode.DAL.Repositories.Realizations.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Realizations.Team;
using Streetcode.DAL.Repositories.Realizations.Timeline;
using Streetcode.DAL.Repositories.Realizations.Toponyms;
using Streetcode.DAL.Repositories.Realizations.Transactions;
using Streetcode.DAL.Repositories.Realizations.Users;

namespace Streetcode.DAL.Repositories.Realizations.Base;

public class RepositoryWrapper : IRepositoryWrapper
{
    private readonly StreetcodeDbContext _streetcodeDbContext;

    private readonly Lazy<IArtRepository> _artRepository;
    private readonly Lazy<IAudioRepository> _audioRepository;
    private readonly Lazy<ICommentRepository> _commentRepository;
    private readonly Lazy<IFactRepository> _factRepository;
    private readonly Lazy<IHistoricalContextRepository> _historyContextRepository;
    private readonly Lazy<IHistoricalContextTimelineRepository> _historicalContextTimelineRepository;
    private readonly Lazy<IImageDetailsRepository> _imageDetailsRepository;
    private readonly Lazy<IImageRepository> _imageRepository;
    private readonly Lazy<INewsRepository> _newsRepository;
    private readonly Lazy<IPartnerSourceLinkRepository> _partnerSourceLinkRepository;
    private readonly Lazy<IPartnerStreetcodeRepository> _partnerStreetcodeRepository;
    private readonly Lazy<IPartnersRepository> _partnersRepository;
    private readonly Lazy<IPositionRepository> _positionRepository;
    private readonly Lazy<IRelatedFigureRepository> _relatedFigureRepository;
    private readonly Lazy<IRelatedTermRepository> _relatedTermRepository;
    private readonly Lazy<ISourceCategoryRepository> _sourceCategoryRepository;
    private readonly Lazy<IStatisticRecordRepository> _statisticRecordRepository;
    private readonly Lazy<IStreetcodeArtRepository> _streetcodeArtRepository;
    private readonly Lazy<IStreetcodeCategoryContentRepository> _streetcodeCategoryContentRepository;
    private readonly Lazy<IStreetcodeCoordinateRepository> _streetcodeCoordinateRepository;
    private readonly Lazy<IStreetcodeImageRepository> _streetcodeImageRepository;
    private readonly Lazy<IStreetcodeRepository> _streetcodeRepository;
    private readonly Lazy<IStreetcodeTagIndexRepository> _streetcodeTagIndexRepository;
    private readonly Lazy<IStreetcodeToponymRepository> _streetcodeToponymRepository;
    private readonly Lazy<ISubtitleRepository> _subtitleRepository;
    private readonly Lazy<ITagRepository> _tagRepository;
    private readonly Lazy<ITeamLinkRepository> _teamLinkRepository;
    private readonly Lazy<ITeamPositionRepository> _teamPositionRepository;
    private readonly Lazy<ITeamRepository> _teamRepository;
    private readonly Lazy<ITermRepository> _termRepository;
    private readonly Lazy<ITextRepository> _textRepository;
    private readonly Lazy<ITimelineRepository> _timelineRepository;
    private readonly Lazy<IToponymRepository> _toponymRepository;
    private readonly Lazy<ITransactLinksRepository> _transactLinksRepository;
    private readonly Lazy<IUserRepository> _userRepository;
    private readonly Lazy<IVideoRepository> _videoRepository;

    public RepositoryWrapper(StreetcodeDbContext streetcodeDbContext)
    {
        _streetcodeDbContext = streetcodeDbContext;
        _artRepository = new Lazy<IArtRepository>(() => new ArtRepository(_streetcodeDbContext));
        _audioRepository = new Lazy<IAudioRepository>(() => new AudioRepository(_streetcodeDbContext));
        _commentRepository = new Lazy<ICommentRepository>(() => new CommentRepository(_streetcodeDbContext));
        _factRepository = new Lazy<IFactRepository>(() => new FactRepository(_streetcodeDbContext));
        _historyContextRepository = new Lazy<IHistoricalContextRepository>(() => new HistoricalContextRepository(_streetcodeDbContext));
        _historicalContextTimelineRepository = new Lazy<IHistoricalContextTimelineRepository>(() => new HistoricalContextTimelineRepository(_streetcodeDbContext));
        _imageDetailsRepository = new Lazy<IImageDetailsRepository>(() => new ImageDetailsRepository(_streetcodeDbContext));
        _imageRepository = new Lazy<IImageRepository>(() => new ImageRepository(_streetcodeDbContext));
        _newsRepository = new Lazy<INewsRepository>(() => new NewsRepository(_streetcodeDbContext));
        _partnerSourceLinkRepository = new Lazy<IPartnerSourceLinkRepository>(() => new PartnerSourceLinksRepository(_streetcodeDbContext));
        _partnerStreetcodeRepository = new Lazy<IPartnerStreetcodeRepository>(() => new PartnerStreetcodeRepository(_streetcodeDbContext));
        _partnersRepository = new Lazy<IPartnersRepository>(() => new PartnersRepository(_streetcodeDbContext));
        _positionRepository = new Lazy<IPositionRepository>(() => new PositionRepository(_streetcodeDbContext));
        _relatedFigureRepository = new Lazy<IRelatedFigureRepository>(() => new RelatedFigureRepository(_streetcodeDbContext));
        _relatedTermRepository = new Lazy<IRelatedTermRepository>(() => new RelatedTermRepository(_streetcodeDbContext));
        _sourceCategoryRepository = new Lazy<ISourceCategoryRepository>(() => new SourceCategoryRepository(_streetcodeDbContext));
        _statisticRecordRepository = new Lazy<IStatisticRecordRepository>(() => new StatisticRecordRepository(_streetcodeDbContext));
        _streetcodeArtRepository = new Lazy<IStreetcodeArtRepository>(() => new StreetcodeArtRepository(_streetcodeDbContext));
        _streetcodeCategoryContentRepository = new Lazy<IStreetcodeCategoryContentRepository>(() => new StreetcodeCategoryContentRepository(_streetcodeDbContext));
        _streetcodeCoordinateRepository = new Lazy<IStreetcodeCoordinateRepository>(() => new StreetcodeCoordinateRepository(_streetcodeDbContext));
        _streetcodeImageRepository = new Lazy<IStreetcodeImageRepository>(() => new StreetcodeImageRepository(_streetcodeDbContext));
        _streetcodeRepository = new Lazy<IStreetcodeRepository>(() => new StreetcodeRepository(_streetcodeDbContext));
        _streetcodeTagIndexRepository = new Lazy<IStreetcodeTagIndexRepository>(() => new StreetcodeTagIndexRepository(_streetcodeDbContext));
        _streetcodeToponymRepository = new Lazy<IStreetcodeToponymRepository>(() => new StreetcodeToponymRepository(_streetcodeDbContext));
        _subtitleRepository = new Lazy<ISubtitleRepository>(() => new SubtitleRepository(_streetcodeDbContext));
        _tagRepository = new Lazy<ITagRepository>(() => new TagRepository(_streetcodeDbContext));
        _teamLinkRepository = new Lazy<ITeamLinkRepository>(() => new TeamLinkRepository(_streetcodeDbContext));
        _teamPositionRepository = new Lazy<ITeamPositionRepository>(() => new TeamPositionRepository(_streetcodeDbContext));
        _teamRepository = new Lazy<ITeamRepository>(() => new TeamRepository(_streetcodeDbContext));
        _termRepository = new Lazy<ITermRepository>(() => new TermRepository(_streetcodeDbContext));
        _textRepository = new Lazy<ITextRepository>(() => new TextRepository(_streetcodeDbContext));
        _timelineRepository = new Lazy<ITimelineRepository>(() => new TimelineRepository(_streetcodeDbContext));
        _toponymRepository = new Lazy<IToponymRepository>(() => new ToponymRepository(_streetcodeDbContext));
        _transactLinksRepository = new Lazy<ITransactLinksRepository>(() => new TransactLinksRepository(_streetcodeDbContext));
        _userRepository = new Lazy<IUserRepository>(() => new UserRepository(_streetcodeDbContext));
        _videoRepository = new Lazy<IVideoRepository>(() => new VideoRepository(_streetcodeDbContext));
    }

    public IArtRepository ArtRepository => _artRepository.Value;
    public IAudioRepository AudioRepository => _audioRepository.Value;
    public ICommentRepository CommentRepository => _commentRepository.Value;
    public IFactRepository FactRepository => _factRepository.Value;
    public IHistoricalContextRepository HistoricalContextRepository => _historyContextRepository.Value;
    public IHistoricalContextTimelineRepository HistoricalContextTimelineRepository => _historicalContextTimelineRepository.Value;
    public IImageRepository ImageRepository => _imageRepository.Value;
    public INewsRepository NewsRepository => _newsRepository.Value;
    public IPartnersRepository PartnersRepository => _partnersRepository.Value;
    public ITeamRepository TeamRepository => _teamRepository.Value;
    public ITeamPositionRepository TeamPositionRepository => _teamPositionRepository.Value;
    public IStreetcodeCoordinateRepository StreetcodeCoordinateRepository => _streetcodeCoordinateRepository.Value;
    public IVideoRepository VideoRepository => _videoRepository.Value;
    public IStreetcodeArtRepository StreetcodeArtRepository => _streetcodeArtRepository.Value;
    public ISourceCategoryRepository SourceCategoryRepository => _sourceCategoryRepository.Value;
    public IStreetcodeCategoryContentRepository StreetcodeCategoryContentRepository => _streetcodeCategoryContentRepository.Value;
    public IRelatedFigureRepository RelatedFigureRepository => _relatedFigureRepository.Value;
    public IStreetcodeRepository StreetcodeRepository => _streetcodeRepository.Value;
    public ISubtitleRepository SubtitleRepository => _subtitleRepository.Value;
    public IStatisticRecordRepository StatisticRecordRepository => _statisticRecordRepository.Value;
    public ITagRepository TagRepository => _tagRepository.Value;
    public ITermRepository TermRepository => _termRepository.Value;
    public ITextRepository TextRepository => _textRepository.Value;
    public ITimelineRepository TimelineRepository => _timelineRepository.Value;
    public IToponymRepository ToponymRepository => _toponymRepository.Value;
    public ITransactLinksRepository TransactLinksRepository => _transactLinksRepository.Value;
    public IPartnerSourceLinkRepository PartnerSourceLinkRepository => _partnerSourceLinkRepository.Value;
    public IRelatedTermRepository RelatedTermRepository => _relatedTermRepository.Value;
    public IUserRepository UserRepository => _userRepository.Value;
    public IStreetcodeTagIndexRepository StreetcodeTagIndexRepository => _streetcodeTagIndexRepository.Value;
    public IPartnerStreetcodeRepository PartnerStreetcodeRepository => _partnerStreetcodeRepository.Value;
    public IPositionRepository PositionRepository => _positionRepository.Value;
    public ITeamLinkRepository TeamLinkRepository => _teamLinkRepository.Value;
    public IImageDetailsRepository ImageDetailsRepository => _imageDetailsRepository.Value;
    public IStreetcodeToponymRepository StreetcodeToponymRepository => _streetcodeToponymRepository.Value;
    public IStreetcodeImageRepository StreetcodeImageRepository => _streetcodeImageRepository.Value;

    public int SaveChanges()
    {
        return _streetcodeDbContext.SaveChanges();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _streetcodeDbContext.SaveChangesAsync();
    }

    public TransactionScope BeginTransaction()
    {
        return new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
    }
}
