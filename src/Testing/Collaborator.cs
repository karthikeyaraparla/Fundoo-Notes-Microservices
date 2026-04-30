using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CollaboratorService.Application.Features.Collaborators.Commands.AddCollaborator;
using CollaboratorService.Application.Interfaces;
using CollaboratorService.Application.Options;
using CollaboratorService.Domain.Entities;
using CollaboratorService.Application.DTOs;
using Dapr.Client;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

[TestClass]
public class CollaboratorServiceTests
{
    [TestMethod]
    public async Task AddCollaborator_Should_Return_True()
    {
        var mockRepo = new Mock<ICollaboratorRepository>();
        var mockDaprClient = new Mock<DaprClient>();

        mockRepo.Setup(r => r.AddAsync(It.IsAny<Collaborator>()))
            .Returns(Task.CompletedTask);

        var handler = new AddCollaboratorHandler(
            mockRepo.Object,
            mockDaprClient.Object,
            Options.Create(new CollaboratorInvitationPubSubOptions()),
            NullLogger<AddCollaboratorHandler>.Instance);
        
        var command = new AddCollaboratorCommand(
            1,
            new AddCollaboratorDto(Convert.ToInt32("123"), 2, "collaborator@example.com")
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(result);
        mockRepo.Verify(r => r.AddAsync(It.Is<Collaborator>(c =>
            c.NoteId == 123 &&
            c.OwnerUserId == 1 &&
            c.CollaboratorUserId == 2)), Times.Once);
    }
}
