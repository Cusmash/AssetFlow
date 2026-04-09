using AssetFlow.Application.Abstractions;
using AssetFlow.Domain.Entities;

namespace AssetFlow.Infrastructure.Persistence
{
    public class InMemoryAssetRepository : IAssetRepository
    {
        private static readonly List<Asset> Assets = new();

        public Task<Asset> CreateAsync(Asset asset, CancellationToken cancellationToken = default)
        {
            Assets.Add(asset);
            return Task.FromResult(asset);
        }

        public Task<IReadOnlyList<Asset>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<Asset>)Assets.ToList());
        }

        public Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var asset = Assets.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(asset);
        }

        public Task<Asset> UpdateAsync(Asset asset, CancellationToken cancellationToken = default)
        {
            var existingAsset = Assets.FirstOrDefault(x => x.Id == asset.Id);
            return Task.FromResult(existingAsset ?? throw new InvalidOperationException($"Asset with ID {asset.Id} not found."));
        }
    }
}
