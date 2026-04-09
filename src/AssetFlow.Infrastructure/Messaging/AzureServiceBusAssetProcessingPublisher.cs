using System.Text.Json;
using AssetFlow.Application.Abstractions.Messaging;
using AssetFlow.Application.Messaging;
using AssetFlow.Infrastructure.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;

namespace AssetFlow.Infrastructure.Messaging
{
    public class AzureServiceBusAssetProcessingPublisher : IAssetProcessingPublisher, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public AzureServiceBusAssetProcessingPublisher(
            IOptions<ServiceBusOptions> options)
        {
            var serviceBusOptions = options.Value;

            if (string.IsNullOrWhiteSpace(serviceBusOptions.ConnectionString))
            {
                throw new InvalidOperationException("ServiceBus:ConnectionString is not configured.");
            }

            if (string.IsNullOrWhiteSpace(serviceBusOptions.QueueName))
            {
                throw new InvalidOperationException("ServiceBus:QueueName is not configured.");
            }

            _client = new ServiceBusClient(serviceBusOptions.ConnectionString);
            _sender = _client.CreateSender(serviceBusOptions.QueueName);
        }

        public async Task PublishAsync(
            AssetProcessingMessage message,
            CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(message);

            var serviceBusMessage = new ServiceBusMessage(json)
            {
                MessageId = message.AssetId.ToString(),
                Subject = "asset-processing-request"
            };

            serviceBusMessage.ApplicationProperties["assetId"] = message.AssetId;
            serviceBusMessage.ApplicationProperties["blobName"] = message.BlobName;

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
