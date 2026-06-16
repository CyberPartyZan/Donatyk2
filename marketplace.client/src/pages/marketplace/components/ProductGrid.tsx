import { useState } from 'react';
import { Link } from 'react-router-dom';
import { products } from '../../../mocks/products';
import AuctionCountdown from './AuctionCountdown';

interface ProductGridProps {
    filters: any;
    sortBy: string;
}

export default function ProductGrid({ filters, sortBy }: ProductGridProps) {
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 12;

    const filteredProducts = products.filter((product) => {
        if (filters.minPrice && product.price < parseFloat(filters.minPrice)) return false;
        if (filters.maxPrice && product.price > parseFloat(filters.maxPrice)) return false;
        if (filters.minDiscount && product.discount < parseFloat(filters.minDiscount)) return false;
        if (filters.maxDiscount && product.discount > parseFloat(filters.maxDiscount)) return false;
        if (filters.lotType.length > 0 && !filters.lotType.includes(product.lotType)) return false;
        return true;
    });

    const sortedProducts = [...filteredProducts].sort((a, b) => {
        if (sortBy === 'price-low') return a.price - b.price;
        if (sortBy === 'price-high') return b.price - a.price;
        if (sortBy === 'date') return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        return 0;
    });

    const totalPages = Math.ceil(sortedProducts.length / itemsPerPage);
    const displayedProducts = sortedProducts.slice(0, currentPage * itemsPerPage);

    const loadMore = () => {
        if (currentPage < totalPages) {
            setCurrentPage(currentPage + 1);
        }
    };

    return (
        <div>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6" data-product-shop>
                {displayedProducts.map((product) => (
                    <Link
                        key={product.id}
                        to={`/marketplace/item/${product.id}`}
                        className="bg-white rounded-lg shadow-sm hover:shadow-lg transition-shadow overflow-hidden group cursor-pointer"
                    >
                        <div className="relative h-64 bg-gray-100 overflow-hidden">
                            <img
                                src={product.image}
                                alt={product.name}
                                className="w-full h-full object-cover object-top group-hover:scale-105 transition-transform duration-300"
                            />
                            {product.discount > 0 && (
                                <div className="absolute top-3 right-3 px-3 py-1 bg-red-500 text-white text-sm font-bold rounded-full whitespace-nowrap">
                                    -{product.discount}%
                                </div>
                            )}
                            {product.lotType === 'auction' && (
                                <div className="absolute top-3 left-3 flex flex-col gap-1.5 items-start">
                                    <div className="px-3 py-1 bg-amber-500 text-white text-sm font-medium rounded-full whitespace-nowrap">
                                        Auction
                                    </div>
                                    {product.auctionEndsAt && (
                                        <AuctionCountdown endsAt={product.auctionEndsAt} />
                                    )}
                                </div>
                            )}
                            {product.lotType === 'draw' && (
                                <div className="absolute top-3 left-3 px-3 py-1 bg-purple-500 text-white text-sm font-medium rounded-full whitespace-nowrap">
                                    Draw
                                </div>
                            )}
                        </div>
                        <div className="p-4">
                            <h3 className="font-semibold text-gray-900 mb-2 line-clamp-2">{product.name}</h3>
                            <div className="flex items-center justify-between mb-3">
                                <div>
                                    {product.lotType === 'draw' ? (
                                        <div className="flex flex-col gap-0.5">
                                            <span className="text-xl font-bold text-teal-500">
                                                ${(product.ticketPrice ?? 1).toFixed(2)} / ticket
                                            </span>
                                            <span className="text-sm text-gray-500">
                                                {Math.max(0, Math.floor(product.price / (product.ticketPrice ?? 1)) - (product.ticketsSold ?? 0))} tickets left
                                            </span>
                                        </div>
                                    ) : product.discount > 0 ? (
                                        <div className="flex items-center gap-2">
                                            <span className="text-xl font-bold text-teal-500">
                                                ${(product.price * (1 - product.discount / 100)).toFixed(2)}
                                            </span>
                                            <span className="text-sm text-gray-400 line-through">
                                                ${product.price.toFixed(2)}
                                            </span>
                                        </div>
                                    ) : (
                                        <span className="text-xl font-bold text-teal-500">
                                            ${product.price.toFixed(2)}
                                        </span>
                                    )}
                                </div>
                            </div>
                            <button className="w-full py-2.5 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors text-sm font-medium whitespace-nowrap cursor-pointer">
                                {product.lotType === 'auction' ? 'Place Bid' : product.lotType === 'draw' ? 'Enter Draw' : 'Add to Cart'}
                            </button>
                        </div>
                    </Link>
                ))}
            </div>

            {currentPage < totalPages && (
                <div className="mt-8 text-center">
                    <button
                        onClick={loadMore}
                        className="px-8 py-3 bg-white border-2 border-teal-500 text-teal-500 rounded-lg hover:bg-teal-50 transition-colors font-semibold whitespace-nowrap cursor-pointer"
                    >
                        Load More
                    </button>
                </div>
            )}

            {displayedProducts.length === 0 && (
                <div className="text-center py-16">
                    <i className="ri-inbox-line text-6xl text-gray-300 mb-4"></i>
                    <p className="text-gray-500 text-lg">No products found matching your filters</p>
                </div>
            )}
        </div>
    );
}