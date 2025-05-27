using Moq;
using Xunit;
using Streetcode.BLL.Interfaces.Text;
using Streetcode.BLL.MediatR.Streetcode.Text.GetParsed;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Text.GetParsed;

public class GetParsedTextAdminPreviewHandlerTests
{
    private readonly Mock<ITextService> _textServiceMock;
    private readonly GetParsedTextAdminPreviewHandler _handler;

    public GetParsedTextAdminPreviewHandlerTests()
    {
        _textServiceMock = new Mock<ITextService>();
        _handler = new GetParsedTextAdminPreviewHandler(_textServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenParsingSucceeds()
    {
        // Arrange
        var (rawText, parsedText) = CreateValidTextPair();
        SetupMocksForParsing(parsedText);

        // Act
        var result = await _handler.Handle(
            new GetParsedTextForAdminPreviewQuery(rawText),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        VerifyMocksCalledOnce(rawText);
    }

    [Fact]
    public async Task Handle_ShouldReturnParsedText_WhenParsingSucceeds()
    {
        // Arrange
        var (rawText, parsedText) = CreateValidTextPair();
        SetupMocksForParsing(parsedText);

        // Act
        var result = await _handler.Handle(
            new GetParsedTextForAdminPreviewQuery(rawText),
            CancellationToken.None);

        // Assert
        Assert.Equal(parsedText, result.Value);
        VerifyMocksCalledOnce(rawText);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailedResult_WhenParsingFails()
    {
        // Arrange
        const string rawText = "<p>sample text</p>";
        SetupMocksForParsing(null);

        // Act
        var result = await _handler.Handle(
            new GetParsedTextForAdminPreviewQuery(rawText),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        VerifyMocksCalledOnce(rawText);
    }

    private static (string rawText, string parsedText) CreateValidTextPair()
    {
        const string rawText = "<p>sample text</p>";
        const string parsedText = "<p><Popover><Term>sample</Term><Desc>description</Desc></Popover> text</p>";

        return (rawText, parsedText);
    }

    private void SetupMocksForParsing(string? parsedText)
    {
        _textServiceMock
            .Setup(s => s.AddTermsTag(It.IsAny<string>()))
            .ReturnsAsync(() => parsedText!);
    }

    private void VerifyMocksCalledOnce(string rawText)
    {
        _textServiceMock.Verify(
            s => s.AddTermsTag(rawText),
            Times.Once);
    }
}