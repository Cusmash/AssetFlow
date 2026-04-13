using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Abstractions.Storage;
using AssetFlow.Application.DTOs;
using AssetFlow.Domain.Entities;
using AssetFlow.Application.Abstractions.Messaging;
using AssetFlow.Application.Messaging;
using Microsoft.Extensions.Logging;

namespace AssetFlow.Application.Services
{
    public class AssetService
    {
        private readonly IAssetRepository _repository;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IAssetProcessingPublisher _assetProcessingPublisher;
        private readonly ILogger<AssetService> _logger;

        public AssetService(
            IAssetRepository repository,
            IBlobStorageService blobStorageService,
            IAssetProcessingPublisher assetProcessingPublisher,
            ILogger<AssetService> logger)
        {
            _repository = repository;
            _blobStorageService = blobStorageService;
            _assetProcessingPublisher = assetProcessingPublisher;
            _logger = logger;
        }

        public async Task<AssetResponse> CreateAsync(CreateAssetRequest request, CancellationToken cancellationToken = default)
        {
            var asset = new Asset
            {
                Name = request.Name,
                Description = request.Description,
                ContentType = request.ContentType,
                Status = AssetStatus.Pending
            };

            var created = await _repository.CreateAsync(asset, cancellationToken);
            return MapToResponse(created);
        }

        public async Task<AssetResponse> UploadAsync(
            Stream fileStream,
            string originalFileName,
            string contentType,
            string name,
            string? description,
            CancellationToken cancellationToken = default)
        {
            if (fileStream is null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name is required.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(originalFileName))
            {
                throw new ArgumentException("Original file name is required.", nameof(originalFileName));
            }

            if (string.IsNullOrWhiteSpace(contentType))
            {
                contentType = "application/octet-stream";
            }

            try
            {
                _logger.LogInformation(
                        "Starting asset upload. FileName: {FileName}, ContentType: {ContentType} ",
                        name,
                        contentType);

                var uploadResult = await _blobStorageService.UploadAsync(
                    fileStream,
                    originalFileName,
                    contentType,
                    cancellationToken);

                _logger.LogInformation(
                    "Blob upload completed. BlobName: {BlobName}, ContainerName: {ContainerName}, StorageUri: {StorageUri}",
                    uploadResult.BlobName,
                    uploadResult.ContainerName,
                    uploadResult.BlobUri);

                var asset = new Asset
                {
                    Name = name,
                    Description = description,
                    ContentType = uploadResult.ContentType,
                    Status = AssetStatus.Uploaded,
                    BlobContainerName = uploadResult.ContainerName,
                    BlobName = uploadResult.BlobName,
                    OriginalFileName = originalFileName,
                    FileSizeBytes = uploadResult.FileSizeBytes,
                    StorageUri = uploadResult.BlobUri.ToString()
                };

                var created = await _repository.CreateAsync(asset, cancellationToken);

                _logger.LogInformation(
                    "Asset metadata saved. AssetId: {AssetId}, Status: {Status}",
                    created.Id,
                    created.Status);

                var message = new AssetProcessingMessage
                {
                    AssetId = created.Id,
                    BlobName = created.BlobName,
                    BlobContainerName = created.BlobContainerName,
                    CreatedAtUtc = created.CreatedAtUtc
                };

                await _assetProcessingPublisher.PublishAsync(message, cancellationToken);

                _logger.LogInformation(
                    "Asset processing message published. AssetId: {AssetId}, BlobName: {BlobName}",
                    created.Id,
                    created.BlobName);

                return MapToResponse(created);
            }
            catch (Exception ex)
            {

                _logger.LogError(
                     ex,
                     "Asset upload flow failed. FileName: {FileName}, Name: {Name}",
                     originalFileName,
                     name);

                throw;
            }
        }

        public async Task<IReadOnlyList<AssetResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var items = await _repository.GetAllAsync(cancellationToken);
            return items.Select(MapToResponse).ToList();
        }

        public async Task<AssetResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var asset = await _repository.GetByIdAsync(id, cancellationToken);
            return asset is null ? null : MapToResponse(asset);
        }

        public async Task<BlobFileDownloadResult?> DownloadAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var asset = await _repository.GetByIdAsync(id, cancellationToken);
            if (asset is null || string.IsNullOrWhiteSpace(asset.BlobName))
            {
                return null;
            }

            var downloadResult = await _blobStorageService.DownloadAsync(asset.BlobName, cancellationToken);
            if (downloadResult is null)
            {
                return null;
            }

            var fileName = string.IsNullOrWhiteSpace(asset.OriginalFileName)
                ? downloadResult.FileName
                : asset.OriginalFileName;

            return new BlobFileDownloadResult
            {
                Content = downloadResult.Content,
                ContentType = downloadResult.ContentType,
                FileName = fileName
            };
        }

        private static AssetResponse MapToResponse(Asset asset)
        {
            return new AssetResponse
            {
                Id = asset.Id,
                Name = asset.Name,
                Description = asset.Description,
                ContentType = asset.ContentType,
                Status = asset.Status,
                CreatedAtUtc = asset.CreatedAtUtc,
                BlobContainerName = asset.BlobContainerName,
                BlobName = asset.BlobName,
                OriginalFileName = asset.OriginalFileName,
                FileSizeBytes = asset.FileSizeBytes,
                StorageUri = asset.StorageUri
            };
        }
    }
}