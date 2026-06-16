import { Link } from 'react-router-dom';
import { useState } from 'react';

export default function OrdersHeader() {
    const [showNotifications, setShowNotifications] = useState(false);

    return (
        <header className="bg-white shadow-sm sticky top-0 z-50">
            <div className="max-w-[1600px] mx-auto px-6">
                <div className="flex items-center justify-between h-16">
                    <Link to="/" className="flex items-center gap-2 cursor-pointer">
                        <div className="w-10 h-10 bg-teal-500 rounded-lg flex items-center justify-center">
                            <i className="ri-store-2-fill text-white text-xl"></i>
                        </div>
                        <span className="text-xl font-bold text-gray-900">Marketplace</span>
                    </Link>

                    <div className="flex items-center gap-4">
                        <Link
                            to="/marketplace"
                            className="px-4 py-2 text-gray-700 hover:text-teal-500 transition-colors text-sm font-medium whitespace-nowrap cursor-pointer"
                        >
                            Continue Shopping
                        </Link>

                        <Link
                            to="/orders"
                            className="relative p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
                        >
                            <i className="ri-file-list-3-line text-xl text-teal-500"></i>
                            <span className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs rounded-full flex items-center justify-center">3</span>
                        </Link>

                        <button className="relative p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer">
                            <i className="ri-shopping-cart-line text-xl text-gray-700"></i>
                            <span className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs rounded-full flex items-center justify-center">2</span>
                        </button>

                        <div className="relative">
                            <button
                                onClick={() => setShowNotifications(!showNotifications)}
                                className="relative p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
                            >
                                <i className="ri-notification-3-line text-xl text-gray-700"></i>
                                <span className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs rounded-full flex items-center justify-center">5</span>
                            </button>

                            {showNotifications && (
                                <>
                                    <div
                                        className="fixed inset-0 z-40"
                                        onClick={() => setShowNotifications(false)}
                                    ></div>
                                    <div className="absolute right-0 top-full mt-2 w-80 bg-white rounded-lg shadow-lg border border-gray-200 z-50">
                                        <div className="p-4 border-b border-gray-200">
                                            <h3 className="font-semibold text-gray-900">Notifications</h3>
                                        </div>
                                        <div className="max-h-96 overflow-y-auto">
                                            {[1, 2, 3, 4, 5].map((i) => (
                                                <div key={i} className="p-4 border-b border-gray-100 hover:bg-gray-50 cursor-pointer">
                                                    <p className="text-sm text-gray-900 mb-1">Order #{1000 + i} has been shipped</p>
                                                    <p className="text-xs text-gray-500">{i} hours ago</p>
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                </>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </header>
    );
}
