using FluentResults;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;

using Entity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact;

public class FactValidator
{
    private readonly ILoggerService _logger;

    public FactValidator(ILoggerService logger)
    {
        _logger = logger;
    }

    public Result? Validation(IRequest<Result<FactDTO>> request, Entity? newFact)
    {
        if (newFact is null)
        {
            const string errorMsg = "Invalid fact data provided. New Fact is null.";
            return LogAndFail(request, errorMsg);
        }

        if (newFact.StreetcodeId == 0)
        {
            const string errorMsg = "StreetcodeId is required.";
            return LogAndFail(request, errorMsg);
        }

        if (newFact.Title.IsNullOrEmpty())
        {
            const string errorMsg = "Заголовок факту є обов'язковим.";
            return LogAndFail(request, errorMsg);
        }

        if (newFact.Title.Length > 68)
        {
            const string errorMsg = "Заголовок факту не може перевищувати 68 символів.";
            return LogAndFail(request, errorMsg);
        }

        if (newFact.FactContent.IsNullOrEmpty())
        {
            const string errorMsg = "Основний текст факту є обов'язковим.";
            return LogAndFail(request, errorMsg);
        }

        if (newFact.FactContent.Length > 600)
        {
            const string errorMsg = "Основний текст факту не може перевищувати 600 символів.";
            return LogAndFail(request, errorMsg);
        }

        if (newFact.Image == null)
        {
            const string errorMsg = "Зображення є обов'язковим.";
            return LogAndFail(request, errorMsg);
        }

        if (newFact.ImageDescription?.Length > 200)
        {
            const string errorMsg = "Опис зображення не може перевищувати 200 символів.";
            return LogAndFail(request, errorMsg);
        }

        return null;
    }

    public Result LogAndFail(IRequest<Result<FactDTO>> request, string errorMsg)
    {
        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}