using AssetFlow.Application.Abstractions;
using AssetFlow.Domain.Entities;
using AssetFlow.Infrastructure.Options;
using AssetFlow.Infrastructure.Persistence.Documents;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace AssetFlow.Infrastructure.Persistence
{
    public class CosmosAssetRepository : IAssetRepository
    {
        private readonly Container _container;

        public CosmosAssetRepository(
            CosmosClient cosmosClient,
            IOptions<CosmosDbOptions> options)
        {
            try
            {
                var cosmosOptions = options.Value;

                var database = cosmosClient
                    .CreateDatabaseIfNotExistsAsync(cosmosOptions.DatabaseName)
                    .GetAwaiter()
                    .GetResult();

                var container = database.Database
                    .CreateContainerIfNotExistsAsync(
                        id: cosmosOptions.ContainerName,
                        partitionKeyPath: "/status")
                    .GetAwaiter()
                    .GetResult();

                _container = container.Container;
            }
            catch (Exception ex)
            {

                var innerMessage = ex.InnerException?.Message ?? "No inner exception";
                throw new Exception(
                    $"Error initializing CosmosAssetRepository. Message: {ex.Message}. Inner: {innerMessage}",
                    ex);
            }
        }

        public async Task<Asset> CreateAsync(Asset asset, CancellationToken cancellationToken = default)
        {
            var document = MapToDocument(asset);

            var response = await _container.CreateItemAsync(
                document,
                new PartitionKey(document.Status),
                cancellationToken: cancellationToken);

            return MapToDomain(response.Resource);
        }

        public async Task<IReadOnlyList<Asset>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var query = new QueryDefinition("SELECT * FROM c ORDER BY c.createdAtUtc DESC");
            var iterator = _container.GetItemQueryIterator<AssetDocument>(query);

            var results = new List<Asset>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response.Select(MapToDomain));
            }

            return results;
        }

        public async Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                .WithParameter("@id", id.ToString());

            var iterator = _container.GetItemQueryIterator<AssetDocument>(
                query,
                requestOptions: new QueryRequestOptions
                {
                    MaxItemCount = 1
                });

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                var document = response.FirstOrDefault();

                if (document is not null)
                {
                    return MapToDomain(document);
                }
            }

            return null;
        }

        private static AssetDocument MapToDocument(Asset asset)
        {
            return new AssetDocument
            {
                Id = asset.Id.ToString(),
                Status = asset.Status.ToString(),
                Name = asset.Name,
                Description = asset.Description,
                ContentType = asset.ContentType,
                CreatedAtUtc = asset.CreatedAtUtc,
                BlobContainerName = asset.BlobContainerName,
                BlobName = asset.BlobName,
                OriginalFileName = asset.OriginalFileName,
                FileSizeBytes = asset.FileSizeBytes,
                StorageUri = asset.StorageUri
            };
        }

        private static Asset MapToDomain(AssetDocument document)
        {
            return new Asset
            {
                Id = Guid.Parse(document.Id),
                Status = Enum.Parse<AssetStatus>(document.Status),
                Name = document.Name,
                Description = document.Description,
                ContentType = document.ContentType,
                CreatedAtUtc = document.CreatedAtUtc,
                BlobContainerName = document.BlobContainerName,
                BlobName = document.BlobName,
                OriginalFileName = document.OriginalFileName,
                FileSizeBytes = document.FileSizeBytes,
                StorageUri = document.StorageUri
            };
        }
    }
}
