using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UserService.Application.Features.Auth.Commands.Register;
using UserService.Application.DTOs;
using UserService.Domain.Entities;

[TestClass]
public class UserServiceTests
{
    [TestMethod]
    public async Task Register_Should_Create_User()
    {
        var mockRepo = new Mock<IUserRepository>();

        mockRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        mockRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var handler = new RegisterHandler(mockRepo.Object);

        var command = new RegisterCommand(
            new RegisterDto("John", "Doe", "john@test.com", "123456")
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result);
    }
}