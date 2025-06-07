using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

using Entity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Create;

public class CreateTextHandler : IRequestHandler<CreateTextCommand, Result<TextDTO>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public CreateTextHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TextDTO>> Handle(CreateTextCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Entity>(request.CreateTextRequest);

        if (entity == null)
        {
            const string errorMsg = "Cannot map CreateTextRequest to entity.";
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        await _repositoryWrapper.TextRepository.CreateAsync(entity);
        var saveResult = await _repositoryWrapper.SaveChangesAsync();

        if (saveResult > 0)
        {
            return Result.Ok(_mapper.Map<TextDTO>(entity));
        }

        const string failMsg = "Failed to save new Text.";
        _logger.LogError(request, failMsg);
        return Result.Fail(failMsg);
    }
}