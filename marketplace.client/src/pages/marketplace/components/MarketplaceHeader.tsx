import { Link } from 'react-router-dom';
import { useState } from 'react';
import CartModal from '../../../components/feature/CartModal';
import { useAuth } from '@/hooks/useAuth';

interface MarketplaceHeaderProps {
    onCategoriesClick: () => void;
}

export default function MarketplaceHeader({ onCategoriesClick }: MarketplaceHeaderProps) {
    const [cartOpen, setCartOpen] = useState(false);
    const { isLoggedIn } = useAuth();

    return (
        <>
            <header className="bg-white shadow-sm sticky top-0 z-40">
                <div className="max-w-[1600px] mx-auto px-6 py-4">
                    <div className="flex items-center justify-between gap-8">
                        <div className="flex items-center gap-6">
                            <Link to="/">
                                <img
                                    src="https://public.readdy.ai/ai/img_res/fd4376ec-da1c-49df-8529-c6f422339bdf.png"
                                    alt="Logo"
                                    className="h-10 w-auto"
                                />
                            </Link>
                            <button
                                onClick={onCategoriesClick}
                                className="px-5 py-2.5 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors flex items-center gap-2 whitespace-nowrap cursor-pointer"
                            >
                                <i className="ri-menu-line text-lg"></i>
                                Categories
                            </button>
                        </div>

                        <div className="flex-1 max-w-2xl">
                            <div className="relative">
                                <input
                                    type="text"
                                    placeholder="Search for products..."
                                    className="w-full px-5 py-2.5 pr-12 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-500 text-sm"
                                />
                                <button className="absolute right-2 top-1/2 -translate-y-1/2 w-8 h-8 bg-teal-500 text-white rounded-lg flex items-center justify-center hover:bg-teal-600 transition-colors cursor-pointer">
                                    <i className="ri-search-line text-lg"></i>
                                </button>
                            </div>
                        </div>

                        <div className="flex items-center gap-3">
                            <button
                                onClick={() => setCartOpen(true)}
                                className="relative p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
                            >
                                <i className="ri-shopping-cart-line text-xl text-gray-700"></i>
                                <span className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs rounded-full flex items-center justify-center">2</span>
                            </button>

                            {isLoggedIn ? (
                                <>
                                    <Link
                                        to="/orders"
                                        className="relative p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
                                    >
                                        <i className="ri-file-list-3-line text-xl text-gray-700"></i>
                                        <span className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs rounded-full flex items-center justify-center">3</span>
                                    </Link>
                                    <button className="relative p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer">
                                        <i className="ri-notification-3-line text-xl text-gray-700"></i>
                                        <span className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs rounded-full flex items-center justify-center">5</span>
                                    </button>
                                    <Link
                                        to="/admin"
                                        className="relative p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
                                        title="Admin Panel"
                                    >
                                        <i className="ri-user-settings-line text-xl text-gray-700"></i>
                                    </Link>
                                </>
                            ) : (
                                <Link
                                    to="/login"
                                    className="px-4 py-2 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors text-sm font-medium whitespace-nowrap cursor-pointer flex items-center gap-2"
                                >
                                    <i className="ri-login-box-line"></i>
                                    Sign In
                                </Link>
                            )}
                        </div>
                    </div>
                </div>
            </header>

            <CartModal isOpen={cartOpen} onClose={() => setCartOpen(false)} />
        </>
    );
}