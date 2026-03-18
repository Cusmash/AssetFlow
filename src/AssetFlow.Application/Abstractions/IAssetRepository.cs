using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Abstractions
{
    public interface IAssetRepository
    {
        Task<Asset> CreateAsync(Asset asset, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Asset>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
