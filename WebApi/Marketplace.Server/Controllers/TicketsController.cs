using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketsService _ticketsService;

        public TicketsController(ITicketsService ticketsService)
        {
            _ticketsService = ticketsService;
        }

        [HttpGet("{lotId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(Guid lotId)
        {
            var tickets = await _ticketsService.GetAll(lotId);
            return Ok(tickets);
        }

        [HttpPost("{lotId:guid}")]
        public async Task<IActionResult> Create(Guid lotId, [FromQuery] int count = 1)
        {
            if (count <= 0)
            {
                return BadRequest("Count must be greater than zero.");
            }

            try
            {
                var tickets = await _ticketsService.Create(lotId, count);
                return Ok(tickets);
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

        [HttpPost("{lotId:guid}/winner")]
        public async Task<IActionResult> FindWinner(Guid lotId)
        {
            try
            {
                var winner = await _ticketsService.FindWinner(lotId);
                return Ok(winner);
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
    }
}