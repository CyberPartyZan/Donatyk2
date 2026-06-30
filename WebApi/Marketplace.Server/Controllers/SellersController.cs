using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Server.Controllers
{
    public sealed class UploadSellerAvatarRequest
    {
        public IFormFile? File { get; set; }
    }

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SellersController : ControllerBase
    {
        private readonly ISellersService _sellersService;

        public SellersController(ISellersService sellersService)
        {
            _sellersService = sellersService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var sellers = await _sellersService.GetAll(search, page, pageSize);
            return Ok(sellers);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(Guid id)
        {
            var seller = await _sellersService.GetById(id);
            return seller is null ? NotFound() : Ok(seller);
        }

        [HttpGet("/api/seller/avatar/{blobKey}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvatar(string blobKey)
        {
            if (string.IsNullOrWhiteSpace(blobKey))
                return BadRequest("Avatar key is required.");

            try
            {
                var stream = await _sellersService.GetAvatar(blobKey);
                return File(stream, "image/*");
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SellerDto seller)
        {
            await _sellersService.Create(seller);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromBody] SellerDto seller, Guid id)
        {
            await _sellersService.Update(id, seller);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _sellersService.Delete(id);
            return NoContent();
        }

        [HttpPost("avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAvatar([FromForm] UploadSellerAvatarRequest request)
        {
            if (request.File is null || request.File.Length == 0)
                return BadRequest("Avatar file is required.");

            if (!request.File.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only image files are allowed.");

            using var stream = request.File.OpenReadStream();
            var blob = await _sellersService.UploadAvatar(stream, request.File.FileName);
            return Ok(blob);
        }

        [HttpGet("by-user/{userId:guid}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim is null || (!User.IsInRole("Admin") && userIdClaim != userId.ToString()))
            {
                return NotFound();
            }

            var seller = await _sellersService.GetByUserId(userId);
            return seller is null ? NotFound() : Ok(seller);
        }
    }
}
