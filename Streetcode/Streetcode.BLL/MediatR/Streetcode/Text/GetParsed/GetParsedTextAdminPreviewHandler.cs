﻿using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.GetParsed;

public class GetParsedTextAdminPreviewHandler : IRequestHandler<GetParsedTextForAdminPreviewQuery, Result<string>>
{
    private readonly ITextService _textService;

    public GetParsedTextAdminPreviewHandler(ITextService textService)
    {
        _textService = textService;
    }

    public async Task<Result<string>> Handle(GetParsedTextForAdminPreviewQuery request, CancellationToken cancellationToken)
    {
        string? parsedText = await _textService.AddTermsTag(request.textToParse);
        return parsedText == null ? Result.Fail(new Error("text was not parsed successfully")) : Result.Ok(parsedText);
    }
}