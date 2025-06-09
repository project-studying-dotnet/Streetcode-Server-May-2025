using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Delete;

public class DeleteCommentHandlerI : IRequestHandler<DeleteCommentCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public DeleteCommentHandlerI(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var commentId = request.CommentId;

        var comment = await _repositoryWrapper.CommentRepository.GetFirstOrDefaultAsync(c => c.Id == commentId);
        if (comment is null)
        {
            var errorMsg = $"Comment deletion failed: no comment found with ID '{commentId}'.";
            _logger.LogError(request, errorMsg);

            return Result.Fail(errorMsg);
        }

        _repositoryWrapper.CommentRepository.Delete(comment);

        var changesSaved = await _repositoryWrapper.SaveChangesAsync() > 0;
        if (!changesSaved)
        {
            var errorMsg = $"Comment deletion failed: unable to persist deletion for comment ID '{commentId}'.";
            _logger.LogError(request, errorMsg);

            return Result.Fail(new Error(errorMsg));
        }

        return Result.Ok(Unit.Value);
    }
}