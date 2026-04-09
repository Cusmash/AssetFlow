using AssetFlow.Application.Messaging;

namespace AssetFlow.Application.Abstractions.Messaging
{
    public interface IAssetProcessingPublisher
    {
        Task PublishAsync(AssetProcessingMessage message, CancellationToken cancellationToken);
    }
}
