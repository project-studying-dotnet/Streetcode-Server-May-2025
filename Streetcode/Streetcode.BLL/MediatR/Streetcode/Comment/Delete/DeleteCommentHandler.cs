using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Delete;

public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public DeleteCommentHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repositoryWrapper.CommentRepository.GetFirstOrDefaultAsync(c => c.Id == request.Id);
        if (comment is null)
        {
            string errorMsg = $"Cannot find comment with id: {request.Id}";
            _logger.LogError(request, errorMsg);
            return Result.Fail<Unit>(errorMsg);
        }

        _repositoryWrapper.CommentRepository.Delete(comment);
        await _repositoryWrapper.SaveChangesAsync();

        return Result.Ok(Unit.Value);
    }
}