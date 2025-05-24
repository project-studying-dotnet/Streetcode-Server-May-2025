using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
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
            var art = await _repositoryWrapper.ArtRepository.GetFirstOrDefaultAsync(
                predicate: a => a.Id == request.Id,
                include: query => query.Include(a => a.Image));

            if (art == null)
            {
                string errorMsg = $"Cannot find an art with corresponding ID: {request.Id}";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            string? blobNameToDelete = art.Image?.BlobName;

            _repositoryWrapper.ArtRepository.Delete(art);
            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (!resultIsSuccess)
            {
                string errorMsg = $"Failed to delete an art with ID: {request.Id} from the database.";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            if (!string.IsNullOrWhiteSpace(blobNameToDelete))
            {
                try
                {
                    await _blobService.DeleteFileInStorageAsync(blobNameToDelete);
                    _logger.LogInformation($"Successfully deleted blob: {blobNameToDelete} for Art ID: {request.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(request, $"Failed to delete blob: {blobNameToDelete} for Art ID: {request.Id}. Error: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning($"Art ID: {request.Id} did not have an associated image blob name to delete.");
            }

            _logger.LogInformation($"DeleteArtCommand for Art ID: {request.Id} handled successfully (database entity deleted).");
            return Result.Ok(Unit.Value);
        }
    }
}
