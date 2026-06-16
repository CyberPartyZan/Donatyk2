import { Link, useSearchParams } from 'react-router-dom';

export default function PaymentSuccessPage() {
    const [searchParams] = useSearchParams();
    const orderId = searchParams.get('orderId') || 'N/A';
    const type = searchParams.get('type') || 'simple';
    const amount = searchParams.get('amount') || '0';
    const tickets = searchParams.get('tickets');
    const bidAmount = searchParams.get('bidAmount');

    return (
        <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4">
            <div className="max-w-lg w-full bg-white rounded-lg shadow-sm border border-gray-200 p-8">
                <div className="text-center mb-8">
                    <div className="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-6">
                        <i className="ri-check-line text-4xl text-green-600"></i>
                    </div>
                    <h1 className="text-3xl font-bold text-gray-900 mb-2">Payment Successful!</h1>
                    <p className="text-gray-600 text-sm">Your payment has been processed successfully.</p>
                </div>

                <div className="bg-gray-50 rounded-lg p-5 space-y-3 mb-6">
                    <div className="flex items-center justify-between text-sm">
                        <span className="text-gray-600">Order ID</span>
                        <span className="font-semibold text-gray-900">#{orderId}</span>
                    </div>

                    {type === 'auction' ? (
                        <>
                            <div className="flex items-center justify-between text-sm">
                                <span className="text-gray-600">Type</span>
                                <span className="px-2 py-1 bg-amber-100 text-amber-700 text-xs font-medium rounded-full whitespace-nowrap">Auction Bid</span>
                            </div>
                            <div className="flex items-center justify-between text-sm">
                                <span className="text-gray-600">Bid Amount (Held)</span>
                                <span className="font-bold text-teal-600 text-lg">${parseFloat(bidAmount || amount).toFixed(2)}</span>
                            </div>
                            <div className="bg-amber-50 border border-amber-200 rounded-md p-3 mt-3">
                                <p className="text-xs text-amber-700 flex items-start gap-2">
                                    <i className="ri-information-line mt-0.5"></i>
                                    Your bid amount has been held. If your bid is outbid, the held amount will be released. If you win, it will be applied to the final purchase.
                                </p>
                            </div>
                        </>
                    ) : type === 'draw' ? (
                        <>
                            <div className="flex items-center justify-between text-sm">
                                <span className="text-gray-600">Type</span>
                                <span className="px-2 py-1 bg-purple-100 text-purple-700 text-xs font-medium rounded-full whitespace-nowrap">Draw Tickets</span>
                            </div>
                            <div className="flex items-center justify-between text-sm">
                                <span className="text-gray-600">Tickets Purchased</span>
                                <span className="font-bold text-gray-900">{tickets || '—'}</span>
                            </div>
                            <div className="flex items-center justify-between text-sm">
                                <span className="text-gray-600">Total Paid</span>
                                <span className="font-bold text-teal-600 text-lg">${parseFloat(amount).toFixed(2)}</span>
                            </div>
                            <div className="bg-purple-50 border border-purple-200 rounded-md p-3 mt-3">
                                <p className="text-xs text-purple-700 flex items-start gap-2">
                                    <i className="ri-information-line mt-0.5"></i>
                                    Good luck! Winners will be announced after the draw ends. Check your orders for updates.
                                </p>
                            </div>
                        </>
                    ) : (
                        <>
                            <div className="flex items-center justify-between text-sm">
                                <span className="text-gray-600">Type</span>
                                <span className="px-2 py-1 bg-blue-100 text-blue-700 text-xs font-medium rounded-full whitespace-nowrap">Purchase</span>
                            </div>
                            <div className="flex items-center justify-between text-sm">
                                <span className="text-gray-600">Total Paid</span>
                                <span className="font-bold text-teal-600 text-lg">${parseFloat(amount).toFixed(2)}</span>
                            </div>
                        </>
                    )}
                </div>

                <div className="flex flex-col gap-3">
                    <Link
                        to="/orders"
                        className="w-full py-3 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap text-center"
                    >
                        View My Orders
                    </Link>
                    <Link
                        to="/marketplace"
                        className="w-full py-3 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap text-center"
                    >
                        Continue Shopping
                    </Link>
                </div>
            </div>
        </div>
    );
}