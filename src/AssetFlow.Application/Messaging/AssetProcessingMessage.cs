namespace AssetFlow.Application.Messaging
{
    public class AssetProcessingMessage
    {
        public Guid AssetId { get; set; } = default!;
        public string? BlobName { get; set; } = default!;
        public string? BlobContainerName { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; }
    }
}
