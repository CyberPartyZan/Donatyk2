import { Header } from '../components/Header';
import { Footer } from '../components/Footer';
import { LotGrid } from '../components/LotGrid';

export const HomePage = () => {
  return (
    <div className='main'>
      <Header />

      <LotGrid />

      <Footer />
    </div>
  );
};
