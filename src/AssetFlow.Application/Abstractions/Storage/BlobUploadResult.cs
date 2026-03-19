namespace AssetFlow.Application.Abstractions.Storage
{
    public sealed class BlobUploadResult
    {
        public string ContainerName { get; init; } = default!;
        public string BlobName { get; init; } = default!;
        public Uri BlobUri { get; init; } = default!;
        public string ContentType { get; init; } = default!;
        public long FileSizeBytes { get; init; }
    }
}
