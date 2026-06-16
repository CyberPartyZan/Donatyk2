import { useState } from 'react';
import { Link, useSearchParams, useNavigate } from 'react-router-dom';

interface CartItem {
    id: string;
    name: string;
    image: string;
    price: number;
    quantity: number;
}

interface DeliveryAddress {
    fullName: string;
    street: string;
    city: string;
    state: string;
    zipCode: string;
    country: string;
    phone: string;
}

const CARRIERS = [
    { id: 'ups', name: 'UPS', icon: 'ri-truck-line' },
    { id: 'fedex', name: 'FedEx', icon: 'ri-ship-line' },
    { id: 'dhl', name: 'DHL', icon: 'ri-global-line' },
    { id: 'usps', name: 'USPS', icon: 'ri-mail-line' },
    { id: 'royal_mail', name: 'Royal Mail', icon: 'ri-mail-send-line' },
    { id: 'dpd', name: 'DPD', icon: 'ri-truck-line' },
    { id: 'gls', name: 'GLS', icon: 'ri-box-3-line' },
    { id: 'hermes', name: 'Hermes', icon: 'ri-box-2-line' },
    { id: 'tnt', name: 'TNT', icon: 'ri-flight-takeoff-line' },
];

const mockCartItems: CartItem[] = [
    {
        id: 'cart-1',
        name: 'Premium Wireless Headphones with Active Noise Cancellation',
        image: 'https://readdy.ai/api/search-image?query=premium%20wireless%20headphones%20with%20active%20noise%20cancellation%20on%20clean%20white%20background%20product%20photography&width=200&height=200&seq=checkout-hp&orientation=squarish',
        price: 254.99,
        quantity: 1,
    },
    {
        id: 'cart-2',
        name: 'Mechanical Gaming Keyboard RGB Backlit',
        image: 'https://readdy.ai/api/search-image?query=mechanical%20gaming%20keyboard%20rgb%20backlit%20on%20clean%20white%20background%20product%20photography&width=200&height=200&seq=checkout-kb&orientation=squarish',
        price: 134.99,
        quantity: 1,
    },
];

const savedAddresses: DeliveryAddress[] = [
    {
        fullName: 'John Anderson',
        street: '123 Main Street, Apt 4B',
        city: 'New York',
        state: 'NY',
        zipCode: '10001',
        country: 'United States',
        phone: '+1 (555) 234-5678',
    },
    {
        fullName: 'John Anderson',
        street: '456 Office Blvd, Suite 200',
        city: 'Brooklyn',
        state: 'NY',
        zipCode: '11201',
        country: 'United States',
        phone: '+1 (555) 234-5678',
    },
];

export default function CheckoutPage() {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const type = searchParams.get('type') || 'cart';
    const lotId = searchParams.get('lotId') || '';
    const tickets = parseInt(searchParams.get('tickets') || '0', 10);
    const bidAmount = parseFloat(searchParams.get('bidAmount') || '0');
    const ticketPrice = parseFloat(searchParams.get('ticketPrice') || '0');
    const lotName = searchParams.get('lotName') || '';

    const [currentStep, setCurrentStep] = useState(1);
    const [selectedCarrier, setSelectedCarrier] = useState('ups');
    const [selectedAddressIndex, setSelectedAddressIndex] = useState(0);
    const [useNewAddress, setUseNewAddress] = useState(false);
    const [newAddress, setNewAddress] = useState<DeliveryAddress>({
        fullName: '',
        street: '',
        city: '',
        state: '',
        zipCode: '',
        country: 'United States',
        phone: '',
    });
    const [paymentMethod, setPaymentMethod] = useState('stripe');
    const [isProcessing, setIsProcessing] = useState(false);
    const [deliveryPreference, setDeliveryPreference] = useState('standard');

    const DELIVERY_PREFERENCES = [
        { id: 'standard', label: 'Standard Delivery', carrier: 'ups', addressIndex: 0, icon: 'ri-truck-line' },
        { id: 'express', label: 'Express Delivery', carrier: 'fedex', addressIndex: 0, icon: 'ri-flight-takeoff-line' },
        { id: 'nextday', label: 'Next Day Delivery', carrier: 'dhl', addressIndex: 1, icon: 'ri-rocket-line' },
    ];

    const handleDeliveryPreferenceChange = (prefId: string) => {
        setDeliveryPreference(prefId);
        const pref = DELIVERY_PREFERENCES.find(p => p.id === prefId);
        if (pref) {
            setSelectedCarrier(pref.carrier);
            setSelectedAddressIndex(pref.addressIndex);
        }
    };

    const activeAddress = useNewAddress ? newAddress : savedAddresses[selectedAddressIndex];

    const cartSubtotal = mockCartItems.reduce((s, i) => s + i.price * i.quantity, 0);
    const shippingCost = 15.00;

    let itemsTotal: number;
    let totalLabel: string;
    let totalAmount: number;
    let itemsLabel: string;
    let itemDescription: string;

    if (type === 'auction') {
        itemsTotal = bidAmount;
        totalLabel = 'Held Amount';
        totalAmount = bidAmount;
        itemsLabel = 'Auction Bid';
        itemDescription = `Placed bid on "${lotName}"`;
    } else if (type === 'draw') {
        itemsTotal = tickets * ticketPrice;
        totalLabel = 'Total';
        totalAmount = itemsTotal + shippingCost;
        itemsLabel = `Draw Tickets (${tickets})`;
        itemDescription = `${tickets} ticket${tickets > 1 ? 's' : ''} for "${lotName}"`;
    } else {
        itemsTotal = cartSubtotal;
        totalLabel = 'Total';
        totalAmount = itemsTotal + shippingCost;
        itemsLabel = 'Cart Items';
        itemDescription = `${mockCartItems.length} item${mockCartItems.length > 1 ? 's' : ''} in cart`;
    }

    const handlePlaceOrder = () => {
        if (!activeAddress.fullName || !activeAddress.street || !activeAddress.city) {
            alert('Please fill in all delivery address fields.');
            return;
        }
        setIsProcessing(true);
        const orderId = Math.floor(Math.random() * 900000 + 100000).toString();

        setTimeout(() => {
            setIsProcessing(false);
            navigate(
                `/payment/success?orderId=${orderId}&type=${type}&amount=${totalAmount}${type === 'draw' ? `&tickets=${tickets}` : ''}${type === 'auction' ? `&bidAmount=${bidAmount}` : ''}`
            );
        }, 1500);
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <header className="bg-white shadow-sm sticky top-0 z-40">
                <div className="max-w-7xl mx-auto px-6 py-4 flex items-center justify-between">
                    <Link to="/">
                        <img
                            src="https://public.readdy.ai/ai/img_res/fd4376ec-da1c-49df-8529-c6f422339bdf.png"
                            alt="Logo"
                            className="h-10 w-auto"
                        />
                    </Link>
                    <div className="flex items-center gap-2 text-sm text-gray-500">
                        <Link to="/marketplace" className="hover:text-teal-600 cursor-pointer">Marketplace</Link>
                        <i className="ri-arrow-right-s-line"></i>
                        <span className="text-gray-900 font-medium">Checkout</span>
                    </div>
                </div>
            </header>

            <div className="max-w-5xl mx-auto px-6 py-8">
                <h1 className="text-2xl font-bold text-gray-900 mb-8">Checkout</h1>

                {/* Step Indicator */}
                <div className="flex items-center justify-center gap-4 mb-10">
                    {[1, 2, 3].map((step) => (
                        <div key={step} className="flex items-center gap-2">
                            <div
                                className={`w-10 h-10 rounded-full flex items-center justify-center text-sm font-bold ${currentStep >= step
                                        ? 'bg-teal-600 text-white'
                                        : 'bg-gray-200 text-gray-500'
                                    }`}
                            >
                                {currentStep > step ? <i className="ri-check-line"></i> : step}
                            </div>
                            <span className={`text-sm font-medium ${currentStep >= step ? 'text-gray-900' : 'text-gray-400'}`}>
                                {step === 1 ? 'Items' : step === 2 ? 'Delivery' : 'Payment'}
                            </span>
                            {step < 3 && <div className={`w-12 h-0.5 ${currentStep > step ? 'bg-teal-600' : 'bg-gray-200'}`}></div>}
                        </div>
                    ))}
                </div>

                <div className="grid lg:grid-cols-3 gap-8">
                    {/* Main Content */}
                    <div className="lg:col-span-2 space-y-6">
                        {/* Section 1: Items */}
                        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                            <div className="flex items-center justify-between mb-5">
                                <h2 className="text-lg font-bold text-gray-900">
                                    <i className="ri-shopping-bag-line mr-2 text-teal-600"></i>
                                    Purchased Items
                                </h2>
                                <button
                                    onClick={() => setCurrentStep(1)}
                                    className="text-sm text-teal-600 hover:text-teal-700 font-medium cursor-pointer whitespace-nowrap"
                                >
                                    <i className="ri-edit-line mr-1"></i> Edit
                                </button>
                            </div>

                            {type === 'auction' ? (
                                <div className="bg-amber-50 rounded-lg p-5">
                                    <div className="flex items-center justify-between mb-2">
                                        <span className="px-2 py-1 bg-amber-200 text-amber-800 text-xs font-medium rounded-full whitespace-nowrap">Auction</span>
                                        <span className="text-xs text-amber-600">#{lotId}</span>
                                    </div>
                                    <h3 className="font-semibold text-gray-900 mb-1">{lotName}</h3>
                                    <div className="flex items-baseline gap-2 mt-3">
                                        <span className="text-sm text-gray-600">Your Bid:</span>
                                        <span className="text-2xl font-bold text-teal-600">${bidAmount.toFixed(2)}</span>
                                    </div>
                                    <p className="text-xs text-amber-700 mt-3 bg-amber-100 rounded-md p-2">
                                        <i className="ri-information-line mr-1"></i>
                                        This amount will be held and applied if your bid wins. Released if outbid.
                                    </p>
                                </div>
                            ) : type === 'draw' ? (
                                <div className="bg-purple-50 rounded-lg p-5">
                                    <div className="flex items-center justify-between mb-2">
                                        <span className="px-2 py-1 bg-purple-200 text-purple-800 text-xs font-medium rounded-full whitespace-nowrap">Draw</span>
                                        <span className="text-xs text-purple-600">#{lotId}</span>
                                    </div>
                                    <h3 className="font-semibold text-gray-900 mb-1">{lotName}</h3>
                                    <div className="grid grid-cols-3 gap-3 mt-4">
                                        <div className="bg-white rounded-md p-3 text-center">
                                            <p className="text-xs text-gray-500">Tickets</p>
                                            <p className="text-xl font-bold text-gray-900">{tickets}</p>
                                        </div>
                                        <div className="bg-white rounded-md p-3 text-center">
                                            <p className="text-xs text-gray-500">Price/Ticket</p>
                                            <p className="text-xl font-bold text-gray-900">${ticketPrice.toFixed(2)}</p>
                                        </div>
                                        <div className="bg-white rounded-md p-3 text-center">
                                            <p className="text-xs text-gray-500">Subtotal</p>
                                            <p className="text-xl font-bold text-teal-600">${itemsTotal.toFixed(2)}</p>
                                        </div>
                                    </div>
                                </div>
                            ) : (
                                <div className="space-y-4">
                                    {mockCartItems.map((item) => (
                                        <div key={item.id} className="flex items-center gap-4 p-3 bg-gray-50 rounded-lg">
                                            <div className="w-16 h-16 bg-gray-200 rounded-lg overflow-hidden flex-shrink-0">
                                                <img src={item.image} alt={item.name} className="w-full h-full object-cover object-top" />
                                            </div>
                                            <div className="flex-1 min-w-0">
                                                <h3 className="text-sm font-semibold text-gray-900 truncate">{item.name}</h3>
                                                <p className="text-xs text-gray-500">Qty: {item.quantity}</p>
                                            </div>
                                            <div className="text-right">
                                                <p className="text-sm font-bold text-gray-900">${(item.price * item.quantity).toFixed(2)}</p>
                                                <p className="text-xs text-gray-500">${item.price.toFixed(2)} each</p>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                        {/* Section 2: Carrier */}
                        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                            <div className="flex items-center justify-between mb-5">
                                <h2 className="text-lg font-bold text-gray-900">
                                    <i className="ri-truck-line mr-2 text-teal-600"></i>
                                    Carrier
                                </h2>
                            </div>

                            <div className="grid grid-cols-3 sm:grid-cols-5 gap-2">
                                {CARRIERS.map((carrier) => (
                                    <button
                                        key={carrier.id}
                                        onClick={() => setSelectedCarrier(carrier.id)}
                                        className={`flex flex-col items-center gap-1 p-3 rounded-lg border-2 transition-colors cursor-pointer ${selectedCarrier === carrier.id
                                                ? 'border-teal-500 bg-teal-50'
                                                : 'border-gray-200 hover:border-gray-300'
                                            }`}
                                    >
                                        <i className={`${carrier.icon} text-lg ${selectedCarrier === carrier.id ? 'text-teal-600' : 'text-gray-500'}`}></i>
                                        <span className={`text-xs font-medium whitespace-nowrap ${selectedCarrier === carrier.id ? 'text-teal-700' : 'text-gray-600'}`}>
                                            {carrier.name}
                                        </span>
                                    </button>
                                ))}
                            </div>
                        </div>

                        {/* Section 2b: Address */}
                        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                            <div className="flex items-center justify-between mb-5">
                                <h2 className="text-lg font-bold text-gray-900">
                                    <i className="ri-map-pin-line mr-2 text-teal-600"></i>
                                    Address
                                </h2>
                            </div>

                            {/* Saved Addresses */}
                            {!useNewAddress && (
                                <div className="mb-5">
                                    <label className="block text-sm font-semibold text-gray-700 mb-3">Saved Addresses</label>
                                    <div className="space-y-3">
                                        {savedAddresses.map((addr, idx) => (
                                            <label
                                                key={idx}
                                                className={`flex items-start gap-3 p-4 border-2 rounded-lg cursor-pointer transition-colors ${selectedAddressIndex === idx
                                                        ? 'border-teal-500 bg-teal-50'
                                                        : 'border-gray-200 hover:border-gray-300'
                                                    }`}
                                            >
                                                <input
                                                    type="radio"
                                                    name="savedAddress"
                                                    checked={selectedAddressIndex === idx}
                                                    onChange={() => setSelectedAddressIndex(idx)}
                                                    className="mt-0.5 w-4 h-4 text-teal-600 cursor-pointer"
                                                />
                                                <div>
                                                    <p className="text-sm font-semibold text-gray-900">{addr.fullName}</p>
                                                    <p className="text-xs text-gray-600">{addr.street}</p>
                                                    <p className="text-xs text-gray-600">{addr.city}, {addr.state} {addr.zipCode}</p>
                                                    <p className="text-xs text-gray-500">{addr.phone}</p>
                                                </div>
                                            </label>
                                        ))}
                                    </div>
                                    <button
                                        onClick={() => setUseNewAddress(true)}
                                        className="mt-3 text-sm text-teal-600 hover:text-teal-700 font-medium cursor-pointer whitespace-nowrap"
                                    >
                                        <i className="ri-add-line mr-1"></i> Use a new address
                                    </button>
                                </div>
                            )}

                            {/* New Address Form */}
                            {useNewAddress && (
                                <div className="space-y-4">
                                    <button
                                        onClick={() => setUseNewAddress(false)}
                                        className="text-sm text-teal-600 hover:text-teal-700 font-medium cursor-pointer whitespace-nowrap mb-2"
                                    >
                                        <i className="ri-arrow-left-line mr-1"></i> Back to saved addresses
                                    </button>
                                    <div className="grid grid-cols-2 gap-4">
                                        <div>
                                            <label className="block text-xs font-medium text-gray-700 mb-1">Full Name</label>
                                            <input
                                                type="text"
                                                value={newAddress.fullName}
                                                onChange={(e) => setNewAddress({ ...newAddress, fullName: e.target.value })}
                                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                                placeholder="John Doe"
                                            />
                                        </div>
                                        <div>
                                            <label className="block text-xs font-medium text-gray-700 mb-1">Phone</label>
                                            <input
                                                type="text"
                                                value={newAddress.phone}
                                                onChange={(e) => setNewAddress({ ...newAddress, phone: e.target.value })}
                                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                                placeholder="+1 (555) 000-0000"
                                            />
                                        </div>
                                        <div className="col-span-2">
                                            <label className="block text-xs font-medium text-gray-700 mb-1">Street Address</label>
                                            <input
                                                type="text"
                                                value={newAddress.street}
                                                onChange={(e) => setNewAddress({ ...newAddress, street: e.target.value })}
                                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                                placeholder="123 Main Street, Apt 4B"
                                            />
                                        </div>
                                        <div>
                                            <label className="block text-xs font-medium text-gray-700 mb-1">City</label>
                                            <input
                                                type="text"
                                                value={newAddress.city}
                                                onChange={(e) => setNewAddress({ ...newAddress, city: e.target.value })}
                                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                                placeholder="New York"
                                            />
                                        </div>
                                        <div>
                                            <label className="block text-xs font-medium text-gray-700 mb-1">State</label>
                                            <input
                                                type="text"
                                                value={newAddress.state}
                                                onChange={(e) => setNewAddress({ ...newAddress, state: e.target.value })}
                                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                                placeholder="NY"
                                            />
                                        </div>
                                        <div>
                                            <label className="block text-xs font-medium text-gray-700 mb-1">ZIP Code</label>
                                            <input
                                                type="text"
                                                value={newAddress.zipCode}
                                                onChange={(e) => setNewAddress({ ...newAddress, zipCode: e.target.value })}
                                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                                placeholder="10001"
                                            />
                                        </div>
                                        <div>
                                            <label className="block text-xs font-medium text-gray-700 mb-1">Country</label>
                                            <input
                                                type="text"
                                                value={newAddress.country}
                                                onChange={(e) => setNewAddress({ ...newAddress, country: e.target.value })}
                                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                                placeholder="United States"
                                            />
                                        </div>
                                    </div>
                                </div>
                            )}
                        </div>

                        {/* Section 2c: Delivery Preferences */}
                        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                            <div className="flex items-center justify-between mb-5">
                                <h2 className="text-lg font-bold text-gray-900">
                                    <i className="ri-settings-3-line mr-2 text-teal-600"></i>
                                    Delivery Preferences
                                </h2>
                            </div>

                            <div className="space-y-3 mb-5">
                                {DELIVERY_PREFERENCES.map((pref) => (
                                    <label
                                        key={pref.id}
                                        className={`flex items-center gap-4 p-4 border-2 rounded-lg cursor-pointer transition-colors ${deliveryPreference === pref.id
                                                ? 'border-teal-500 bg-teal-50'
                                                : 'border-gray-200 hover:border-gray-300'
                                            }`}
                                    >
                                        <input
                                            type="radio"
                                            name="deliveryPreference"
                                            checked={deliveryPreference === pref.id}
                                            onChange={() => handleDeliveryPreferenceChange(pref.id)}
                                            className="w-4 h-4 text-teal-600 cursor-pointer flex-shrink-0"
                                        />
                                        <div className="w-10 h-10 bg-teal-100 rounded-lg flex items-center justify-center flex-shrink-0">
                                            <i className={`${pref.icon} text-lg text-teal-600`}></i>
                                        </div>
                                        <div className="flex-1">
                                            <p className="text-sm font-semibold text-gray-900">{pref.label}</p>
                                            <p className="text-xs text-gray-500">
                                                via {CARRIERS.find(c => c.id === pref.carrier)?.name}
                                                {pref.id === 'nextday' ? ' — Fastest option' : pref.id === 'express' ? ' — 2-3 business days' : ' — 5-7 business days'}
                                            </p>
                                        </div>
                                        {deliveryPreference === pref.id && (
                                            <div className="w-6 h-6 rounded-full bg-teal-600 flex items-center justify-center flex-shrink-0">
                                                <i className="ri-check-line text-white text-xs"></i>
                                            </div>
                                        )}
                                    </label>
                                ))}
                            </div>

                            <div className="bg-gradient-to-br from-teal-50 to-emerald-50 rounded-xl p-5 border border-teal-100">
                                <div className="flex items-center gap-3 mb-4 pb-4 border-b border-teal-200/60">
                                    <div className="w-10 h-10 bg-teal-100 rounded-lg flex items-center justify-center flex-shrink-0">
                                        <i className={`${CARRIERS.find(c => c.id === selectedCarrier)?.icon || 'ri-truck-line'} text-lg text-teal-600`}></i>
                                    </div>
                                    <div>
                                        <p className="text-xs text-teal-600 font-medium">Auto-Selected Carrier</p>
                                        <p className="text-sm font-bold text-gray-900">
                                            {CARRIERS.find(c => c.id === selectedCarrier)?.name || '-'}
                                        </p>
                                    </div>
                                </div>

                                <div className="flex items-start gap-3">
                                    <div className="w-10 h-10 bg-emerald-100 rounded-lg flex items-center justify-center flex-shrink-0">
                                        <i className="ri-map-pin-line text-lg text-emerald-600"></i>
                                    </div>
                                    <div>
                                        <p className="text-xs text-emerald-600 font-medium">Auto-Selected Address</p>
                                        <p className="text-sm font-bold text-gray-900">{activeAddress.fullName || '-'}</p>
                                        <p className="text-xs text-gray-600">{activeAddress.street || '-'}</p>
                                        <p className="text-xs text-gray-600">
                                            {activeAddress.city}{activeAddress.city && ', '}{activeAddress.state} {activeAddress.zipCode}
                                        </p>
                                        <p className="text-xs text-gray-500 mt-0.5">{activeAddress.phone || '-'}</p>
                                    </div>
                                </div>

                                <div className="mt-4 pt-3 border-t border-teal-200/60">
                                    <p className="text-xs text-gray-500">
                                        <i className="ri-information-line mr-1 text-teal-500"></i>
                                        Selecting a delivery preference automatically updates your carrier and delivery address.
                                    </p>
                                </div>
                            </div>
                        </div>

                        {/* Section 3: Payment */}
                        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                            <div className="flex items-center justify-between mb-5">
                                <h2 className="text-lg font-bold text-gray-900">
                                    <i className="ri-bank-card-line mr-2 text-teal-600"></i>
                                    Payment Method
                                </h2>
                                <button
                                    onClick={() => setCurrentStep(3)}
                                    className="text-sm text-teal-600 hover:text-teal-700 font-medium cursor-pointer whitespace-nowrap"
                                >
                                    <i className="ri-edit-line mr-1"></i> Edit
                                </button>
                            </div>

                            <div className="space-y-3">
                                <label
                                    className={`flex items-center gap-4 p-4 border-2 rounded-lg cursor-pointer transition-colors ${paymentMethod === 'stripe'
                                            ? 'border-teal-500 bg-teal-50'
                                            : 'border-gray-200 hover:border-gray-300'
                                        }`}
                                >
                                    <input
                                        type="radio"
                                        name="payment"
                                        checked={paymentMethod === 'stripe'}
                                        onChange={() => setPaymentMethod('stripe')}
                                        className="w-4 h-4 text-teal-600 cursor-pointer"
                                    />
                                    <div className="flex-1">
                                        <div className="flex items-center gap-2">
                                            <span className="font-semibold text-gray-900">Stripe</span>
                                            <span className="px-2 py-0.5 bg-teal-100 text-teal-700 text-xs font-medium rounded-full whitespace-nowrap">Recommended</span>
                                        </div>
                                        <p className="text-xs text-gray-500 mt-0.5">Secure payment via credit/debit card</p>
                                    </div>
                                    <div className="flex gap-1">
                                        <i className="ri-visa-line text-2xl text-blue-600"></i>
                                        <i className="ri-mastercard-line text-2xl text-red-500"></i>
                                    </div>
                                </label>

                                <div className="bg-gray-100 rounded-lg p-4 text-center">
                                    <p className="text-sm text-gray-500">
                                        <i className="ri-information-line mr-1"></i>
                                        More payment methods coming soon
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Order Summary Sidebar */}
                    <div className="lg:col-span-1">
                        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 sticky top-24">
                            <h3 className="text-lg font-bold text-gray-900 mb-5">Order Summary</h3>

                            <div className="space-y-3 mb-5">
                                <div className="flex justify-between text-sm">
                                    <span className="text-gray-600">{itemsLabel}</span>
                                    <span className="font-semibold text-gray-900">${itemsTotal.toFixed(2)}</span>
                                </div>

                                {type !== 'auction' && (
                                    <div className="flex justify-between text-sm">
                                        <span className="text-gray-600">Shipping ({CARRIERS.find(c => c.id === selectedCarrier)?.name})</span>
                                        <span className="font-semibold text-gray-900">${shippingCost.toFixed(2)}</span>
                                    </div>
                                )}

                                <div className="pt-3 border-t border-gray-200">
                                    <div className="flex justify-between">
                                        <span className="text-base font-bold text-gray-900">{totalLabel}</span>
                                        <span className="text-xl font-bold text-teal-600">${totalAmount.toFixed(2)}</span>
                                    </div>
                                    {type === 'auction' && (
                                        <p className="text-xs text-amber-600 mt-1">Amount held, not charged immediately</p>
                                    )}
                                </div>
                            </div>

                            {/* Delivery Summary */}
                            <div className="bg-gray-50 rounded-lg p-4 mb-5">
                                <p className="text-xs font-semibold text-gray-700 mb-2">
                                    <i className="ri-map-pin-line mr-1"></i> Delivery
                                </p>
                                <p className="text-xs text-gray-600">{activeAddress.fullName || '—'}</p>
                                <p className="text-xs text-gray-600">{activeAddress.street || '—'}</p>
                                <p className="text-xs text-gray-600">
                                    {activeAddress.city}{activeAddress.city && ', '}{activeAddress.state} {activeAddress.zipCode}
                                </p>
                                <p className="text-xs text-gray-500 mt-1">
                                    <i className="ri-truck-line mr-1"></i>
                                    {CARRIERS.find(c => c.id === selectedCarrier)?.name}
                                </p>
                            </div>

                            <button
                                onClick={handlePlaceOrder}
                                disabled={isProcessing}
                                className="w-full py-3.5 bg-teal-600 text-white text-base font-semibold rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap disabled:opacity-60 disabled:cursor-not-allowed"
                            >
                                {isProcessing ? (
                                    <span className="flex items-center justify-center gap-2">
                                        <i className="ri-loader-4-line animate-spin"></i>
                                        Processing...
                                    </span>
                                ) : type === 'auction' ? (
                                    'Confirm Bid'
                                ) : (
                                    'Place Order'
                                )}
                            </button>

                            <p className="text-xs text-gray-400 text-center mt-3">
                                <i className="ri-lock-line mr-1"></i>
                                Secured by Stripe
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}