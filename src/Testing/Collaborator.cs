using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CollaboratorService.Application.Features.Collaborators.Commands.AddCollaborator;
using CollaboratorService.Application.Interfaces;
using CollaboratorService.Domain.Entities;
using CollaboratorService.Application.DTOs;

[TestClass]
public class CollaboratorServiceTests
{
    [TestMethod]
    public async Task AddCollaborator_Should_Return_True()
    {
        var mockRepo = new Mock<ICollaboratorRepository>();

        mockRepo.Setup(r => r.AddAsync(It.IsAny<Collaborator>()))
            .Returns(Task.CompletedTask);

        var handler = new AddCollaboratorHandler(mockRepo.Object);
        
        var command = new AddCollaboratorCommand(
            1,
            new AddCollaboratorDto("123", 2)
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result);
    }
}