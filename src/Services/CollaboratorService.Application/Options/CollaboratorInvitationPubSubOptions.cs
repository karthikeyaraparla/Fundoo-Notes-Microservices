namespace CollaboratorService.Application.Options;

public class CollaboratorInvitationPubSubOptions
{
    public string PubSubName { get; set; } = "rabbitmq-pubsub";
    public string TopicName { get; set; } = "collaborator-invitations";
}
