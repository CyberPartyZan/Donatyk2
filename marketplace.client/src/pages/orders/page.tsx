import { useState } from 'react';
import { Link } from 'react-router-dom';
import OrdersHeader from './components/OrdersHeader';
import OrderCard from './components/OrderCard';
import { orders } from '../../mocks/orders';

export default function Orders() {
    const [expandedOrders, setExpandedOrders] = useState<string[]>([]);

    const toggleOrder = (orderId: string) => {
        setExpandedOrders(prev =>
            prev.includes(orderId)
                ? prev.filter(id => id !== orderId)
                : [...prev, orderId]
        );
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <OrdersHeader />

            <div className="max-w-[1400px] mx-auto px-6 py-8">
                <div className="mb-6">
                    <h1 className="text-2xl font-bold text-gray-900 mb-2">My Orders</h1>
                    <p className="text-sm text-gray-600">Track and manage your orders</p>
                </div>

                <div className="space-y-4">
                    {orders.map((order) => (
                        <OrderCard
                            key={order.id}
                            order={order}
                            isExpanded={expandedOrders.includes(order.id)}
                            onToggle={() => toggleOrder(order.id)}
                        />
                    ))}
                </div>

                {orders.length === 0 && (
                    <div className="text-center py-16 bg-white rounded-lg">
                        <i className="ri-shopping-bag-line text-6xl text-gray-300 mb-4"></i>
                        <p className="text-gray-500 text-lg mb-2">No orders yet</p>
                        <p className="text-gray-400 text-sm mb-6">Start shopping to see your orders here</p>
                        <Link
                            to="/marketplace"
                            className="inline-block px-6 py-3 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors font-medium whitespace-nowrap cursor-pointer"
                        >
                            Browse Marketplace
                        </Link>
                    </div>
                )}
            </div>
        </div>
    );
}
