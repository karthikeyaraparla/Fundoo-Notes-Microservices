using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NotesService.Application.Features.Notes.Commands.CreateNote;
using NotesService.Application.Interfaces;
using NotesService.Application.DTOs;
using NotesService.Domain.Entities;

[TestClass]
public class NotesServiceTests
{
    [TestMethod]
    public async Task CreateNote_Should_Return_Id()
    {
        var mockRepo = new Mock<INoteRepository>();
        var mockCache = new Mock<ICacheService>();

        mockRepo.Setup(r => r.CreateAsync(It.IsAny<Note>()))
            .ReturnsAsync(1);

        mockCache.Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateNoteHandler(mockRepo.Object, mockCache.Object);

        var command = new CreateNoteCommand(
            1,
            new CreateNoteDto("Title", "Description", "blue")
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.AreEqual(1, result);
    }
}