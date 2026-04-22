namespace LabelsService.Domain.Entities;

public class Label
{
    public int Id { get; set; }
    public string Name { get; set; }   
    public int UserId { get; set; }    
    public DateTime CreatedAt { get; set; }
}