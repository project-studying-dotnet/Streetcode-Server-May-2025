using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

using TermEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Term;

namespace Streetcode.BLL.MediatR.Streetcode.Term.Create;

public class CreateTermHandler : IRequestHandler<CreateTermCommand, Result<TermDTO>>
{
    private readonly IRepositoryWrapper _repository;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public CreateTermHandler(IRepositoryWrapper repository, IMapper mapper, ILoggerService logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TermDTO>> Handle(CreateTermCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<TermEntity>(request.Term);

        if (entity is null)
        {
            const string errorMsg = "Cannot map CreateTermRequest to entity.";
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        await _repository.TermRepository.CreateAsync(entity);
        var result = await _repository.SaveChangesAsync();

        if (result > 0)
        {
            var dto = _mapper.Map<TermDTO>(entity);
            return Result.Ok(dto);
        }

        const string failMsg = "Failed to save new Term.";
        _logger.LogError(request, failMsg);
        return Result.Fail(failMsg);
    }
}