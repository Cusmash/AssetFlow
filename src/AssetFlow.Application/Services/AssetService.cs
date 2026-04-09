using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Abstractions.Storage;
using AssetFlow.Application.DTOs;
using AssetFlow.Domain.Entities;
using AssetFlow.Application.Abstractions.Messaging;
using AssetFlow.Application.Messaging;

namespace AssetFlow.Application.Services
{
    public class AssetService
    {
        private readonly IAssetRepository _repository;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IAssetProcessingPublisher _assetProcessingPublisher;

        public AssetService(
            IAssetRepository repository,
            IBlobStorageService blobStorageService,
            IAssetProcessingPublisher assetProcessingPublisher)
        {
            _repository = repository;
            _blobStorageService = blobStorageService;
            _assetProcessingPublisher = assetProcessingPublisher;
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

            var uploadResult = await _blobStorageService.UploadAsync(
                fileStream,
                originalFileName,
                contentType,
                cancellationToken);

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

            var message = new AssetProcessingMessage
            {
                AssetId = created.Id,
                BlobName = created.BlobName,
                BlobContainerName = created.BlobContainerName,
                CreatedAtUtc = created.CreatedAtUtc
            };

            await _assetProcessingPublisher.PublishAsync(message, cancellationToken);

            return MapToResponse(created);
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