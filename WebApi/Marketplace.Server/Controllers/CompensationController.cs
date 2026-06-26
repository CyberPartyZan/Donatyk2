using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Server.Controllers
{
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