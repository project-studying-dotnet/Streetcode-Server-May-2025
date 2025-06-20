using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.GetPending;

public class GetPendingCommentsHandler : IRequestHandler<GetPendingCommentsQuery, Result<List<AdminCommentDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetPendingCommentsHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<AdminCommentDTO>>> Handle(GetPendingCommentsQuery request, CancellationToken cancellationToken)
    {
        var pendingComments = (await _repositoryWrapper.CommentRepository.GetAllAsync(
                predicate: c => !c.IsApproved,
                include: q => q
                    .Include(c => c.User)
                    .Include(c => c.Streetcode)));

        if (pendingComments is null)
        {
            const string errorMsg = "Cannot find any pending comments";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var sortedComments = pendingComments.OrderByDescending(c => c.CreatedAt).ToList();
        var commentDtos = _mapper.Map<List<AdminCommentDTO>>(sortedComments);

        return Result.Ok(commentDtos);
    }
}
