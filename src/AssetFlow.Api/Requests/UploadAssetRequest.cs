namespace AssetFlow.Api.Requests
{
    public class UploadAssetRequest
    {
        public IFormFile File { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
    }
}
