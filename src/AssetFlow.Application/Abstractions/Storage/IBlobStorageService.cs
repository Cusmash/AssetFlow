namespace AssetFlow.Application.Abstractions.Storage
{
    public interface IBlobStorageService
    {
        Task<BlobUploadResult> UploadAsync(
            Stream content,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default);

        Task<BlobFileDownloadResult?> DownloadAsync(
            string blobName,
            CancellationToken cancellationToken = default);
    }
}
