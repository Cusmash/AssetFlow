
using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.DTOs
{
    public class AssetResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string ContentType { get; set; } = default!;
        public AssetStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public string? BlobContainerName { get; set; }
        public string? BlobName { get; set; }
        public string? OriginalFileName { get; set; }
        public long? FileSizeBytes { get; set; }
        public string? StorageUri { get; set; }
    }
}
