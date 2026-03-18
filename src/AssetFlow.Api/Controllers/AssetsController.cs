using AssetFlow.Application.DTOs;
using AssetFlow.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.Api.Controllers
{
    [ApiController]
    [Route("api/assets")]
    public class AssetsController : ControllerBase
    {
        private readonly AssetService _assetService;

        public AssetsController(AssetService assetService)
        {
            _assetService = assetService;
        }

        [HttpPost]
        public async Task<ActionResult<AssetResponse>> Create(
            [FromBody] CreateAssetRequest request,
            CancellationToken cancellationToken)
        {
            var created = await _assetService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AssetResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _assetService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AssetResponse>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _assetService.GetByIdAsync(id, cancellationToken);
            if (result is null) return NotFound();

            return Ok(result);
        }
    }
}