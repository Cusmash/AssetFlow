using AssetFlow.Application.Abstractions;
using AssetFlow.Application.DTOs;
using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Services
{
    public class AssetService
    {
        private readonly IAssetRepository _repository;

        public AssetService(IAssetRepository repository)
        {
            _repository = repository;
        }

        public async Task<AssetResponse> CreateAsync(CreateAssetRequest request, CancellationToken cancellationToken = default)
        {
            var asset = new Asset
            {
                Name = request.Name,
                Description = request.Description,
                ContentType = request.ContentType,
                Status = AssetStatus.Pending
            };

            var created = await _repository.CreateAsync(asset, cancellationToken);

            return new AssetResponse
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description,
                ContentType = created.ContentType,
                Status = created.Status,
                CreatedAtUtc = created.CreatedAtUtc
            };
        }

        public async Task<IReadOnlyList<AssetResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var items = await _repository.GetAllAsync(cancellationToken);

            return items.Select(x => new AssetResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ContentType = x.ContentType,
                Status = x.Status,
                CreatedAtUtc = x.CreatedAtUtc
            }).ToList();
        }

        public async Task<AssetResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var asset = await _repository.GetByIdAsync(id, cancellationToken);
            if (asset is null) return null;

            return new AssetResponse
            {
                Id = asset.Id,
                Name = asset.Name,
                Description = asset.Description,
                ContentType = asset.ContentType,
                Status = asset.Status,
                CreatedAtUtc = asset.CreatedAtUtc
            };
        }
    }
}
