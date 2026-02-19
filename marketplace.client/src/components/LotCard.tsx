import type { Lot, AuctionLot, DrawLot, SimpleLot } from '../data.ts';

const LotCard = (props: Lot) => {
  const { seller, name, description, price, type, images } = props;
  const endOfAuction =
    type === 'auction' ? (props as AuctionLot).endOfAuction : undefined;
  const ticketPrice =
    type === 'draw' ? (props as DrawLot).ticketPrice : undefined;

  return (
    <section>
      {type === 'auction' && (
        <h3>End of auction: {endOfAuction?.toDateString()}</h3>
      )}
      <div className='card-image-container'>
        <img src={images[0]} alt={name} className='img-card' />
      </div>
      <div className='card-description-container'>
        <h3>Seller: {seller.name}</h3>
        <h3>{name}</h3>
        <h3>{description}</h3>
        <h3>Price: {price}</h3>
        {ticketPrice && <h3>Ticket price: {ticketPrice}</h3>}
      </div>
    </section>
  );
};

export default LotCard;
