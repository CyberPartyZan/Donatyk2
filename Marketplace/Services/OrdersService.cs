using Marketplace.Abstractions;
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
        private readonly ITicketsService _ticketsService;
        private readonly IBidsService _bidsService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ILogger<OrdersService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersService(
            ClaimsPrincipal user,
            ICartRepository cartRepository,
            ILotsRepository lotsRepository,
            IOrdersRepository ordersRepository,
            ITicketsService ticketsService,
            IBidsService bidsService,
            IPaymentGateway paymentGateway,
            IPublishEndpoint publishEndpoint,
            ILogger<OrdersService> logger)
        {
            _user = user;
            _cartRepository = cartRepository;
            _lotsRepository = lotsRepository;
            _ordersRepository = ordersRepository;
            _ticketsService = ticketsService;
            _bidsService = bidsService;
            _paymentGateway = paymentGateway;
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

                if (!lot.Price.Equals(cartItem.Lot.Price))
                {
                    throw new InvalidOperationException($"Price for lot '{lot.Name}' has changed. Please refresh your cart.");
                }

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

            var paymentUrl = await _paymentGateway.CreatePaymentUrlAsync(order, paymentInfo);

            await _cartRepository.ClearCart(userId);

            await _publishEndpoint.Publish(new OrderCreated(order.Id, order.Total));

            return new CheckoutResponse
            {
                OrderId = order.Id,
                PaymentUrl = paymentUrl
            };
        }

        public async Task<CheckoutResponse> CheckoutDrawAsync(CheckoutDrawRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.LotId == Guid.Empty)
            {
                throw new ArgumentException("Lot id must be provided.", nameof(request.LotId));
            }

            if (request.TicketsCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(request.TicketsCount), "Tickets count must be greater than zero.");
            }

            var userId = GetCurrentUserIdOrThrow();

            var lot = await _lotsRepository.GetLotById(request.LotId)
                ?? throw new KeyNotFoundException($"Lot with id '{request.LotId}' not found.");

            var drawLot = lot as DrawLot
                ?? throw new InvalidOperationException($"Lot '{lot.Name}' is not a draw lot.");

            var shippingInfo = ToShippingInfo(request.Shipping);
            var paymentInfo = ToPaymentInfo(request.Payment);

            await _ticketsService.Create(drawLot.Id, request.TicketsCount);

            var pricedItem = PricedItem.FromCustomPrice(
                drawLot.Id,
                $"{drawLot.Name} ticket",
                drawLot.TicketPrice,
                request.TicketsCount,
                paymentInfo.TaxRate);

            var order = Order.Create(userId, shippingInfo, paymentInfo, new[] { pricedItem });

            await _ordersRepository.Create(order);

            // TODO: Remove tickets from the lot if payment is not completed within a certain time frame
            var paymentUrl = await _paymentGateway.CreatePaymentUrlAsync(order, paymentInfo);

            await _publishEndpoint.Publish(new OrderCreated(order.Id, order.Total));

            return new CheckoutResponse
            {
                OrderId = order.Id,
                PaymentUrl = paymentUrl
            };
        }

        public async Task<CheckoutResponse> CheckoutAuctionAsync(CheckoutAuctionRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.LotId == Guid.Empty)
            {
                throw new ArgumentException("Lot id must be provided.", nameof(request.LotId));
            }

            if (request.Amount is null)
            {
                throw new ArgumentNullException(nameof(request.Amount));
            }

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

            var paymentUrl = await _paymentGateway.CreatePaymentHoldUrlAsync(order, paymentInfo);

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
                await _publishEndpoint.Publish(new PaymentProcessed(request.OrderId, false));
                _logger.LogWarning("Received failed payment webhook for order {OrderId}", request.OrderId);
                return;
            }

            await _ordersRepository.MarkPaid(request.OrderId, request.Provider, request.Reference);

            await _publishEndpoint.Publish(new PaymentProcessed(request.OrderId, true));
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