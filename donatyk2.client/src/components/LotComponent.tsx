import type { Lot, AuctionLot, DrawLot, SimpleLot } from '../data.ts';

const LotComponent = (props: Lot) => {
  const { seller, name, description, price, type } = props;
  const endOfAuction =
    type === 'auction' ? (props as AuctionLot).endOfAuction : undefined;
  const ticketPrice =
    type === 'draw' ? (props as DrawLot).ticketPrice : undefined;

  return (
    <section>
      <div className='lot-image-container'></div>
      <div className='lot-description-container'>
        {type === 'auction' && (
          <h3>End of auction: {endOfAuction?.toDateString()}</h3>
        )}
        <h3>Seller: {seller.name}</h3>
        <h3>{name}</h3>
        <h3>{description}</h3>
        <h3>Price: {price}</h3>
        {ticketPrice && <h3>Ticket price: {ticketPrice}</h3>}
      </div>
    </section>
  );
};

export default LotComponent;
