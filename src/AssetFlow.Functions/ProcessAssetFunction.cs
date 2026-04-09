using System.Text.Json;
using AssetFlow.Application.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AssetFlow.Functions
{
    public class ProcessAssetFunction
    {
        private readonly ILogger<ProcessAssetFunction> _logger;

        public ProcessAssetFunction(ILogger<ProcessAssetFunction> logger)
        {
            _logger = logger;
        }

        [Function("ProcessAssetMessage")]
        public void Run(
            [ServiceBusTrigger("asset-processing", Connection = "ServiceBusConnection")]
            string message)
        {
            _logger.LogInformation("Service Bus message received: {Message}", message);

            var assetMessage = JsonSerializer.Deserialize<AssetProcessingMessage>(message);

            if (assetMessage is null)
            {
                _logger.LogWarning("The message could not be deserialized to AssetProcessingMessage.");
                return;
            }

            _logger.LogInformation(
                "Asset message processed. AssetId: {AssetId}, BlobName: {BlobName}, BlobContainerName: {BlobContainerName}, CreatedAtUtc: {CreatedAtUtc}",
                assetMessage.AssetId,
                assetMessage.BlobName,
                assetMessage.BlobContainerName,
                assetMessage.CreatedAtUtc);
        }
    }
}
