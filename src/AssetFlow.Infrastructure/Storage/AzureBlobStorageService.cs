using AssetFlow.Application.Abstractions.Storage;
using AssetFlow.Infrastructure.Options;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace AssetFlow.Infrastructure.Storage
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly BlobStorageOptions _options;
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobStorageService(IOptions<BlobStorageOptions> options)
        {
            _options = options.Value;
            _blobServiceClient = new BlobServiceClient(_options.ConnectionString);
        }

        public async Task<BlobUploadResult> UploadAsync(
            Stream content,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            var blobName = BuildBlobName(fileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            };

            await blobClient.UploadAsync(content, uploadOptions, cancellationToken);

            return new BlobUploadResult
            {
                ContainerName = containerClient.Name,
                BlobName = blobName,
                BlobUri = blobClient.Uri,
                ContentType = contentType,
                FileSizeBytes = content.Length
            };
        }

        public async Task<BlobFileDownloadResult?> DownloadAsync(
            string blobName,
            CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var exists = await blobClient.ExistsAsync(cancellationToken);
            if (!exists.Value)
            {
                return null;
            }

            var downloadResponse = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

            var contentType = downloadResponse.Value.Details.ContentType;
            if (string.IsNullOrWhiteSpace(contentType))
            {
                contentType = "application/octet-stream";
            }

            return new BlobFileDownloadResult
            {
                Content = downloadResponse.Value.Content,
                ContentType = contentType,
                FileName = Path.GetFileName(blobName)
            };
        }

        private static string BuildBlobName(string fileName)
        {
            var safeFileName = Path.GetFileName(fileName);
            var extension = Path.GetExtension(safeFileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(safeFileName);

            var sanitizedName = string.IsNullOrWhiteSpace(nameWithoutExtension)
                ? "file"
                : nameWithoutExtension.Replace(" ", "-").ToLowerInvariant();

            return $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}-{sanitizedName}{extension}";
        }
    }
}