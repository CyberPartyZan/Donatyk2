import { Link } from 'react-router-dom';

export default function PaymentCancelPage() {
    return (
        <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4">
            <div className="max-w-md w-full bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center">
                <div className="w-20 h-20 bg-amber-100 rounded-full flex items-center justify-center mx-auto mb-6">
                    <i className="ri-close-line text-4xl text-amber-600"></i>
                </div>
                <h1 className="text-3xl font-bold text-gray-900 mb-3">Payment Cancelled</h1>
                <p className="text-gray-600 text-sm mb-6">
                    Your payment was not completed. No charges have been made to your account.
                </p>
                <p className="text-gray-500 text-xs mb-8">
                    You can retry the payment or choose a different payment method.
                </p>
                <div className="flex flex-col gap-3">
                    <Link
                        to="/marketplace"
                        className="w-full py-3 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap text-center inline-block"
                    >
                        Back to Marketplace
                    </Link>
                    <Link
                        to="/orders"
                        className="w-full py-3 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap text-center inline-block"
                    >
                        View My Orders
                    </Link>
                </div>
            </div>
        </div>
    );
}