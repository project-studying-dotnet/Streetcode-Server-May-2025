using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Threading;
using System.Threading.Tasks;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Approve;

public class ApproveCommentHandler : IRequestHandler<ApproveCommentCommand, Result>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public ApproveCommentHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result> Handle(ApproveCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repositoryWrapper.CommentRepository.GetFirstOrDefaultAsync(c => c.Id == request.CommentId);
        if (comment is null)
        {
            string errorMsg = $"Cannot find comment with id: {request.CommentId}";
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }
        comment.IsApproved = true;
        await _repositoryWrapper.CommentRepository.UpdateAsync(comment);
        await _repositoryWrapper.SaveChangesAsync();
        return Result.Ok();
    }
} 