using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;

        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            if (request is null)
            {
                return BadRequest();
            }

            var response = await _ordersService.CheckoutAsync(request);
            Response.Headers["X-Order-Id"] = response.OrderId.ToString();

            return Redirect(response.PaymentUrl);
        }

        [AllowAnonymous]
        [HttpPost("payment/webhook")]
        public async Task<IActionResult> PaymentWebhook([FromBody] PaymentWebhookRequest request)
        {
            if (request is null)
            {
                return BadRequest();
            }

            await _ordersService.HandlePaymentWebhookAsync(request);
            return Ok();
        }
    }
}