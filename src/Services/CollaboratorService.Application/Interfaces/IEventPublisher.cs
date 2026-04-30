namespace CollaboratorService.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T message);
}