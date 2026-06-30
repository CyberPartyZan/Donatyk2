using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentsController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        /// <summary>
        /// Moves the shipment (and its associated order) to Processing status.
        /// </summary>
        [HttpPut("{shipmentId:guid}/take-into-processing")]
        public async Task<IActionResult> TakeIntoProcessing(Guid shipmentId, [FromQuery] string trackingNumber)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return BadRequest("Tracking number is required.");

            try
            {
                await _shipmentService.TakeIntoProcessingAsync(shipmentId, trackingNumber);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Marks the shipment (and its associated order) as Shipped.
        /// </summary>
        [HttpPut("{shipmentId:guid}/shipped")]
        public async Task<IActionResult> Shipped(Guid shipmentId)
        {
            try
            {
                await _shipmentService.MarkShippedAsync(shipmentId);
                return NoContent();
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

        /// <summary>
        /// Marks the shipment (and its associated order) as InTransit.
        /// </summary>
        [HttpPut("{shipmentId:guid}/in-transit")]
        public async Task<IActionResult> InTransit(Guid shipmentId)
        {
            try
            {
                await _shipmentService.MarkInTransitAsync(shipmentId);
                return NoContent();
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

        /// <summary>
        /// Marks the shipment (and its associated order) as OutForDelivery.
        /// </summary>
        [HttpPut("{shipmentId:guid}/out-for-delivery")]
        public async Task<IActionResult> OutForDelivery(Guid shipmentId)
        {
            try
            {
                await _shipmentService.MarkOutForDeliveryAsync(shipmentId);
                return NoContent();
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

        /// <summary>
        /// Marks the shipment (and its associated order) as Delivered.
        /// </summary>
        [HttpPut("{shipmentId:guid}/delivered")]
        public async Task<IActionResult> Delivered(Guid shipmentId)
        {
            try
            {
                await _shipmentService.MarkDeliveredAsync(shipmentId);
                return NoContent();
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

        /// <summary>
        /// Get all shipments with optional filters.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool onlyPending = false,
            [FromQuery] Guid? sellerId = null)
        {
            var totalCount = await _shipmentService.GetTotalCountAsync(search, onlyPending, sellerId);
            var shipments = await _shipmentService.GetAllAsync(search, page, pageSize, onlyPending, sellerId);

            Response.Headers["X-Total-Count"] = totalCount.ToString();
            return Ok(shipments);
        }

        /// <summary>
        /// Gets shipment statistics.
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] string? search, [FromQuery] Guid? sellerId = null)
        {
            var stats = await _shipmentService.GetStatisticsAsync(search, sellerId);
            return Ok(stats);
        }
    }
}