using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using LabelsService.Application.Features.Labels.Commands;
using LabelsService.Application.Interfaces;
using LabelsService.Application.DTOs;
using LabelsService.Domain.Entities;

[TestClass]
public class LabelsServiceTests
{
    [TestMethod]
    public async Task CreateLabel_Should_Return_Id()
    {
        var mockRepo = new Mock<ILabelRepository>();

        mockRepo.Setup(r => r.CreateLabel(It.IsAny<Label>()))
            .ReturnsAsync(1);

        var handler = new CreateLabelHandler(mockRepo.Object);

        var command = new CreateLabelCommand
        {
            UserId = 1,
            Dto = new CreateLabelDto("Work")
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.AreEqual(1, result);
    }
}