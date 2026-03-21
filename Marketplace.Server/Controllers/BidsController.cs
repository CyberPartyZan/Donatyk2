using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BidsController : ControllerBase
    {
        private readonly IBidsService _bidsService;

        public BidsController(IBidsService bidsService)
        {
            _bidsService = bidsService;
        }

        [HttpPost("{lotId:guid}")]
        public async Task<IActionResult> PlaceBid(Guid lotId, [FromBody] PlaceBidRequest request)
        {
            if (request is null || request.Amount is null)
            {
                return BadRequest();
            }

            try
            {
                var bid = await _bidsService.PlaceBid(lotId, request.Amount);
                return Ok(bid);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{lotId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBidHistory(Guid lotId)
        {
            var history = await _bidsService.LoadBidHistory(lotId);
            return Ok(history);
        }
    }
}