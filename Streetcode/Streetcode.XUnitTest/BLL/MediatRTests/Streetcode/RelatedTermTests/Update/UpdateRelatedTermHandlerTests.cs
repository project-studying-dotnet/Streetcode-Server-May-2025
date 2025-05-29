using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.RelatedTermTests.Update;

public class UpdateRelatedTermHandlerTests
{
    [Fact]
    public async Task Handle_WhenCalled_ShouldThrowNotImplementedException()
    {
        // Arrange
        var command = new UpdateRelatedTermCommand(1, new RelatedTermDTO());

        // Act
        var handler = new UpdateRelatedTermHandler();

        // Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => handler.Handle(command, CancellationToken.None));
    }
}