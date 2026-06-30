using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Server.Controllers
{
    public sealed class ProcessCompensationRequest
    {
        public List<Guid> Ids { get; set; } = [];
        public IFormFile? ApprovementDocument { get; set; }
    }

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CompensationController : ControllerBase
    {
        private readonly ICompensationService _compensationService;

        public CompensationController(ICompensationService compensationService)
        {
            _compensationService = compensationService;
        }

        [HttpPost("request/{sellerId:guid}")]
        public async Task<IActionResult> Request(Guid sellerId)
        {
            var updated = await _compensationService.RequestCompensation(sellerId);
            return Ok(new { updated });
        }

        [HttpPost("process")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Process([FromForm] ProcessCompensationRequest request)
        {
            if (request.Ids is null || request.Ids.Count == 0)
                return BadRequest("At least one compensation id is required.");

            if (request.ApprovementDocument is null || request.ApprovementDocument.Length == 0)
                return BadRequest("Approval document is required.");

            if (!request.ApprovementDocument.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Approval document must be a PDF file.");

            try
            {
                using var stream = request.ApprovementDocument.OpenReadStream();
                var updated = await _compensationService.Process(request.Ids, stream, request.ApprovementDocument.FileName);
                return Ok(new { updated });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("seller/{sellerId:guid}")]
        public async Task<IActionResult> GetBySeller(Guid sellerId, [FromQuery] CompensationStatus? status = null)
        {
            var data = await _compensationService.GetBySellerId(sellerId, status);
            return Ok(data);
        }

        [HttpGet("{id:guid}/approvement-document-url")]
        public async Task<IActionResult> GetApprovementDocumentUrl(Guid id)
        {
            try
            {
                var url = await _compensationService.GetApprovementDocumentUrl(id);
                return Ok(new { url });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] CompensationStatus? status = null)
        {
            var data = await _compensationService.GetAll(page, pageSize, status);
            return Ok(data);
        }
    }
}