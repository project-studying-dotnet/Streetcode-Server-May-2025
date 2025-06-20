using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;

public class GetCommentsByStreetcodeIdHandler : IRequestHandler<GetCommentsByStreetcodeIdQuery, Result<List<CommentDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;

    public GetCommentsByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
    }

    public async Task<Result<List<CommentDTO>>> Handle(GetCommentsByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var comments = await _repositoryWrapper.CommentRepository.GetAllAsync(
            predicate: c => c.StreetcodeId == request.StreetcodeId && (request.ForModeration || c.IsApproved),
            include: q => q
                .Include(c => c.User)
                .Include(c => c.Replies));

        var rootComments = comments.Where(c => c.ParentCommentId == null).ToList();
        var commentDtos = _mapper.Map<List<CommentDTO>>(rootComments);

        return Result.Ok(commentDtos);
    }
} 