using Marketplace.Abstractions;
using Marketplace.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Marketplace
{
    public class CheckAuctionEndedJob
    {
        private readonly ILotsRepository _lotsRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CheckAuctionEndedJob> _logger;

        public CheckAuctionEndedJob(
            ILotsRepository lotsRepository,
            IPublishEndpoint publishEndpoint,
            ILogger<CheckAuctionEndedJob> logger)
        {
            _lotsRepository = lotsRepository;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var endedLots = await _lotsRepository.GetEndedAuctionLots(cancellationToken);

            foreach (var lot in endedLots)
            {
                if (!lot.BidHistory.Any())
                {
                    _logger.LogInformation(
                        "Auction lot {LotId} has ended with no bids. Skipping.",
                        lot.Id);
                    continue;
                }

                _logger.LogInformation(
                    "Auction lot {LotId} has ended. Publishing AuctionEnded event.",
                    lot.Id);

                await _publishEndpoint.Publish(new AuctionEnded(lot.Id), cancellationToken);
            }
        }
    }
}