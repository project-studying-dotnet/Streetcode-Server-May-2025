using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Update;

public class UpdateFactsHandler : IRequestHandler<UpdateFactsCommand, Result<FactDTO>>
{
    private readonly ILoggerService _logger;
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly FactValidator _validator;

    public UpdateFactsHandler(ILoggerService loggerService, IMapper mapper, IRepositoryWrapper repositoryWrapper)
    {
        _logger = loggerService;
        _mapper = mapper;
        _repositoryWrapper = repositoryWrapper;
        _validator = new FactValidator(_logger);
    }

    public async Task<Result<FactDTO>> Handle(UpdateFactsCommand request, CancellationToken cancellationToken)
    {
        var fact = _mapper.Map<DAL.Entities.Streetcode.TextContent.Fact>(request.FactDTO);

        if (fact == null)
        {
            const string errorMsg = "Cannot convert null to Fact";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var validation = _validator.Validation(request, fact);

        if (validation != null)
        {
            return validation;
        }

        _repositoryWrapper.FactRepository.Update(fact);
        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (!resultIsSuccess)
        {
            const string errorMsg = "Failed to update facts";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        return Result.Ok(_mapper.Map<FactDTO>(fact));
    }
}