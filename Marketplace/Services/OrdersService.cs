using Marketplace.Abstractions;
using Marketplace.Notification;
using Marketplace.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Marketplace
{
    public class OrdersService : IOrdersService
    {
        private readonly ClaimsPrincipal _user;
        private readonly ICartRepository _cartRepository;
        private readonly ILotsRepository _lotsRepository;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly INotificationService _notificationService;
        private readonly ILogger<OrdersService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersService(
            ClaimsPrincipal user,
            ICartRepository cartRepository,
            ILotsRepository lotsRepository,
            IOrdersRepository ordersRepository,
            IPaymentGateway paymentGateway,
            INotificationService notificationService,
            IPublishEndpoint publishEndpoint,
            ILogger<OrdersService> logger)
        {
            _user = user;
            _cartRepository = cartRepository;
            _lotsRepository = lotsRepository;
            _ordersRepository = ordersRepository;
            _paymentGateway = paymentGateway;
            _notificationService = notificationService;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var userId = GetCurrentUserIdOrThrow();

            var cart = await _cartRepository.GetCartByUserId(userId);
            var cartItems = cart.Items.ToList();

            if (cartItems.Count == 0)
            {
                throw new InvalidOperationException("Cart is empty.");
            }

            var shippingInfo = ToShippingInfo(request.Shipping);
            var paymentInfo = ToPaymentInfo(request.Payment);
            var pricedItems = new List<PricedItem>(cartItems.Count);

            foreach (var cartItem in cartItems)
            {
                var lot = await _lotsRepository.GetLotById(cartItem.Lot.Id)
                    ?? throw new KeyNotFoundException($"Lot with id '{cartItem.Lot.Id}' not found.");

                if (lot.StockCount < cartItem.Quantity)
                {
                    throw new InvalidOperationException($"Lot '{lot.Name}' does not have enough stock.");
                }

                if (!lot.Price.Equals(cartItem.Lot.Price))
                {
                    throw new InvalidOperationException($"Price for lot '{lot.Name}' has changed. Please refresh your cart.");
                }

                pricedItems.Add(PricedItem.FromLot(lot, cartItem.Quantity, paymentInfo.TaxRate));
            }

            var order = Order.Create(userId, shippingInfo, paymentInfo, pricedItems);

            await _ordersRepository.Create(order);

            var paymentUrl = await _paymentGateway.CreatePaymentUrlAsync(order, paymentInfo);

            await _cartRepository.ClearCart(userId);

            await _publishEndpoint.Publish(new OrderCreated(order.Id, order.Total));

            return new CheckoutResponse
            {
                OrderId = order.Id,
                PaymentUrl = paymentUrl
            };
        }

        public async Task HandlePaymentWebhookAsync(PaymentWebhookRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!request.IsSuccess)
            {
                _logger.LogWarning("Received failed payment webhook for order {OrderId}", request.OrderId);
                return;
            }

            var userId = await _ordersRepository.MarkPaid(request.OrderId, request.Provider, request.Reference);
            await _notificationService.NotifyOrderPaidAsync(request.OrderId);
        }

        private static ShippingInfo ToShippingInfo(ShippingInfoDto dto)
        {
            if (dto is null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            return new ShippingInfo(
                dto.RecipientName,
                dto.Line1,
                dto.Line2,
                dto.City,
                dto.State,
                dto.PostalCode,
                dto.Country,
                dto.Phone);
        }

        private static PaymentInfo ToPaymentInfo(PaymentInfoDto dto)
        {
            if (dto is null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            return new PaymentInfo(dto.Provider, dto.TaxRate, dto.ReturnUrl);
        }

        private Guid GetCurrentUserIdOrThrow()
        {
            var sub = _user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrWhiteSpace(sub))
            {
                throw new InvalidOperationException("User id is not available in the current principal.");
            }

            return Guid.Parse(sub);
        }
    }
}