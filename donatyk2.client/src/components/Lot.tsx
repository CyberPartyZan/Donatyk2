import { Link } from 'react-router';
import lotImage from '../assets/igrushka-vyazanaya-svinka.webp';
import './Lot.css';

export const Lot = () => {
  return (
    <div className='lot-card'>
      <Link className='lot-href' to='/lot'>
        <img className='lot-image' src={lotImage} alt='' />
        <div className='lot-description-container'>
          <p className='lot-name'>
            Knitted teddy bear Knitted teddy bear Knitted teddy bear
          </p>
          <p className='lot-description'>
            Soft and cool teddy bear Soft and cool teddy bear Soft and cool
            teddy bear Soft and cool teddy bear Soft and cool teddy bear Soft
            and cool teddy bear Soft and cool teddy bear
          </p>
          <p className='lot-price'>2200 ₴</p>
        </div>
      </Link>
      <button>Add to cart</button>
    </div>
  );
};
