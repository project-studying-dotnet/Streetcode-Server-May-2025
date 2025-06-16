using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode.Comment;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.GetById;

public class GetCommentByIdHandler : IRequestHandler<GetCommentByIdQuery, Result<AdminCommentDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetCommentByIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<AdminCommentDTO>> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        var comment = await _repositoryWrapper.CommentRepository.GetFirstOrDefaultAsync(
            predicate: c => c.Id == request.Id,
            include: q => q
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User));

        if (comment is null)
        {
            string errorMsg = $"Cannot find comment with id: {request.Id}";
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        var commentDto = _mapper.Map<AdminCommentDTO>(comment);
        return Result.Ok(commentDto);
    }
} 