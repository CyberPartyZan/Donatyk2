using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LotsController : ControllerBase
    {
        private readonly ILotsService _lotsService;

        public LotsController(ILotsService lotsService)
        {
            _lotsService = lotsService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] LotSearchQuery query)
        {
            var effectiveQuery = query ?? new LotSearchQuery();
            var totalCount = await _lotsService.GetTotalCount(effectiveQuery);
            var results = await _lotsService.GetAll(effectiveQuery);

            Response.Headers["X-Total-Count"] = totalCount.ToString();
            return Ok(results);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(Guid id)
        {
            var dto = await _lotsService.GetLotById(id);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LotDto dto)
        {
            if (dto is null) return BadRequest();

            try
            {
                var id = await _lotsService.CreateLot(dto);
                return CreatedAtAction(nameof(Get), new { id }, null);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] LotDto dto)
        {
            try
            {
                await _lotsService.UpdateLot(id, dto);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _lotsService.DeleteLot(id);
            return NoContent();
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                await _lotsService.ApproveLot(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/decline")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Decline(Guid id, [FromBody] DeclineLotRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest("Decline reason is required.");
            }

            try
            {
                await _lotsService.DeclineLot(id, request.Reason);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("statistics")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStatistic([FromQuery] LotSearchQuery query)
        {
            var effectiveQuery = query ?? new LotSearchQuery();
            var stats = await _lotsService.GetStatistic(effectiveQuery);
            return Ok(stats);
        }
    }
}
