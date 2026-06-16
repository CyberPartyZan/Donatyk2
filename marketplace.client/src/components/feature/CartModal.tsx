import { useState } from 'react';
import { Link } from 'react-router-dom';

interface CartItem {
    id: string;
    productId: string;
    name: string;
    image: string;
    price: number;
    quantity: number;
    seller: {
        name: string;
        avatar: string;
    };
}

interface CartModalProps {
    isOpen: boolean;
    onClose: () => void;
}

export default function CartModal({ isOpen, onClose }: CartModalProps) {
    const [cartItems, setCartItems] = useState<CartItem[]>([
        {
            id: '1',
            productId: 'prod-1',
            name: 'Premium Wireless Headphones with Active Noise Cancellation',
            image: 'https://readdy.ai/api/search-image?query=premium%20wireless%20headphones%20with%20active%20noise%20cancellation%20on%20simple%20white%20background%20product%20photography%20studio%20lighting%20high%20quality%20professional&width=200&height=200&seq=cart1&orientation=squarish',
            price: 299.99,
            quantity: 1,
            seller: {
                name: 'TechGear Pro',
                avatar: 'https://readdy.ai/api/search-image?query=professional%20technology%20store%20logo%20modern%20minimalist%20design&width=100&height=100&seq=seller1&orientation=squarish'
            }
        },
        {
            id: '2',
            productId: 'prod-2',
            name: 'Smart Fitness Watch with Heart Rate Monitor',
            image: 'https://readdy.ai/api/search-image?query=smart%20fitness%20watch%20with%20heart%20rate%20monitor%20on%20simple%20white%20background%20product%20photography%20studio%20lighting%20high%20quality%20professional&width=200&height=200&seq=cart2&orientation=squarish',
            price: 199.99,
            quantity: 2,
            seller: {
                name: 'FitLife Store',
                avatar: 'https://readdy.ai/api/search-image?query=fitness%20lifestyle%20store%20logo%20modern%20minimalist%20design&width=100&height=100&seq=seller2&orientation=squarish'
            }
        }
    ]);

    const [openMenuId, setOpenMenuId] = useState<string | null>(null);

    if (!isOpen) return null;

    const updateQuantity = (id: string, newQuantity: number) => {
        if (newQuantity < 1) return;
        setCartItems(cartItems.map(item =>
            item.id === id ? { ...item, quantity: newQuantity } : item
        ));
    };

    const removeItem = (id: string) => {
        setCartItems(cartItems.filter(item => item.id !== id));
        setOpenMenuId(null);
    };

    const addToWishlist = (id: string) => {
        alert('Added to wishlist!');
        setOpenMenuId(null);
    };

    const subtotal = cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const shipping = 15.00;
    const total = subtotal + shipping;

    return (
        <div className="fixed inset-0 z-50 flex items-start justify-end">
            <div
                className="absolute inset-0 bg-black/50"
                onClick={onClose}
            ></div>

            <div className="relative w-full max-w-md h-full bg-white shadow-2xl flex flex-col animate-slide-in-right">
                {/* Header */}
                <div className="flex items-center justify-between p-6 border-b border-gray-200">
                    <h2 className="text-2xl font-bold text-gray-900">Shopping Cart</h2>
                    <button
                        onClick={onClose}
                        className="w-10 h-10 flex items-center justify-center hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
                    >
                        <i className="ri-close-line text-2xl text-gray-700"></i>
                    </button>
                </div>

                {/* Cart Items */}
                <div className="flex-1 overflow-y-auto p-6">
                    {cartItems.length === 0 ? (
                        <div className="flex flex-col items-center justify-center h-full text-center">
                            <i className="ri-shopping-cart-line text-6xl text-gray-300 mb-4"></i>
                            <p className="text-gray-500 text-lg mb-2">Your cart is empty</p>
                            <p className="text-gray-400 text-sm mb-6">Add items to get started</p>
                            <button
                                onClick={onClose}
                                className="px-6 py-3 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors font-semibold whitespace-nowrap cursor-pointer"
                            >
                                Continue Shopping
                            </button>
                        </div>
                    ) : (
                        <div className="space-y-4">
                            {cartItems.map((item) => (
                                <div key={item.id} className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow">
                                    <div className="flex gap-4">
                                        <Link
                                            to={`/marketplace/item/${item.productId}`}
                                            onClick={onClose}
                                            className="w-20 h-20 bg-gray-100 rounded-lg overflow-hidden flex-shrink-0 cursor-pointer"
                                        >
                                            <img
                                                src={item.image}
                                                alt={item.name}
                                                className="w-full h-full object-cover object-top"
                                            />
                                        </Link>

                                        <div className="flex-1 min-w-0">
                                            <Link
                                                to={`/marketplace/item/${item.productId}`}
                                                onClick={onClose}
                                                className="font-semibold text-gray-900 line-clamp-2 mb-2 hover:text-teal-500 cursor-pointer"
                                            >
                                                {item.name}
                                            </Link>

                                            <div className="flex items-center gap-2 mb-3">
                                                <div className="w-6 h-6 bg-gray-200 rounded-full overflow-hidden flex-shrink-0">
                                                    <img
                                                        src={item.seller.avatar}
                                                        alt={item.seller.name}
                                                        className="w-full h-full object-cover object-top"
                                                    />
                                                </div>
                                                <span className="text-xs text-gray-600">{item.seller.name}</span>
                                            </div>

                                            <div className="flex items-center justify-between">
                                                <div className="flex items-center gap-2">
                                                    <button
                                                        onClick={() => updateQuantity(item.id, item.quantity - 1)}
                                                        className="w-7 h-7 border border-gray-300 rounded flex items-center justify-center hover:bg-gray-100 cursor-pointer"
                                                    >
                                                        <i className="ri-subtract-line text-sm"></i>
                                                    </button>
                                                    <span className="text-sm font-medium w-8 text-center">{item.quantity}</span>
                                                    <button
                                                        onClick={() => updateQuantity(item.id, item.quantity + 1)}
                                                        className="w-7 h-7 border border-gray-300 rounded flex items-center justify-center hover:bg-gray-100 cursor-pointer"
                                                    >
                                                        <i className="ri-add-line text-sm"></i>
                                                    </button>
                                                </div>

                                                <div className="flex items-center gap-3">
                                                    <span className="text-lg font-bold text-teal-500">
                                                        ${(item.price * item.quantity).toFixed(2)}
                                                    </span>

                                                    <div className="relative">
                                                        <button
                                                            onClick={() => setOpenMenuId(openMenuId === item.id ? null : item.id)}
                                                            className="w-8 h-8 flex items-center justify-center hover:bg-gray-100 rounded cursor-pointer"
                                                        >
                                                            <i className="ri-more-2-fill text-gray-600"></i>
                                                        </button>

                                                        {openMenuId === item.id && (
                                                            <div className="absolute right-0 top-full mt-1 w-48 bg-white border border-gray-200 rounded-lg shadow-lg z-10">
                                                                <button
                                                                    onClick={() => addToWishlist(item.id)}
                                                                    className="w-full px-4 py-3 text-left text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-3 cursor-pointer"
                                                                >
                                                                    <i className="ri-heart-line"></i>
                                                                    Add to Wishlist
                                                                </button>
                                                                <button
                                                                    onClick={() => removeItem(item.id)}
                                                                    className="w-full px-4 py-3 text-left text-sm text-red-600 hover:bg-red-50 flex items-center gap-3 cursor-pointer"
                                                                >
                                                                    <i className="ri-delete-bin-line"></i>
                                                                    Remove
                                                                </button>
                                                            </div>
                                                        )}
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                {/* Footer */}
                {cartItems.length > 0 && (
                    <div className="border-t border-gray-200 p-6 space-y-4">
                        <div className="space-y-2">
                            <div className="flex items-center justify-between text-sm">
                                <span className="text-gray-600">Subtotal</span>
                                <span className="font-semibold text-gray-900">${subtotal.toFixed(2)}</span>
                            </div>
                            <div className="flex items-center justify-between text-sm">
                                <span className="text-gray-600">Shipping</span>
                                <span className="font-semibold text-gray-900">${shipping.toFixed(2)}</span>
                            </div>
                            <div className="pt-2 border-t border-gray-200">
                                <div className="flex items-center justify-between">
                                    <span className="text-lg font-bold text-gray-900">Total</span>
                                    <span className="text-2xl font-bold text-teal-500">${total.toFixed(2)}</span>
                                </div>
                            </div>
                        </div>

                        <Link
                            to="/checkout?type=cart"
                            className="w-full py-4 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors text-lg font-semibold whitespace-nowrap cursor-pointer text-center block"
                            onClick={onClose}
                        >
                            Place Order
                        </Link>

                        <button
                            onClick={onClose}
                            className="w-full py-3 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors font-semibold whitespace-nowrap cursor-pointer"
                        >
                            Continue Shopping
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
}
