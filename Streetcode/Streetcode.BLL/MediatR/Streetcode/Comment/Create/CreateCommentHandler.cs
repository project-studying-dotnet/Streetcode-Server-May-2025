using AutoMapper;
using FluentResults;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.DTO.Streetcode;

using CommentEntity = Streetcode.DAL.Entities.Streetcode.Comment;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Create;

public class CreateCommentHandler
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public CreateCommentHandler(
        IRepositoryWrapper repositoryWrapper,
        IMapper mapper,
        ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CommentDTO>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = _mapper.Map<CommentEntity>(request.newComment);

        if(comment is null)
        {
            const string errorMsg = "Cannot create new comment for a streetcode!";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var createdComment = await _repositoryWrapper.CommentRepository.CreateAsync(comment);

        var isSuccessResult = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (!isSuccessResult)
        {
            const string errorMsg = "Cannot save changes in the database after comment creation!";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var createdCommentDto = _mapper.Map<CommentDTO>(createdComment);

        if (createdCommentDto != null)
        {
            return Result.Ok(createdCommentDto);
        }
        else
        {
            const string errorMsg = "Cannot map entity!";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
