using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Update;

public class UpdateCommentHandler : IRequestHandler<UpdateCommentCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public UpdateCommentHandler(IRepositoryWrapper repositoryWrapper,
        ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(UpdateCommentCommand request,
        CancellationToken cancellationToken)
    {
        var updatedCommentDto = request.UpdatedComment;
        var targetCommentId = updatedCommentDto.Id;

        var commentEntity = await _repositoryWrapper.CommentRepository
            .GetFirstOrDefaultAsync(c => c.Id == targetCommentId);
        if (commentEntity is null)
        {
            var errorMsg = $"Comment update failed: no comment found with ID '{targetCommentId}'.";
            _logger.LogError(request, errorMsg);

            return Result.Fail(errorMsg);
        }

        commentEntity.Text = updatedCommentDto.Text;
        commentEntity.UpdatedAt = DateTime.UtcNow;

        _repositoryWrapper.CommentRepository.Update(commentEntity);

        var changesSaved = await _repositoryWrapper.SaveChangesAsync() > 0;
        if (!changesSaved)
        {
            var errorMsg = $"Comment update failed: unable to persist changes for comment ID '{targetCommentId}'.";
            _logger.LogError(request, errorMsg);

            return Result.Fail(new Error(errorMsg));
        }

        return Result.Ok(Unit.Value);
    }
}