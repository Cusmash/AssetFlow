

namespace AssetFlow.Domain.Entities
{
    public class Asset
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string ContentType { get; set; } = default!;
        public AssetStatus Status { get; set; } = AssetStatus.Pending;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public string? BlobContainerName { get; set; }
        public string? BlobName { get; set; }
        public string? OriginalFileName { get; set; }
        public long? FileSizeBytes { get; set; }
        public string? StorageUri { get; set; }
    }
}
