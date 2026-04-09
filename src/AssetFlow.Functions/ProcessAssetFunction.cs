using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Messaging;
using AssetFlow.Domain.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AssetFlow.Functions
{
    public class ProcessAssetFunction
    {
        private readonly ILogger<ProcessAssetFunction> _logger;
        private readonly IAssetRepository _assetRepository;

        public ProcessAssetFunction(
          ILogger<ProcessAssetFunction> logger,
          IAssetRepository assetRepository)
        {
            _logger = logger;
            _assetRepository = assetRepository;
        }

        [Function("ProcessAssetMessage")]
        public async Task Run(
            [ServiceBusTrigger("asset-processing", Connection = "ServiceBusConnection")]
            string message,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service Bus message received: {Message}", message);

            var assetMessage = JsonSerializer.Deserialize<AssetProcessingMessage>(message);

            if (assetMessage is null)
            {
                _logger.LogWarning("The message could not be deserialized to AssetProcessingMessage.");
                return;
            }

            _logger.LogInformation("Processing asset with ID: {assetMessage}", JsonSerializer.Serialize(assetMessage));

            var asset = await _assetRepository.GetByIdAsync(assetMessage.AssetId, cancellationToken);

            _logger.LogInformation("Asset retrieved from repository: {asset}", JsonSerializer.Serialize(asset));

            if (asset is null)
            {
                _logger.LogWarning("Asset with id {AssetId} was not found in Cosmos DB.", assetMessage.AssetId);
                return;
            }

            try
            {
                asset.Status = AssetStatus.Processing;
                await _assetRepository.UpdateAsync(asset, cancellationToken);

                _logger.LogInformation("Asset {AssetId} moved to Processing.", asset.Id);

                await Task.Delay(2000, cancellationToken);

                asset.Status = AssetStatus.Processed;
                await _assetRepository.UpdateAsync(asset, cancellationToken);

                _logger.LogInformation("Asset {AssetId} moved to Processed.", asset.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing asset {AssetId}.", asset.Id);

                try
                {
                    asset.Status = AssetStatus.Failed;
                    await _assetRepository.UpdateAsync(asset, cancellationToken);

                    _logger.LogInformation("Asset {AssetId} moved to Failed.", asset.Id);
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "An error occurred while updating asset {AssetId} to Failed.", asset.Id);
                }

                throw;
            }
        }
    }
}
