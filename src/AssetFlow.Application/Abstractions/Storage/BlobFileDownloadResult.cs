namespace AssetFlow.Application.Abstractions.Storage
{
    public sealed class BlobFileDownloadResult
    {
        public Stream Content { get; init; } = default!;
        public string ContentType { get; init; } = default!;
        public string FileName { get; init; } = default!;
    }
}
