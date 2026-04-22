using MediatR;
using LabelsService.Application.DTOs;

public class CreateLabelCommand : IRequest<int>
{
    public CreateLabelDto Dto { get; set; }   
    public int UserId { get; set; }
}