import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { mockSellers } from '@/mocks/sellers';
import { mockLots, mockCategories } from '@/mocks/lots';
import AuctionCountdown from '@/pages/marketplace/components/AuctionCountdown';

export default function SellerPage() {
    const { id } = useParams<{ id: string }>();
    const seller = mockSellers.find(s => s.id === id);
    const [activeFilter, setActiveFilter] = useState<'All' | 'Simple' | 'Auction' | 'Draw'>('All');
    const [sortBy, setSortBy] = useState('date');
    const [searchQuery, setSearchQuery] = useState('');
    const [activeSearchQuery, setActiveSearchQuery] = useState('');

    if (!seller) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <div className="w-20 h-20 flex items-center justify-center mx-auto mb-6">
                        <i className="ri-store-2-line text-7xl text-gray-300"></i>
                    </div>
                    <h1 className="text-2xl font-bold text-gray-900 mb-2">Seller Not Found</h1>
                    <p className="text-gray-600 mb-6">The seller you're looking for doesn't exist or has been removed.</p>
                    <Link to="/marketplace" className="px-6 py-2.5 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap">
                        Back to Marketplace
                    </Link>
                </div>
            </div>
        );
    }

    const sellerLots = mockLots.filter(l => l.sellerId === seller.id && l.stage === 'Approved' && l.isActive);

    const filteredLots = sellerLots.filter(lot => {
        const matchesSearch = lot.name.toLowerCase().includes(activeSearchQuery.toLowerCase()) ||
            lot.description.toLowerCase().includes(activeSearchQuery.toLowerCase());
        const matchesType = activeFilter === 'All' || lot.type === activeFilter;
        return matchesSearch && matchesType;
    });

    const sortedLots = [...filteredLots].sort((a, b) => {
        if (sortBy === 'price-low') return a.price - b.price;
        if (sortBy === 'price-high') return b.price - a.price;
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
    });

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Header */}
            <header className="bg-white shadow-sm sticky top-0 z-40">
                <div className="max-w-[1600px] mx-auto px-6 py-4">
                    <div className="flex items-center justify-between">
                        <Link to="/marketplace" className="flex items-center gap-2 text-gray-600 hover:text-gray-900 transition-colors cursor-pointer">
                            <i className="ri-arrow-left-line"></i>
                            <span className="text-sm font-medium">Back to Marketplace</span>
                        </Link>
                        <Link to="/">
                            <img
                                src="https://public.readdy.ai/ai/img_res/fd4376ec-da1c-49df-8529-c6f422339bdf.png"
                                alt="Logo"
                                className="h-10 w-auto"
                            />
                        </Link>
                        <div className="w-32"></div>
                    </div>
                </div>
            </header>

            {/* Seller Profile */}
            <div className="max-w-[1600px] mx-auto px-6 py-8">
                <div className="bg-white rounded-lg border border-gray-200 overflow-hidden mb-8">
                    <div className="p-8">
                        <div className="flex items-start gap-6">
                            <div className="w-28 h-28 rounded-xl bg-gray-100 overflow-hidden flex-shrink-0 border-2 border-gray-100">
                                <img src={seller.avatarImage} alt={seller.name} className="w-full h-full object-cover object-top" />
                            </div>
                            <div className="flex-1">
                                <h1 className="text-2xl font-bold text-gray-900 mb-1">{seller.name}</h1>
                                <div className="flex items-center gap-2 mb-3">
                                    <div className="flex items-center">
                                        <i className="ri-star-fill text-amber-400 text-sm"></i>
                                        <span className="text-sm font-semibold text-gray-900 ml-1">{seller.rating}</span>
                                    </div>
                                    <span className="text-gray-300">•</span>
                                    <span className="text-sm text-gray-600">{seller.totalSales} sales</span>
                                    <span className="text-gray-300">•</span>
                                    <span className="text-sm text-gray-600">{seller.totalLots} lots listed</span>
                                </div>
                                <p className="text-sm text-gray-600 leading-relaxed">{seller.description}</p>
                                <div className="flex items-center gap-4 mt-4">
                                    <span className="flex items-center gap-1.5 text-sm text-gray-500">
                                        <i className="ri-mail-line"></i>{seller.email}
                                    </span>
                                    <span className="flex items-center gap-1.5 text-sm text-gray-500">
                                        <i className="ri-phone-line"></i>{seller.phoneNumber}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="grid grid-cols-4 border-t border-gray-200">
                        <div className="px-6 py-4 border-r border-gray-200">
                            <div className="text-2xl font-bold text-gray-900">{seller.totalLots}</div>
                            <div className="text-xs text-gray-600 mt-1">Total Lots</div>
                        </div>
                        <div className="px-6 py-4 border-r border-gray-200">
                            <div className="text-2xl font-bold text-teal-600">{seller.approvedLots}</div>
                            <div className="text-xs text-gray-600 mt-1">Approved</div>
                        </div>
                        <div className="px-6 py-4 border-r border-gray-200">
                            <div className="text-2xl font-bold text-gray-900">{seller.totalSales}</div>
                            <div className="text-xs text-gray-600 mt-1">Total Sales</div>
                        </div>
                        <div className="px-6 py-4">
                            <div className="text-2xl font-bold text-amber-500">{seller.rating}</div>
                            <div className="text-xs text-gray-600 mt-1">Rating</div>
                        </div>
                    </div>
                </div>

                {/* Lots Section */}
                <div className="bg-white rounded-lg border border-gray-200 p-6 mb-6">
                    <div className="flex items-center justify-between mb-4">
                        <h2 className="text-lg font-semibold text-gray-900">Listings ({sortedLots.length})</h2>
                        <div className="flex items-center gap-3">
                            <div className="flex items-center gap-2">
                                <span className="text-sm text-gray-600 whitespace-nowrap">Sort:</span>
                                <select
                                    value={sortBy}
                                    onChange={e => setSortBy(e.target.value)}
                                    className="px-3 py-1.5 border border-gray-300 rounded-lg text-sm cursor-pointer focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                >
                                    <option value="date">Date</option>
                                    <option value="price-low">Price: Low-High</option>
                                    <option value="price-high">Price: High-Low</option>
                                </select>
                            </div>
                            <div className="flex items-center gap-1 bg-gray-100 rounded-full p-1">
                                {(['All', 'Simple', 'Auction', 'Draw'] as const).map(t => (
                                    <button
                                        key={t}
                                        onClick={() => setActiveFilter(t)}
                                        className={`px-3 py-1.5 text-xs font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${activeFilter === t ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'
                                            }`}
                                    >
                                        {t}
                                    </button>
                                ))}
                            </div>
                        </div>
                    </div>

                    {/* Search bar */}
                    <div className="flex items-center gap-2">
                        <div className="flex-1 relative">
                            <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                            <input
                                type="text"
                                placeholder="Search listings..."
                                value={searchQuery}
                                onChange={(e) => setSearchQuery(e.target.value)}
                                onKeyDown={(e) => { if (e.key === 'Enter') setActiveSearchQuery(searchQuery); }}
                                className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                            />
                        </div>
                        <button
                            onClick={() => setActiveSearchQuery(searchQuery)}
                            className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                        >
                            <i className="ri-search-line"></i>
                            Search
                        </button>
                    </div>
                </div>

                {sortedLots.length === 0 ? (
                    <div className="text-center py-16">
                        <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                            <i className="ri-inbox-line text-5xl text-gray-300"></i>
                        </div>
                        <p className="text-gray-500">No active listings from this seller</p>
                    </div>
                ) : (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6" data-product-shop>
                        {sortedLots.map(lot => (
                            <Link
                                key={lot.id}
                                to={`/marketplace/item/${lot.id}`}
                                className="bg-white rounded-lg border border-gray-200 hover:shadow-lg transition-shadow overflow-hidden group cursor-pointer"
                            >
                                <div className="relative h-56 bg-gray-100 overflow-hidden">
                                    <LottImage lotId={lot.id} lotName={lot.name} />
                                    {lot.type === 'Auction' && (
                                        <div className="absolute top-3 left-3 flex flex-col gap-1.5">
                                            <div className="px-3 py-1 bg-amber-500 text-white text-sm font-medium rounded-full whitespace-nowrap">Auction</div>
                                            {lot.endOfAuction && <AuctionCountdown endsAt={lot.endOfAuction} />}
                                        </div>
                                    )}
                                    {lot.type === 'Draw' && (
                                        <div className="absolute top-3 left-3 px-3 py-1 bg-purple-500 text-white text-sm font-medium rounded-full whitespace-nowrap">Draw</div>
                                    )}
                                </div>
                                <div className="p-4">
                                    <h3 className="font-semibold text-gray-900 mb-2 line-clamp-2">{lot.name}</h3>
                                    <div className="flex items-center justify-between mb-3">
                                        {lot.type === 'Draw' ? (
                                            <div className="flex flex-col gap-0.5">
                                                <span className="text-xl font-bold text-teal-500">${(lot.ticketPrice ?? 1).toFixed(2)} / ticket</span>
                                                <span className="text-sm text-gray-500">{Math.max(0, (lot.totalTickets ?? 0) - (lot.ticketsSold ?? 0))} tickets left</span>
                                            </div>
                                        ) : lot.discountedPrice ? (
                                            <div className="flex items-center gap-2">
                                                <span className="text-xl font-bold text-teal-500">${lot.discountedPrice.toFixed(2)}</span>
                                                <span className="text-sm text-gray-400 line-through">${lot.price.toFixed(2)}</span>
                                            </div>
                                        ) : (
                                            <span className="text-xl font-bold text-teal-500">${lot.price.toFixed(2)}</span>
                                        )}
                                    </div>
                                    <button className="w-full py-2.5 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors text-sm font-medium whitespace-nowrap cursor-pointer">
                                        {lot.type === 'Auction' ? 'Place Bid' : lot.type === 'Draw' ? 'Enter Draw' : 'View Details'}
                                    </button>
                                </div>
                            </Link>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}

function LottImage({ lotId }: { lotId: string; lotName: string }) {
    const images: Record<string, string> = {
        '1': 'https://readdy.ai/api/search-image?query=vintage%20rolex%20submariner%20luxury%20watch%20product%20photography%20clean%20white%20background%20professional%20ecommerce%20style&width=400&height=400&seq=seller-lot-1&orientation=squarish',
        '2': 'https://readdy.ai/api/search-image?query=apple%20macbook%20pro%2016%20inch%20laptop%20product%20photography%20clean%20white%20background%20professional%20ecommerce%20style&width=400&height=400&seq=seller-lot-2&orientation=squarish',
        '3': 'https://readdy.ai/api/search-image?query=tesla%20model%203%20performance%20electric%20car%20product%20photography%20clean%20white%20background%20professional%20ecommerce%20style&width=400&height=400&seq=seller-lot-3&orientation=squarish',
        '4': 'https://readdy.ai/api/search-image?query=hermes%20birkin%20bag%20luxury%20handbag%20product%20photography%20clean%20white%20background%20professional%20ecommerce%20style&width=400&height=400&seq=seller-lot-4&orientation=squarish',
        '5': 'https://readdy.ai/api/search-image?query=sony%20playstation%205%20pro%20bundle%20gaming%20console%20product%20photography%20clean%20white%20background%20professional%20ecommerce%20style&width=400&height=400&seq=seller-lot-5&orientation=squarish',
        '6': 'https://readdy.ai/api/search-image?query=luxury%20yacht%20mediterranean%20experience%20product%20photography%20clean%20white%20background%20professional%20ecommerce%20style&width=400&height=400&seq=seller-lot-6&orientation=squarish',
        '7': 'https://readdy.ai/api/search-image?query=patek%20philippe%20nautilus%20luxury%20watch%20product%20photography%20clean%20white%20background%20professional%20ecommerce%20style&width=400&height=400&seq=seller-lot-7&orientation=squarish',
    };
    const src = images[lotId] || images['1'];
    return <img src={src} alt="Lot" className="w-full h-full object-cover object-top group-hover:scale-105 transition-transform duration-300" />;
}