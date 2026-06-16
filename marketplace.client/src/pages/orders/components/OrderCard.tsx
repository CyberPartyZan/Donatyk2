interface OrderCardProps {
    order: any;
    isExpanded: boolean;
    onToggle: () => void;
}

export default function OrderCard({ order, isExpanded, onToggle }: OrderCardProps) {
    const getStatusColor = (status: string) => {
        switch (status) {
            case 'Delivered': return 'bg-green-100 text-green-700';
            case 'In Transit': return 'bg-amber-100 text-amber-700';
            case 'Processing': return 'bg-blue-100 text-blue-700';
            case 'Cancelled': return 'bg-red-100 text-red-700';
            default: return 'bg-gray-100 text-gray-700';
        }
    };

    const getPaymentStatusColor = (status: string) => {
        switch (status) {
            case 'Paid': return 'text-green-600';
            case 'Pending': return 'text-amber-600';
            case 'Failed': return 'text-red-600';
            default: return 'text-gray-600';
        }
    };

    return (
        <div className="bg-white rounded-lg shadow-sm overflow-hidden">
            <div
                className="p-6 flex items-center justify-between cursor-pointer hover:bg-gray-50 transition-colors"
                onClick={onToggle}
            >
                <div className="flex items-center gap-6 flex-1">
                    <div>
                        <p className="text-sm text-gray-500 mb-1">Order Number</p>
                        <p className="font-semibold text-gray-900">#{order.orderNumber}</p>
                    </div>

                    <div>
                        <p className="text-sm text-gray-500 mb-1">Date</p>
                        <p className="font-medium text-gray-900">{order.date}</p>
                    </div>

                    <div>
                        <p className="text-sm text-gray-500 mb-1">Status</p>
                        <span className={`inline-block px-3 py-1 rounded-full text-xs font-medium whitespace-nowrap ${getStatusColor(order.status)}`}>
                            {order.status}
                        </span>
                    </div>

                    <div className="flex items-center gap-2">
                        {order.items.slice(0, 3).map((item: any, index: number) => (
                            <div key={index} className="w-12 h-12 bg-gray-100 rounded-lg overflow-hidden">
                                <img src={item.image} alt={item.name} className="w-full h-full object-cover object-top" />
                            </div>
                        ))}
                        {order.items.length > 3 && (
                            <div className="w-12 h-12 bg-gray-200 rounded-lg flex items-center justify-center">
                                <span className="text-xs font-medium text-gray-600">+{order.items.length - 3}</span>
                            </div>
                        )}
                    </div>
                </div>

                <button className="p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer">
                    <i className={`ri-arrow-${isExpanded ? 'up' : 'down'}-s-line text-xl text-gray-600`}></i>
                </button>
            </div>

            {isExpanded && (
                <div className="border-t border-gray-200 p-6">
                    <div className="grid grid-cols-2 gap-8">
                        <div>
                            <h3 className="font-semibold text-gray-900 mb-4">Order Information</h3>

                            <div className="space-y-4">
                                <div>
                                    <p className="text-sm text-gray-500 mb-1">Order Number</p>
                                    <p className="font-medium text-gray-900">#{order.orderNumber}</p>
                                </div>

                                <div>
                                    <p className="text-sm text-gray-500 mb-1">Order Date</p>
                                    <p className="font-medium text-gray-900">{order.date}</p>
                                </div>

                                <div>
                                    <p className="text-sm text-gray-500 mb-1">Status</p>
                                    <span className={`inline-block px-3 py-1 rounded-full text-xs font-medium whitespace-nowrap ${getStatusColor(order.status)}`}>
                                        {order.status}
                                    </span>
                                </div>

                                <div className="pt-4 border-t border-gray-200">
                                    <p className="text-sm text-gray-500 mb-2">Seller Information</p>
                                    <div className="flex items-center gap-3">
                                        <img
                                            src={order.seller.avatar}
                                            alt={order.seller.name}
                                            className="w-10 h-10 rounded-full object-cover object-top"
                                        />
                                        <div>
                                            <p className="font-medium text-gray-900">{order.seller.name}</p>
                                            <p className="text-xs text-gray-500">{order.seller.rating} ★ ({order.seller.reviews} reviews)</p>
                                        </div>
                                    </div>
                                </div>

                                <div className="pt-4 border-t border-gray-200">
                                    <p className="text-sm text-gray-500 mb-2">Delivery Information</p>
                                    <div className="space-y-2">
                                        <div>
                                            <p className="text-xs text-gray-500">Delivery Address</p>
                                            <p className="text-sm font-medium text-gray-900">{order.delivery.address}</p>
                                        </div>
                                        <div>
                                            <p className="text-xs text-gray-500">Recipient</p>
                                            <p className="text-sm font-medium text-gray-900">{order.delivery.recipient}</p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div>
                            <h3 className="font-semibold text-gray-900 mb-4">Order Items</h3>

                            <div className="space-y-4 mb-6">
                                {order.items.map((item: any) => (
                                    <div key={item.id} className="flex gap-4 p-4 bg-gray-50 rounded-lg">
                                        <div className="w-20 h-20 bg-white rounded-lg overflow-hidden flex-shrink-0">
                                            <img src={item.image} alt={item.name} className="w-full h-full object-cover object-top" />
                                        </div>
                                        <div className="flex-1">
                                            <h4 className="font-medium text-gray-900 mb-1 line-clamp-2">{item.name}</h4>
                                            <div className="flex items-center justify-between">
                                                <p className="text-sm text-gray-600">Qty: {item.quantity}</p>
                                                <p className="font-semibold text-teal-500">${item.price.toFixed(2)}</p>
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            </div>

                            <div className="space-y-3 pt-4 border-t border-gray-200">
                                <div className="flex items-center justify-between">
                                    <span className="text-sm text-gray-600">Payment Method</span>
                                    <span className="text-sm font-medium text-gray-900">{order.payment.method}</span>
                                </div>

                                <div className="flex items-center justify-between">
                                    <span className="text-sm text-gray-600">Payment Status</span>
                                    <span className={`text-sm font-semibold ${getPaymentStatusColor(order.payment.status)}`}>
                                        {order.payment.status}
                                    </span>
                                </div>

                                <div className="flex items-center justify-between">
                                    <span className="text-sm text-gray-600">Subtotal</span>
                                    <span className="text-sm font-medium text-gray-900">${order.subtotal.toFixed(2)}</span>
                                </div>

                                <div className="flex items-center justify-between">
                                    <span className="text-sm text-gray-600">Delivery Fee</span>
                                    <span className="text-sm font-medium text-gray-900">${order.deliveryFee.toFixed(2)}</span>
                                </div>

                                <div className="flex items-center justify-between pt-3 border-t border-gray-200">
                                    <span className="font-semibold text-gray-900">Total</span>
                                    <span className="text-xl font-bold text-teal-500">${order.total.toFixed(2)}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
