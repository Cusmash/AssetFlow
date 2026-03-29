using Newtonsoft.Json;

namespace AssetFlow.Infrastructure.Persistence.Documents
{
    public class AssetDocument
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = default!;

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; } = default!;

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = default!;

        [JsonProperty(PropertyName = "description")]
        public string? Description { get; set; }

        [JsonProperty(PropertyName = "contentType")]
        public string ContentType { get; set; } = default!;

        [JsonProperty(PropertyName = "createdAtUtc")]
        public DateTime CreatedAtUtc { get; set; }

        [JsonProperty(PropertyName = "blobContainerName")]
        public string? BlobContainerName { get; set; }

        [JsonProperty(PropertyName = "blobName")]
        public string? BlobName { get; set; }

        [JsonProperty(PropertyName = "originalFileName")]
        public string? OriginalFileName { get; set; }

        [JsonProperty(PropertyName = "fileSizeBytes")]
        public long? FileSizeBytes { get; set; }

        [JsonProperty(PropertyName = "storageUri")]
        public string? StorageUri { get; set; }
    }
}
