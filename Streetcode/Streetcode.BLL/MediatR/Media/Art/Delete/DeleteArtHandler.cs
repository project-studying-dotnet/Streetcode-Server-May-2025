using MediatR;
using FluentResults;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Media.Art.Delete
{
    public class DeleteArtHandler : IRequestHandler<DeleteArtCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IBlobService _blobService;
        private readonly ILoggerService _logger;

        public DeleteArtHandler(IRepositoryWrapper repositoryWrapper, IBlobService blobService, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _blobService = blobService;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(DeleteArtCommand request, CancellationToken cancellationToken)
        {
            var art = await _repositoryWrapper.ArtRepository.GetFirstOrDefaultAsync(a => a.Id == request.Id);

            if (art is null)
            {
                string errorMsg = $"Cannot find an art with corresponding categoryId: {request.Id}";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _repositoryWrapper.ArtRepository.Delete(art);

            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                await _blobService.DeleteFileInStorageAsync(art.Title);
            }

            if (resultIsSuccess)
            {
                _logger?.LogInformation($"DeleteArtCommand handled successfully");
                return Result.Ok(Unit.Value);
            }
            else
            {
                string errorMsg = $"Failed to delete an art";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }
    }
}
