export interface Seller {
  id: string;
  name: string;
}

export type SimpleLot = {
  id: string;
  name: string;
  description: string;
  price: number;
  seller: Seller;
  images: string[];
  type: 'simple';
};

export type AuctionLot = {
  id: string;
  name: string;
  description: string;
  price: number;
  seller: Seller;
  images: string[];
  type: 'auction';
  endOfAuction: Date;
  auctionStep: number;
};

export type DrawLot = {
  id: string;
  name: string;
  description: string;
  price: number;
  seller: Seller;
  images: string[];
  type: 'draw';
  ticketPrice: number;
};

export type Lot = SimpleLot | AuctionLot | DrawLot;

export const lots: Lot[] = [
  {
    id: 'guid-1',
    name: 'RPG Tubus',
    description: 'Used tubus of RPG',
    price: 8500,
    seller: {
      id: 'guid-1',
      name: 'Oleksandr Yanchak',
    },
    type: 'simple',
    images: [],
  },
  {
    id: 'guid-2',
    name: 'Knitted teddy bear',
    description: 'Soft and cool teddy bear',
    price: 2200,
    seller: {
      id: 'guid-2',
      name: 'Knitting studio',
    },
    type: 'auction',
    endOfAuction: new Date('2025-10-26T10:30:00Z'),
    auctionStep: 2,
    images: [],
  },
  {
    id: 'guid-3',
    name: 'Painting',
    description: 'The original oil painting of human body',
    price: 28000,
    seller: {
      id: 'guid-3',
      name: 'Yara Art',
    },
    type: 'draw',
    ticketPrice: 100,
    images: [],
  },
];
