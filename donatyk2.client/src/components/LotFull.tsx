import { Footer } from './Footer';
import { Header } from './Header';
import lotImage from '../assets/igrushka-vyazanaya-svinka.webp';
import './LotFull.css';

export const LotFull = () => {
  return (
    <div className='main'>
      <Header />

      <main>
        <div className='lot-menu'>
          <div className='lot-category'>
            <a href=''>Kids</a> - <a href=''>Knitted</a>
          </div>
        </div>
        <div className='lot-full'>
          <div className='lot-full-image-container'>
            <img src={lotImage} alt='' />
          </div>
          <div className='details-container'>
            <div className='name card'>
              Knitted teddy bear Knitted teddy bear Knitted teddy bear
            </div>
            <div className='description card'>
              Soft and cool teddy bear Soft and cool teddy bear Soft and cool
              teddy bear Soft and cool teddy bear Soft and cool teddy bear Soft
              and cool teddy bear Soft and cool teddy bear
            </div>
            <div className='card'>Dimensions: 10cm x 10cm x 10cm</div>
            <div className='card'>Material: Cotton</div>
            <div className='seller-info card'>
              <div className='seller-name'>
                Produced By: <a href=''>Yara</a>
              </div>
              <div className='buy-info'>
                <span className='price'>2200 ₴</span>
                <button>Add to cart</button>
              </div>
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
};
