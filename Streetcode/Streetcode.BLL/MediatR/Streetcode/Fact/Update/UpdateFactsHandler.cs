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

    public UpdateFactsHandler(ILoggerService loggerService, IMapper mapper, IRepositoryWrapper repositoryWrapper)
    {
        _logger = loggerService;
        _mapper = mapper;
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<Result<FactDTO>> Handle(UpdateFactsCommand request, CancellationToken cancellationToken)
    {
        var factToUpdate =
            await _repositoryWrapper.FactRepository.GetFirstOrDefaultAsync(x => x.Id == request.FactDTO.Id);

        if (factToUpdate == null)
        {
            const string errorMsg = "Fact by this id does not exist";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        factToUpdate.Title = request.FactDTO.Title;
        factToUpdate.ImageId = request.FactDTO.ImageId;
        factToUpdate.FactContent = request.FactDTO.FactContent;
        factToUpdate.StreetcodeId = request.FactDTO.StreetcodeId;

        _repositoryWrapper.FactRepository.Update(factToUpdate);
        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (!resultIsSuccess)
        {
            const string errorMsg = "Failed to update facts";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        return Result.Ok(_mapper.Map<FactDTO>(factToUpdate));
    }
}