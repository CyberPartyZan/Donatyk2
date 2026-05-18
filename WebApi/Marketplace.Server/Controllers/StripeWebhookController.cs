using Marketplace.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Marketplace.Server.Controllers
{
    [ApiController]
    [Route("api/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IOrdersService _ordersService;
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IOrdersService ordersService,
            IOptions<StripeSettings> stripeSettings,
            ILogger<StripeWebhookController> logger)
        {
            _ordersService = ordersService;
            _stripeSettings = stripeSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Stripe webhook endpoint.
        /// Handles checkout.session.completed for standard and hold payments.
        /// Configured in Stripe Dashboard → Webhooks → endpoint URL.
        /// </summary>
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            string json;
            using (var reader = new StreamReader(HttpContext.Request.Body))
                json = await reader.ReadToEndAsync();

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _stripeSettings.WebhookSecret,
                    throwOnApiVersionMismatch: false);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature validation failed.");
                return BadRequest("Invalid Stripe webhook signature.");
            }

            _logger.LogInformation("Received Stripe event {EventType} (id={EventId})", stripeEvent.Type, stripeEvent.Id);

            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                    await HandleCheckoutSessionCompletedAsync(stripeEvent);
                    break;

                case EventTypes.CheckoutSessionExpired:
                    await HandleCheckoutSessionExpiredAsync(stripeEvent);
                    break;

                default:
                    _logger.LogDebug("Unhandled Stripe event type {EventType}. Ignored.", stripeEvent.Type);
                    break;
            }

            return Ok();
        }

        // ── private handlers ─────────────────────────────────────────────────────

        private async Task HandleCheckoutSessionCompletedAsync(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not Session session)
                return;

            if (!TryGetOrderId(session.Metadata, out var orderId))
            {
                _logger.LogWarning(
                    "checkout.session.completed missing orderId in metadata. SessionId={SessionId}",
                    session.Id);
                return;
            }

            var paymentIntentId = session.PaymentIntentId;
            var reference = string.IsNullOrWhiteSpace(paymentIntentId) ? session.Id : paymentIntentId;

            var webhookRequest = new PaymentWebhookRequest
            {
                OrderId = orderId,
                Provider = StripeSettings.SectionName,
                Reference = reference,
                IsSuccess = true
            };

            await _ordersService.HandlePaymentWebhookAsync(webhookRequest);
        }

        private async Task HandleCheckoutSessionExpiredAsync(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not Session session)
                return;

            if (!TryGetOrderId(session.Metadata, out var orderId))
            {
                _logger.LogWarning(
                    "checkout.session.expired missing orderId in metadata. SessionId={SessionId}",
                    session.Id);
                return;
            }

            var webhookRequest = new PaymentWebhookRequest
            {
                OrderId = orderId,
                Provider = StripeSettings.SectionName,
                Reference = session.Id,
                IsSuccess = false
            };

            await _ordersService.HandlePaymentWebhookAsync(webhookRequest);
        }

        private static bool TryGetOrderId(
            Dictionary<string, string>? metadata,
            out Guid orderId)
        {
            orderId = Guid.Empty;
            return metadata is not null
                && metadata.TryGetValue("orderId", out var raw)
                && Guid.TryParse(raw, out orderId);
        }
    }
}