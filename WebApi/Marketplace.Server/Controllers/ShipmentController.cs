using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        /// <summary>
        /// Moves the shipment (and its associated order) to Processing status.
        /// </summary>
        [HttpPut("{shipmentId:guid}/take-into-processing")]
        public async Task<IActionResult> TakeIntoProcessing(Guid shipmentId)
        {
            try
            {
                await _shipmentService.TakeIntoProcessingAsync(shipmentId);
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
    }
}