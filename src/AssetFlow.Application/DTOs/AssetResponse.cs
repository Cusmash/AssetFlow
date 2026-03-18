
using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.DTOs
{
    public class AssetResponse
    {
        public Guid Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string ContentType { get; set; } = default!;
        public AssetStatus Status { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; }
    }
}
