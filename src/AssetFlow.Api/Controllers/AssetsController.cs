using AssetFlow.Api.Requests;
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

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<AssetResponse>> Upload(
            [FromForm] UploadAssetRequest request,
            CancellationToken cancellationToken)
        {
            if (request.File is null || request.File.Length == 0)
            {
                return BadRequest("A file is required.");
            }

            await using var stream = request.File.OpenReadStream();

            var result = await _assetService.UploadAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                request.Name,
                request.Description,
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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