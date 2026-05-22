using Marketplace.Payment;
using Marketplace.Repository;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Marketplace
{
    public class BidsService : IBidsService
    {
        private readonly ClaimsPrincipal _user;
        private readonly ILotsRepository _lotsRepository;
        private readonly IBidsRepository _bidsRepository;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IPaymentGatewayFactory _paymentGatewayFactory;

        public BidsService(
            ClaimsPrincipal user,
            ILotsRepository lotsRepository,
            IBidsRepository bidsRepository,
            IOrdersRepository ordersRepository,
            IPaymentGatewayFactory paymentGatewayFactory)
        {
            _user = user;
            _lotsRepository = lotsRepository;
            _bidsRepository = bidsRepository;
            _ordersRepository = ordersRepository;
            _paymentGatewayFactory = paymentGatewayFactory;
        }

        public Task<IReadOnlyCollection<Bid>> LoadBidHistory(Guid lotId)
        {
            return _bidsRepository.LoadBidHistory(lotId);
        }

        public async Task<Bid> PlaceBid(Guid lotId, Money amount)
        {
            var lot = await _lotsRepository.GetLotById(lotId)
                ?? throw new KeyNotFoundException($"Lot '{lotId}' not found.");

            var auction = lot as AuctionLot
                ?? throw new InvalidOperationException("Bid can be placed only for auction lot.");

            var history = await _bidsRepository.LoadBidHistory(lotId);
            auction.LoadBidHistory(history);

            var bidderId = GetCurrentUserIdOrThrow();
            var bid = auction.Bid(bidderId, amount);

            // Release the hold for the previously outbid bidder (if any paid hold order exists)
            var previousHoldOrder = await _ordersRepository.GetPaidOrderByLotId(lotId);
            if (previousHoldOrder is not null)
            {
                await _paymentGatewayFactory.CreatePaymentGateway(previousHoldOrder.PaymentInfo.Provider)
                    .ReleaseHoldAsync(previousHoldOrder);
            }

            await _bidsRepository.PlaceBid(bid);
            await _lotsRepository.UpdateLot(lotId, auction);

            return bid;
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