import { useParams, Link, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { products } from '../../mocks/products';
import { mockGoals } from '../../mocks/goals';
import MarketplaceHeader from '../marketplace/components/MarketplaceHeader';

function useAuctionCountdown(endsAt?: string) {
    const calcTimeLeft = () => {
        if (!endsAt) return null;
        const diff = new Date(endsAt).getTime() - Date.now();
        if (diff <= 0) return { days: 0, hours: 0, minutes: 0, seconds: 0, expired: true };
        return {
            days: Math.floor(diff / 86400000),
            hours: Math.floor((diff % 86400000) / 3600000),
            minutes: Math.floor((diff % 3600000) / 60000),
            seconds: Math.floor((diff % 60000) / 1000),
            expired: false,
        };
    };

    const [timeLeft, setTimeLeft] = useState(calcTimeLeft);

    useEffect(() => {
        if (!endsAt) return;
        const timer = setInterval(() => setTimeLeft(calcTimeLeft()), 1000);
        return () => clearInterval(timer);
    }, [endsAt]);

    return timeLeft;
}

export default function ItemDetails() {
    const comingSoon = true; // Set to true to hide content and show "Coming Soon" message

    const { id } = useParams();
    const navigate = useNavigate();
    const product = products.find((p) => p.id === id);
    const [selectedImage, setSelectedImage] = useState(0);
    const [quantity, setQuantity] = useState(1);
    const [activeTab, setActiveTab] = useState('about');
    const timeLeft = useAuctionCountdown(product?.auctionEndsAt);

    // Goal info
    const goalId = (product as any)?.goalId;
    const goal = goalId ? mockGoals.find(g => g.id === goalId) : null;

    // Auction bid state
    const currentHighestBid = product?.lotType === 'auction' ? product.price : 0;
    const minBid = Math.ceil(currentHighestBid * 1.05 * 100) / 100;
    const [bidAmount, setBidAmount] = useState(minBid);
    const [bidError, setBidError] = useState('');

    // Draw ticket state
    const ticketPrice = product?.ticketPrice ?? 1;
    const ticketsSold = product?.ticketsSold ?? 0;
    const totalTickets = product?.lotType === 'draw' ? Math.floor(product.price / ticketPrice) : 0;
    const ticketsLeft = totalTickets - ticketsSold;
    const [tickets, setTickets] = useState(1);

    const handleBidChange = (value: number) => {
        const rounded = Math.round(value * 100) / 100;
        setBidAmount(rounded);
        if (rounded < minBid) {
            setBidError(`Minimum bid is $${minBid.toFixed(2)} (current highest + 5%)`);
        } else {
            setBidError('');
        }
    };

    const incrementBid = () => handleBidChange(bidAmount + 1);
    const decrementBid = () => handleBidChange(Math.max(minBid, bidAmount - 1));

    const incrementTickets = () => setTickets((t) => Math.min(ticketsLeft, t + 1));
    const decrementTickets = () => setTickets((t) => Math.max(1, t - 1));

    const handleBidCheckout = () => {
        navigate(`/checkout?type=auction&lotId=${product.id}&bidAmount=${bidAmount.toFixed(2)}&lotName=${encodeURIComponent(product.name)}`);
    };

    const handleDrawCheckout = () => {
        navigate(`/checkout?type=draw&lotId=${product.id}&tickets=${tickets}&ticketPrice=${ticketPrice.toFixed(2)}&lotName=${encodeURIComponent(product.name)}`);
    };

    if (!product) {
        return <div className="min-h-screen flex items-center justify-center">Product not found</div>;
    }

    const images = [product.image, product.image, product.image, product.image];
    const finalPrice = product.price * (1 - product.discount / 100);
    const isUrgent = timeLeft && !timeLeft.expired && timeLeft.days === 0 && timeLeft.hours < 2;

    return (
        <div className="min-h-screen bg-gray-50">
            <MarketplaceHeader onCategoriesClick={() => { }} />

            <div className="max-w-[1600px] mx-auto px-6 py-8">
                {/* Breadcrumbs */}
                <div className="flex items-center gap-2 text-sm text-gray-600 mb-8">
                    <Link to="/" className="hover:text-teal-500 cursor-pointer">Home</Link>
                    <i className="ri-arrow-right-s-line"></i>
                    <Link to="/marketplace" className="hover:text-teal-500 cursor-pointer">Marketplace</Link>
                    <i className="ri-arrow-right-s-line"></i>
                    <span className="text-gray-900">{product.name}</span>
                </div>

                <div className="grid lg:grid-cols-2 gap-8 mb-8">
                    {/* Image Gallery */}
                    <div className="bg-white rounded-lg shadow-sm p-6">
                        <div className="relative h-[500px] bg-gray-100 rounded-lg overflow-hidden mb-4">
                            <img
                                src={images[selectedImage]}
                                alt={product.name}
                                className="w-full h-full object-cover object-top"
                            />

                            {/* Auction Countdown Overlay */}
                            {product.lotType === 'auction' && timeLeft && (
                                <div className="absolute bottom-0 left-0 right-0">
                                    {timeLeft.expired ? (
                                        <div className="bg-black/60 backdrop-blur-sm px-4 py-3 flex items-center justify-center gap-2">
                                            <i className="ri-auction-line text-gray-300 text-lg"></i>
                                            <span className="text-gray-300 font-semibold text-sm">Auction Ended</span>
                                        </div>
                                    ) : (
                                        <div className={`${isUrgent ? 'bg-red-600/80' : 'bg-black/60'} backdrop-blur-sm px-4 py-3`}>
                                            <div className="flex items-center justify-between mb-2">
                                                <div className="flex items-center gap-1.5">
                                                    <i className={`ri-auction-line text-sm ${isUrgent ? 'text-red-200' : 'text-amber-400'}`}></i>
                                                    <span className={`text-xs font-semibold uppercase tracking-wider ${isUrgent ? 'text-red-200' : 'text-amber-400'}`}>
                                                        {isUrgent ? 'Ending Soon!' : 'Auction Ends In'}
                                                    </span>
                                                </div>
                                                {isUrgent && (
                                                    <span className="text-xs text-red-200 animate-pulse font-medium">⚡ Bid Now</span>
                                                )}
                                            </div>
                                            <div className="flex items-center justify-center gap-3">
                                                {[
                                                    { value: timeLeft.days, label: 'Days' },
                                                    { value: timeLeft.hours, label: 'Hrs' },
                                                    { value: timeLeft.minutes, label: 'Min' },
                                                    { value: timeLeft.seconds, label: 'Sec' },
                                                ].map(({ value, label }, i) => (
                                                    <div key={label} className="flex items-center gap-3">
                                                        <div className="flex flex-col items-center">
                                                            <div className={`w-14 h-12 rounded-lg flex items-center justify-center text-xl font-bold ${isUrgent ? 'bg-red-500/60 text-white' : 'bg-white/15 text-white'}`}>
                                                                {String(value).padStart(2, '0')}
                                                            </div>
                                                            <span className={`text-xs mt-1 font-medium ${isUrgent ? 'text-red-200' : 'text-gray-300'}`}>{label}</span>
                                                        </div>
                                                        {i < 3 && (
                                                            <span className={`text-xl font-bold mb-4 ${isUrgent ? 'text-red-300' : 'text-gray-400'}`}>:</span>
                                                        )}
                                                    </div>
                                                ))}
                                            </div>
                                        </div>
                                    )}
                                </div>
                            )}
                        </div>
                        <div className="grid grid-cols-4 gap-3">
                            {images.map((img, idx) => (
                                <button
                                    key={idx}
                                    onClick={() => setSelectedImage(idx)}
                                    className={`relative h-24 bg-gray-100 rounded-lg overflow-hidden cursor-pointer ${selectedImage === idx ? 'ring-2 ring-teal-500' : ''
                                        }`}
                                >
                                    <img src={img} alt="" className="w-full h-full object-cover object-top" />
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Product Info */}
                    <div className="space-y-6">
                        <div className="bg-white rounded-lg shadow-sm p-6">
                            <h1 className="text-3xl font-bold text-gray-900 mb-4">{product.name}</h1>

                            <div className="flex items-center gap-4 mb-6">
                                {product.lotType === 'auction' ? (
                                    <div className="space-y-1">
                                        <p className="text-sm text-gray-500 font-medium">Current Highest Bid</p>
                                        <span className="text-4xl font-bold text-teal-500">
                                            ${currentHighestBid.toFixed(2)}
                                        </span>
                                    </div>
                                ) : product.lotType === 'draw' ? (
                                    <div className="flex items-end gap-4">
                                        <div className="space-y-1">
                                            <p className="text-sm text-gray-500 font-medium">Ticket Price</p>
                                            <span className="text-4xl font-bold text-teal-500">
                                                ${ticketPrice.toFixed(2)}
                                            </span>
                                        </div>
                                        <div className="space-y-1 pb-1">
                                            <p className="text-sm text-gray-500 font-medium">Lot Value</p>
                                            <span className="text-xl font-semibold text-gray-400 line-through">
                                                ${product.price.toFixed(2)}
                                            </span>
                                        </div>
                                    </div>
                                ) : product.discount > 0 ? (
                                    <>
                                        <span className="text-4xl font-bold text-teal-500">
                                            ${finalPrice.toFixed(2)}
                                        </span>
                                        <span className="text-2xl text-gray-400 line-through">
                                            ${product.price.toFixed(2)}
                                        </span>
                                        <span className="px-3 py-1 bg-red-100 text-red-600 text-sm font-bold rounded-full whitespace-nowrap">
                                            Save {product.discount}%
                                        </span>
                                    </>
                                ) : (
                                    <span className="text-4xl font-bold text-teal-500">
                                        ${product.price.toFixed(2)}
                                    </span>
                                )}
                            </div>

                            {/* Draw ticket stats */}
                            {product.lotType === 'draw' && (
                                <div className="grid grid-cols-3 gap-3 mb-6">
                                    <div className="p-3 bg-teal-50 rounded-lg text-center">
                                        <p className="text-xs text-teal-600 font-medium mb-1">Total Tickets</p>
                                        <p className="text-xl font-bold text-teal-700">{totalTickets}</p>
                                    </div>
                                    <div className="p-3 bg-amber-50 rounded-lg text-center">
                                        <p className="text-xs text-amber-600 font-medium mb-1">Tickets Sold</p>
                                        <p className="text-xl font-bold text-amber-700">{ticketsSold}</p>
                                    </div>
                                    <div className="p-3 bg-green-50 rounded-lg text-center">
                                        <p className="text-xs text-green-600 font-medium mb-1">Tickets Left</p>
                                        <p className="text-xl font-bold text-green-700">{ticketsLeft}</p>
                                    </div>
                                </div>
                            )}

                            {/* Seller Info */}
                            <Link to={`/marketplace/seller/${product.sellerId}`} className="flex items-center gap-3 p-4 bg-gray-50 rounded-lg mb-6 hover:bg-teal-50 transition-colors cursor-pointer group">
                                <div className="w-12 h-12 bg-gray-200 rounded-full overflow-hidden flex-shrink-0">
                                    <img src={product.seller.avatar} alt={product.seller.name} className="w-full h-full object-cover object-top" />
                                </div>
                                <div>
                                    <p className="text-sm text-gray-600">Sold by</p>
                                    <p className="font-semibold text-gray-900 group-hover:text-teal-600 transition-colors">{product.seller.name}</p>
                                </div>
                                <i className="ri-arrow-right-s-line text-gray-400 ml-auto"></i>
                            </Link>

                            {/* Bid / Tickets / Quantity Picker */}
                            <div className="space-y-4">
                                {product.lotType === 'auction' ? (
                                    <div className="space-y-3">
                                        <div className="flex items-center justify-between">
                                            <span className="text-sm font-semibold text-gray-900">Your Bid:</span>
                                            <span className="text-xs text-gray-500">
                                                Min. bid: <span className="font-semibold text-teal-600">${minBid.toFixed(2)}</span>
                                            </span>
                                        </div>
                                        <div className="flex items-center gap-3">
                                            <button
                                                onClick={decrementBid}
                                                className="w-10 h-10 border border-gray-300 rounded-lg flex items-center justify-center hover:bg-gray-100 cursor-pointer flex-shrink-0"
                                            >
                                                <i className="ri-subtract-line"></i>
                                            </button>
                                            <div className="flex-1 relative">
                                                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 font-medium">$</span>
                                                <input
                                                    type="number"
                                                    value={bidAmount}
                                                    min={minBid}
                                                    step={1}
                                                    onChange={(e) => handleBidChange(parseFloat(e.target.value) || minBid)}
                                                    className={`w-full pl-7 pr-3 py-2.5 border rounded-lg text-center text-lg font-semibold focus:outline-none focus:ring-2 focus:ring-teal-500 ${bidError ? 'border-red-400 bg-red-50' : 'border-gray-300'
                                                        }`}
                                                />
                                            </div>
                                            <button
                                                onClick={incrementBid}
                                                className="w-10 h-10 border border-gray-300 rounded-lg flex items-center justify-center hover:bg-gray-100 cursor-pointer flex-shrink-0"
                                            >
                                                <i className="ri-add-line"></i>
                                            </button>
                                        </div>
                                        {bidError && (
                                            <p className="text-xs text-red-500 flex items-center gap-1">
                                                <i className="ri-error-warning-line"></i>
                                                {bidError}
                                            </p>
                                        )}
                                        <div className="flex gap-2 pt-1">
                                            {[minBid, Math.round(minBid * 1.1 * 100) / 100, Math.round(minBid * 1.25 * 100) / 100].map((preset) => (
                                                <button
                                                    key={preset}
                                                    onClick={() => handleBidChange(preset)}
                                                    className={`flex-1 py-1.5 text-xs font-semibold rounded-md border transition-colors cursor-pointer whitespace-nowrap ${bidAmount === preset
                                                            ? 'bg-teal-500 text-white border-teal-500'
                                                            : 'border-gray-300 text-gray-600 hover:border-teal-400 hover:text-teal-600'
                                                        }`}
                                                >
                                                    ${preset.toFixed(2)}
                                                </button>
                                            ))}
                                        </div>
                                    </div>
                                ) : product.lotType === 'draw' ? (
                                    <div className="space-y-3">
                                        <div className="flex items-center justify-between">
                                            <span className="text-sm font-semibold text-gray-900">Tickets:</span>
                                            <span className="text-xs text-gray-500">
                                                {ticketsLeft > 0
                                                    ? <><span className="font-semibold text-green-600">{ticketsLeft}</span> tickets remaining</>
                                                    : <span className="font-semibold text-red-500">Sold out</span>
                                                }
                                            </span>
                                        </div>
                                        <div className="flex items-center gap-3">
                                            <button
                                                onClick={decrementTickets}
                                                disabled={tickets <= 1}
                                                className="w-10 h-10 border border-gray-300 rounded-lg flex items-center justify-center hover:bg-gray-100 cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed flex-shrink-0"
                                            >
                                                <i className="ri-subtract-line"></i>
                                            </button>
                                            <span className="text-lg font-semibold w-12 text-center">{tickets}</span>
                                            <button
                                                onClick={incrementTickets}
                                                disabled={tickets >= ticketsLeft}
                                                className="w-10 h-10 border border-gray-300 rounded-lg flex items-center justify-center hover:bg-gray-100 cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed flex-shrink-0"
                                            >
                                                <i className="ri-add-line"></i>
                                            </button>
                                        </div>
                                        <div className="flex items-center justify-between px-1 text-sm text-gray-600">
                                            <span>Total cost:</span>
                                            <span className="font-bold text-teal-600 text-base">
                                                ${(tickets * ticketPrice).toFixed(2)}
                                            </span>
                                        </div>
                                    </div>
                                ) : (
                                    <div className="flex items-center gap-4">
                                        <span className="text-sm font-semibold text-gray-900 whitespace-nowrap">Quantity:</span>
                                        <div className="flex items-center gap-3">
                                            <button
                                                onClick={() => setQuantity(Math.max(1, quantity - 1))}
                                                className="w-10 h-10 border border-gray-300 rounded-lg flex items-center justify-center hover:bg-gray-100 cursor-pointer"
                                            >
                                                <i className="ri-subtract-line"></i>
                                            </button>
                                            <span className="text-lg font-medium w-12 text-center">{quantity}</span>
                                            <button
                                                onClick={() => setQuantity(quantity + 1)}
                                                className="w-10 h-10 border border-gray-300 rounded-lg flex items-center justify-center hover:bg-gray-100 cursor-pointer"
                                            >
                                                <i className="ri-add-line"></i>
                                            </button>
                                        </div>
                                    </div>
                                )}

                                {product.lotType === 'auction' ? (
                                    <button
                                        onClick={handleBidCheckout}
                                        disabled={!!bidError}
                                        className="w-full py-4 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors text-lg font-semibold whitespace-nowrap cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                                    >
                                        Place Bid — ${bidAmount.toFixed(2)}
                                    </button>
                                ) : product.lotType === 'draw' ? (
                                    <button
                                        onClick={handleDrawCheckout}
                                        disabled={ticketsLeft === 0}
                                        className="w-full py-4 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors text-lg font-semibold whitespace-nowrap cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                                    >
                                        {ticketsLeft === 0 ? 'Sold Out' : `Enter Draw — ${tickets} Ticket${tickets > 1 ? 's' : ''}`}
                                    </button>
                                ) : (
                                    <>
                                        <button className="w-full py-4 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors text-lg font-semibold whitespace-nowrap cursor-pointer">
                                            Add to Cart
                                        </button>
                                        <button className="w-full py-4 border-2 border-teal-500 text-teal-500 rounded-lg hover:bg-teal-50 transition-colors text-lg font-semibold whitespace-nowrap cursor-pointer">
                                            Buy Now
                                        </button>
                                    </>
                                )}
                            </div>
                        </div>

                        {/* Delivery Options */}
                        <div className="bg-white rounded-lg shadow-sm p-6">
                            <h3 className="text-lg font-bold text-gray-900 mb-4">Delivery Options</h3>
                            <div className="space-y-3">
                                <label className="flex items-center gap-3 p-3 border border-gray-200 rounded-lg cursor-pointer hover:border-teal-500">
                                    <input type="radio" name="delivery" defaultChecked className="w-4 h-4 text-teal-500 cursor-pointer" />
                                    <div className="flex-1">
                                        <p className="font-medium text-gray-900">Standard Delivery</p>
                                        <p className="text-sm text-gray-600">3-5 business days - Free</p>
                                    </div>
                                </label>
                                <label className="flex items-center gap-3 p-3 border border-gray-200 rounded-lg cursor-pointer hover:border-teal-500">
                                    <input type="radio" name="delivery" className="w-4 h-4 text-teal-500 cursor-pointer" />
                                    <div className="flex-1">
                                        <p className="font-medium text-gray-900">Express Delivery</p>
                                        <p className="text-sm text-gray-600">1-2 business days - $15.00</p>
                                    </div>
                                </label>
                                <label className="flex items-center gap-3 p-3 border border-gray-200 rounded-lg cursor-pointer hover:border-teal-500">
                                    <input type="radio" name="delivery" className="w-4 h-4 text-teal-500 cursor-pointer" />
                                    <div className="flex-1">
                                        <p className="font-medium text-gray-900">Next Day Delivery</p>
                                        <p className="text-sm text-gray-600">Next business day - $25.00</p>
                                    </div>
                                </label>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Tabs */}
                <div className="bg-white rounded-lg shadow-sm">
                    <div className="border-b border-gray-200">
                        <div className="flex gap-8 px-6">
                            <button
                                onClick={() => setActiveTab('about')}
                                className={`py-4 font-semibold border-b-2 transition-colors whitespace-nowrap cursor-pointer ${activeTab === 'about' ? 'border-teal-500 text-teal-500' : 'border-transparent text-gray-600 hover:text-gray-900'
                                    }`}
                            >
                                About
                            </button>
                            <button
                                onClick={() => setActiveTab('characteristics')}
                                className={`py-4 font-semibold border-b-2 transition-colors whitespace-nowrap cursor-pointer ${activeTab === 'characteristics' ? 'border-teal-500 text-teal-500' : 'border-transparent text-gray-600 hover:text-gray-900'
                                    }`}
                            >
                                Characteristics
                            </button>
                            {goal && !comingSoon && (
                                <button
                                    onClick={() => setActiveTab('goal')}
                                    className={`py-4 font-semibold border-b-2 transition-colors whitespace-nowrap cursor-pointer ${activeTab === 'goal' ? 'border-teal-500 text-teal-500' : 'border-transparent text-gray-600 hover:text-gray-900'
                                        }`}
                                >
                                    <i className="ri-flag-line mr-1"></i>
                                    Goal
                                </button>
                            )}
                        </div>
                    </div>

                    <div className="p-6">
                        {activeTab === 'about' && (
                            <div className="prose max-w-none">
                                <h3 className="text-xl font-bold text-gray-900 mb-4">Product Description</h3>
                                <p className="text-gray-600 leading-relaxed mb-4">
                                    Experience premium quality with this exceptional product designed to meet your needs. Crafted with attention to detail and built to last, this item combines functionality with style.
                                </p>
                                <p className="text-gray-600 leading-relaxed mb-4">
                                    Whether you're a professional or enthusiast, this product delivers outstanding performance and reliability. Its innovative features and user-friendly design make it the perfect choice for anyone seeking excellence.
                                </p>
                                <h4 className="text-lg font-bold text-gray-900 mb-3 mt-6">Key Features:</h4>
                                <ul className="space-y-2 text-gray-600">
                                    <li className="flex items-start gap-2">
                                        <i className="ri-checkbox-circle-fill text-teal-500 mt-1"></i>
                                        <span>Premium quality materials and construction</span>
                                    </li>
                                    <li className="flex items-start gap-2">
                                        <i className="ri-checkbox-circle-fill text-teal-500 mt-1"></i>
                                        <span>Advanced technology for superior performance</span>
                                    </li>
                                    <li className="flex items-start gap-2">
                                        <i className="ri-checkbox-circle-fill text-teal-500 mt-1"></i>
                                        <span>Ergonomic design for maximum comfort</span>
                                    </li>
                                    <li className="flex items-start gap-2">
                                        <i className="ri-checkbox-circle-fill text-teal-500 mt-1"></i>
                                        <span>Easy to use and maintain</span>
                                    </li>
                                    <li className="flex items-start gap-2">
                                        <i className="ri-checkbox-circle-fill text-teal-500 mt-1"></i>
                                        <span>Backed by manufacturer warranty</span>
                                    </li>
                                </ul>
                            </div>
                        )}

                        {activeTab === 'characteristics' && (
                            <div>
                                <h3 className="text-xl font-bold text-gray-900 mb-4">Technical Specifications</h3>
                                <div className="grid md:grid-cols-2 gap-4">
                                    <div className="p-4 bg-gray-50 rounded-lg">
                                        <p className="text-sm text-gray-600 mb-1">Brand</p>
                                        <p className="font-semibold text-gray-900">{product.seller.name}</p>
                                    </div>
                                    <div className="p-4 bg-gray-50 rounded-lg">
                                        <p className="text-sm text-gray-600 mb-1">Condition</p>
                                        <p className="font-semibold text-gray-900">Brand New</p>
                                    </div>
                                    <div className="p-4 bg-gray-50 rounded-lg">
                                        <p className="text-sm text-gray-600 mb-1">Warranty</p>
                                        <p className="font-semibold text-gray-900">1 Year Manufacturer Warranty</p>
                                    </div>
                                    <div className="p-4 bg-gray-50 rounded-lg">
                                        <p className="text-sm text-gray-600 mb-1">Shipping Weight</p>
                                        <p className="font-semibold text-gray-900">2.5 lbs</p>
                                    </div>
                                    <div className="p-4 bg-gray-50 rounded-lg">
                                        <p className="text-sm text-gray-600 mb-1">Dimensions</p>
                                        <p className="font-semibold text-gray-900">10 x 8 x 4 inches</p>
                                    </div>
                                    <div className="p-4 bg-gray-50 rounded-lg">
                                        <p className="text-sm text-gray-600 mb-1">Color Options</p>
                                        <p className="font-semibold text-gray-900">Black, White, Silver</p>
                                    </div>
                                </div>
                            </div>
                        )}

                        {activeTab === 'goal' && goal && (
                            <div>
                                <div className="bg-gradient-to-r from-teal-500/10 to-emerald-500/10 rounded-lg border border-teal-200 p-6 mb-6">
                                    <div className="flex items-start justify-between flex-col sm:flex-row gap-4">
                                        <div className="flex-1">
                                            <div className="flex items-center gap-2 mb-2">
                                                <span className={`px-2.5 py-0.5 text-xs font-semibold rounded-full whitespace-nowrap ${goal.status === 'Reached' ? 'bg-emerald-100 text-emerald-700' : 'bg-teal-100 text-teal-700'
                                                    }`}>
                                                    {goal.status}
                                                </span>
                                            </div>
                                            <h3 className="text-lg font-bold text-gray-900 mb-1">{goal.title}</h3>
                                            <Link
                                                to={`/organization/${goal.organizationId}`}
                                                className="text-sm text-teal-600 hover:text-teal-700 cursor-pointer inline-flex items-center gap-1 hover:underline"
                                            >
                                                {goal.organizationName}
                                                <i className="ri-external-link-line text-xs"></i>
                                            </Link>
                                            <p className="text-sm text-gray-700 leading-relaxed line-clamp-3 mt-2">{goal.explanation}</p>
                                        </div>
                                        <div className="flex-shrink-0 flex flex-col items-center">
                                            <div className="relative w-20 h-20">
                                                <svg className="w-20 h-20 -rotate-90" viewBox="0 0 100 100">
                                                    <circle cx="50" cy="50" r="40" stroke="#e5e7eb" strokeWidth="8" fill="none" />
                                                    <circle
                                                        cx="50" cy="50" r="40"
                                                        stroke="#0d9488"
                                                        strokeWidth="8"
                                                        fill="none"
                                                        strokeLinecap="round"
                                                        strokeDasharray={`${Math.round((goal.moneyRaised / goal.moneyBudget) * 251.3)} 251.3`}
                                                    />
                                                </svg>
                                                <div className="absolute inset-0 flex items-center justify-center">
                                                    <span className="text-sm font-bold text-teal-600">
                                                        {Math.round((goal.moneyRaised / goal.moneyBudget) * 100)}%
                                                    </span>
                                                </div>
                                            </div>
                                            <span className="text-xs text-gray-500 mt-1">Raised</span>
                                        </div>
                                    </div>
                                    <div className="grid grid-cols-3 gap-4 mt-4 pt-4 border-t border-teal-200/50">
                                        <div>
                                            <p className="text-xs text-gray-500 mb-0.5">Budget</p>
                                            <p className="text-sm font-bold text-gray-900">${goal.moneyBudget.toLocaleString()}</p>
                                        </div>
                                        <div>
                                            <p className="text-xs text-gray-500 mb-0.5">Raised</p>
                                            <p className="text-sm font-bold text-teal-600">${goal.moneyRaised.toLocaleString()}</p>
                                        </div>
                                        <div>
                                            <p className="text-xs text-gray-500 mb-0.5">Remaining</p>
                                            <p className="text-sm font-bold text-amber-600">${Math.max(0, goal.moneyBudget - goal.moneyRaised).toLocaleString()}</p>
                                        </div>
                                    </div>
                                </div>
                                <div className="text-center">
                                    <p className="text-sm text-gray-600 mb-3">
                                        This item contributes directly to funding this charitable goal. Purchasing this lot helps make a real difference.
                                    </p>
                                    <Link
                                        to={`/goals/${goal.id}`}
                                        className="inline-flex items-center gap-2 px-6 py-3 bg-teal-500 text-white text-sm font-semibold rounded-lg hover:bg-teal-600 transition-colors cursor-pointer whitespace-nowrap"
                                    >
                                        <i className="ri-flag-line"></i>
                                        View Goal Details &amp; All Serving Items
                                        <i className="ri-arrow-right-line"></i>
                                    </Link>
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}
