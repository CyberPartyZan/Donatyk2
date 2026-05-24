using Marketplace.Abstractions;
using Marketplace.Payment;
using Marketplace.Repository;
using MassTransit;
using Microsoft.Extensions.Configuration;
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
        private readonly ITicketsService _ticketsService;
        private readonly IBidsService _bidsService;
        private readonly IPaymentGatewayFactory _paymentGatewayFactory;
        private readonly ILogger<OrdersService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly string _apiBaseUrl;

        public OrdersService(
            ClaimsPrincipal user,
            ICartRepository cartRepository,
            ILotsRepository lotsRepository,
            IOrdersRepository ordersRepository,
            ITicketsService ticketsService,
            IBidsService bidsService,
            IPaymentGatewayFactory paymentGatewayFactory,
            IPublishEndpoint publishEndpoint,
            IConfiguration configuration,
            ILogger<OrdersService> logger)
        {
            _user = user;
            _cartRepository = cartRepository;
            _lotsRepository = lotsRepository;
            _ordersRepository = ordersRepository;
            _ticketsService = ticketsService;
            _bidsService = bidsService;
            _paymentGatewayFactory = paymentGatewayFactory;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _apiBaseUrl = configuration.GetValue<string>("Api:BaseUrl") ?? "https://api.local";
        }

        public async Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var userId = GetCurrentUserIdOrThrow();

            var cart = await _cartRepository.GetCartByUserId(userId);
            var cartItems = cart.Items.ToList();

            if (cartItems.Count == 0)
                throw new InvalidOperationException("Cart is empty.");

            var shippingInfo = ToShippingInfo(request.Shipping);
            var paymentInfo = ToPaymentInfo(request.Payment);
            var pricedItems = new List<PricedItem>(cartItems.Count);

            foreach (var cartItem in cartItems)
            {
                var lot = await _lotsRepository.GetLotById(cartItem.Lot.Id)
                    ?? throw new KeyNotFoundException($"Lot with id '{cartItem.Lot.Id}' not found.");

                if (!lot.Price.Equals(cartItem.Lot.Price))
                    throw new InvalidOperationException($"Price for lot '{lot.Name}' has changed. Please refresh your cart.");

                switch (lot)
                {
                    case DrawLot:
                        throw new InvalidOperationException(
                            $"Draw lot '{lot.Name}' cannot be checked out from cart. Create tickets via Tickets API.");

                    case AuctionLot:
                        throw new InvalidOperationException(
                            $"Auction lot '{lot.Name}' cannot be checked out from cart. Place bids via Bids API.");

                    default:
                        lot.Sell(cartItem.Quantity);
                        await _lotsRepository.UpdateLot(lot.Id, lot);
                        break;
                }

                pricedItems.Add(PricedItem.FromLot(lot, cartItem.Quantity, paymentInfo.TaxRate));
            }

            var order = Order.Create(userId, shippingInfo, paymentInfo, pricedItems);

            await _ordersRepository.Create(order);

            var paymentUrl = await _paymentGatewayFactory.CreatePaymentGateway(paymentInfo.Provider)
                .CreatePaymentUrlAsync(order, paymentInfo);

            await _cartRepository.ClearCart(userId);

            await _publishEndpoint.Publish(new OrderCreated(order.Id, order.Total));

            return new CheckoutResponse { OrderId = order.Id, PaymentUrl = paymentUrl };
        }

        public async Task<CheckoutResponse> CheckoutDrawAsync(CheckoutDrawRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.LotId == Guid.Empty)
                throw new ArgumentException("Lot id must be provided.", nameof(request.LotId));

            if (request.TicketsCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(request.TicketsCount), "Tickets count must be greater than zero.");

            var userId = GetCurrentUserIdOrThrow();

            var lot = await _lotsRepository.GetLotById(request.LotId)
                ?? throw new KeyNotFoundException($"Lot with id '{request.LotId}' not found.");

            var drawLot = lot as DrawLot
                ?? throw new InvalidOperationException($"Lot '{lot.Name}' is not a draw lot.");

            var shippingInfo = ToShippingInfo(request.Shipping);

            var drawWebhookReturnUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/orders/payment/draw/webhook?lotId={drawLot.Id}";
            var paymentInfo = new PaymentInfo(request.Payment.Provider, request.Payment.TaxRate, drawWebhookReturnUrl);

            await _ticketsService.Create(drawLot.Id, request.TicketsCount);

            var pricedItem = PricedItem.FromCustomPrice(
                drawLot.Id,
                $"{drawLot.Name} ticket",
                drawLot.TicketPrice,
                request.TicketsCount,
                paymentInfo.TaxRate);

            var order = Order.Create(userId, shippingInfo, paymentInfo, new[] { pricedItem });

            await _ordersRepository.Create(order);

            var paymentUrl = await _paymentGatewayFactory.CreatePaymentGateway(paymentInfo.Provider)
                .CreatePaymentDrawUrlAsync(order, paymentInfo);

            await _publishEndpoint.Publish(new OrderCreated(order.Id, order.Total));

            return new CheckoutResponse { OrderId = order.Id, PaymentUrl = paymentUrl };
        }

        public async Task<CheckoutResponse> CheckoutAuctionAsync(CheckoutAuctionRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.LotId == Guid.Empty)
                throw new ArgumentException("Lot id must be provided.", nameof(request.LotId));

            if (request.Amount is null)
                throw new ArgumentNullException(nameof(request.Amount));

            var userId = GetCurrentUserIdOrThrow();

            var lot = await _lotsRepository.GetLotById(request.LotId)
                ?? throw new KeyNotFoundException($"Lot with id '{request.LotId}' not found.");

            var auctionLot = lot as AuctionLot
                ?? throw new InvalidOperationException($"Lot '{lot.Name}' is not an auction lot.");

            var shippingInfo = ToShippingInfo(request.Shipping);
            var paymentInfo = ToPaymentInfo(request.Payment);

            var bid = await _bidsService.PlaceBid(auctionLot.Id, request.Amount);

            var pricedItem = PricedItem.FromCustomPrice(
                auctionLot.Id,
                $"{auctionLot.Name} bid hold",
                bid.Amount,
                quantity: 1,
                taxRate: 0m);

            var order = Order.Create(userId, shippingInfo, paymentInfo, new[] { pricedItem });

            await _ordersRepository.Create(order);

            var paymentUrl = await _paymentGatewayFactory.CreatePaymentGateway(paymentInfo.Provider)
                .CreatePaymentAuctionUrlAsync(order, paymentInfo);

            await _publishEndpoint.Publish(new OrderCreated(order.Id, order.Total));

            return new CheckoutResponse { OrderId = order.Id, PaymentUrl = paymentUrl };
        }

        public async Task HandlePaymentWebhookAsync(PaymentWebhookRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (!request.IsSuccess)
            {
                await _publishEndpoint.Publish(new PaymentProcessed(request.OrderId, false));
                _logger.LogWarning("Received failed payment webhook for order {OrderId}", request.OrderId);
                return;
            }

            var order = await _ordersRepository.GetById(request.OrderId)
                ?? throw new KeyNotFoundException($"Order '{request.OrderId}' not found.");

            if (!string.Equals(order.PaymentInfo.Provider, request.Provider, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Payment provider does not match the order payment provider.");

            order.PaymentInfo.AttachReference(request.Reference);
            order.MarkPaid();
            await _ordersRepository.Update(order);

            await _publishEndpoint.Publish(new PaymentProcessed(request.OrderId, true));
        }

        public async Task HandleDrawPaymentWebhookAsync(DrawPaymentWebhookRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (!request.IsSuccess)
            {
                var order = await _ordersRepository.GetById(request.OrderId)
                    ?? throw new KeyNotFoundException($"Order '{request.OrderId}' not found.");

                foreach (var item in order.Items)
                {
                    await _ticketsService.CancelTicketsForUserOnLot(item.LotId, order.CustomerId, item.Quantity);
                }

                order.Cancel();
                await _ordersRepository.Update(order);

                _logger.LogWarning(
                    "Draw payment failed for order {OrderId}. Tickets and order cancelled.",
                    request.OrderId);
                return;
            }

            var paidOrder = await _ordersRepository.GetById(request.OrderId)
                ?? throw new KeyNotFoundException($"Order '{request.OrderId}' not found.");

            paidOrder.MarkPaid();
            await _ordersRepository.Update(paidOrder);
            await _ticketsService.MarkAsPayedByOrderId(request.OrderId);

            _logger.LogInformation(
                "Draw payment succeeded for order {OrderId}. Checking if lot {LotId} is ready to draw.",
                request.OrderId, request.LotId);

            var lot = await _lotsRepository.GetLotById(request.LotId);
            if (lot is DrawLot drawLot)
            {
                var tickets = await _ticketsService.GetAll(drawLot.Id);
                drawLot.LoadTickets(tickets);

                if (drawLot.ReadyToDraw)
                {
                    await _publishEndpoint.Publish(new DrawLaunched(drawLot.Id));

                    _logger.LogInformation(
                        "Draw lot {LotId} is ready to draw. DrawLaunched event published.",
                        drawLot.Id);
                }
            }
        }

        public async Task<Guid> MarkPaid(Guid orderId)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("Order id must be provided.", nameof(orderId));

            var order = await _ordersRepository.GetById(orderId)
                ?? throw new KeyNotFoundException($"Order '{orderId}' not found.");

            order.MarkPaid();
            await _ordersRepository.Update(order);

            return order.CustomerId;
        }

        private static ShippingInfo ToShippingInfo(ShippingInfoDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

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
                throw new ArgumentNullException(nameof(dto));

            return new PaymentInfo(dto.Provider, dto.TaxRate, dto.ReturnUrl);
        }

        private Guid GetCurrentUserIdOrThrow()
        {
            var sub = _user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidOperationException("User id is not available in the current principal.");

            return Guid.Parse(sub);
        }
    }
}