using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.RelatedTermTests.Update
{
    public class UpdateRelatedTermHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;

        public UpdateRelatedTermHandlerTests()
        {
            _mockMapper = new Mock<IMapper>();
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        }

        [Fact]
        public async Task Handle_WhenCalled_ShouldThrowNotImplementedException()
        {
            // Arrange
            var command = new UpdateRelatedTermCommand(1, new RelatedTermDTO());

            // Act
            var handler = new UpdateRelatedTermHandler(
                _mockMapper.Object,
                _mockRepositoryWrapper.Object);

            // Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => handler.Handle(command, CancellationToken.None));
        }


    }

}
