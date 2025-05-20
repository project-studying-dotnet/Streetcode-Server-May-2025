using Xunit;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.DTO.Email;
using Streetcode.BLL.Interfaces.Email;
using Streetcode.BLL.MediatR.Email;
using Streetcode.DAL.Entities.AdditionalContent.Email;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using AutoMapper;
using FluentAssertions;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Email
{
    public class SendEmailHandlerTests
    {
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly SendEmailHandler _handler;

        public SendEmailHandlerTests()
        {
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new SendEmailHandler(_emailServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnOk_WhenEmailSentSuccessfully()
        {
            // Arrange
            SetupEmailService(true);

            var emailDto = new EmailDTO { From = "test@example.com", Content = "Hello" };
            var command = new SendEmailCommand(emailDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _emailServiceMock.Verify(es => es.SendEmailAsync(It.IsAny<Message>()), Times.Once);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenEmailServiceFails()
        {
            // Arrange
            SetupEmailService(false);

            var emailDto = new EmailDTO { From = "test@example.com", Content = "Hello" };
            var command = new SendEmailCommand(emailDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            _emailServiceMock.Verify(es => es.SendEmailAsync(It.IsAny<Message>()), Times.Once);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), It.Is<string>(s => s.Contains("Failed to send email"))), Times.Once);
        }

        private void SetupEmailService(bool shouldSucceed)
        {
            _emailServiceMock
                .Setup(es => es.SendEmailAsync(It.IsAny<Message>()))
                .ReturnsAsync(shouldSucceed);
        }
    }
}
