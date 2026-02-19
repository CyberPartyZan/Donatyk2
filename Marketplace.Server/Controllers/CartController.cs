using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var cart = await _cartService.Get();
            return Ok(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddCartItemRequest request)
        {
            if (request is null) return BadRequest();
            if (request.Quantity <= 0) return BadRequest("Quantity must be greater than zero.");

            var lotId = await _cartService.AddItem(request.LotId, request.Quantity);
            return CreatedAtAction(nameof(Get), new { lotId }, null);
        }

        // Change quantity by lotId (owner is inferred from authenticated user)
        [HttpPut("lot/{lotId}")]
        public async Task<IActionResult> Put(Guid lotId, [FromBody] ChangeCartItemQuantityRequest request)
        {
            if (request is null) return BadRequest();
            if (request.Quantity <= 0) return BadRequest("Quantity must be greater than zero.");

            try
            {
                await _cartService.ChangeQuantity(lotId, request.Quantity);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // Remove by lotId (owner is inferred from authenticated user)
        [HttpDelete("lot/{lotId}")]
        public async Task<IActionResult> Delete(Guid lotId)
        {
            try
            {
                await _cartService.RemoveItem(lotId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}