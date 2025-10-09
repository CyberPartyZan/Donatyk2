import { Lot } from './Lot';
import './LotGrid.css';

export const LotGrid = () => {
  return (
    <main>
      <div className='lot-menu'>
        <div className='lot-category'>
          <a href=''>Kids</a> - <a href=''>Knitted</a>
        </div>
        <div className='lot-sort'>
          <select name='sort' id='sort'>
            <option value='1'>By name</option>
            <option value='2'>By rating</option>
            <option value='3'>By price</option>
          </select>
        </div>
      </div>
      <div className='main-container'>
        <Lot />
        <Lot />
        <Lot />
        <Lot />
        <Lot />
        <Lot />
        <Lot />
        <Lot />
        <Lot />
        <Lot />
        <Lot />
        <Lot />
      </div>
    </main>
  );
};
