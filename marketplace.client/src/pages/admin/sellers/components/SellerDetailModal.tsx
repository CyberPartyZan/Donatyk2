interface Seller {
    id: string;
    name: string;
    description: string;
    email: string;
    phoneNumber: string;
    avatarImage: string;
    createdAt: string;
    totalLots: number;
    approvedLots: number;
    totalSales: number;
    rating: number;
}

interface SellerDetailModalProps {
    seller: Seller;
    onClose: () => void;
    onEdit: (seller: Seller) => void;
}

export default function SellerDetailModal({ seller, onClose, onEdit }: SellerDetailModalProps) {
    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    };

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto">
                <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
                    <h2 className="text-xl font-semibold text-gray-900">Seller Details</h2>
                    <button
                        onClick={onClose}
                        className="w-8 h-8 flex items-center justify-center text-gray-400 hover:text-gray-600 transition-colors"
                    >
                        <i className="ri-close-line text-xl"></i>
                    </button>
                </div>

                <div className="p-6">
                    <div className="flex items-start gap-6 mb-6">
                        <img
                            src={seller.avatarImage}
                            alt={seller.name}
                            className="w-32 h-32 rounded-lg object-cover flex-shrink-0"
                        />
                        <div className="flex-1">
                            <h3 className="text-2xl font-bold text-gray-900 mb-2">{seller.name}</h3>
                            <div className="flex items-center gap-2 mb-3">
                                <i className="ri-star-fill text-amber-500 text-lg"></i>
                                <span className="text-lg font-semibold text-gray-900">{seller.rating}</span>
                                <span className="text-gray-500">({seller.totalSales} sales)</span>
                            </div>
                            <p className="text-gray-600 leading-relaxed">{seller.description}</p>
                        </div>
                    </div>

                    <div className="grid grid-cols-4 gap-4 mb-6">
                        <div className="bg-gray-50 rounded-lg p-4 text-center">
                            <div className="text-3xl font-bold text-gray-900">{seller.totalLots}</div>
                            <div className="text-sm text-gray-500 mt-1">Total Lots</div>
                        </div>
                        <div className="bg-teal-50 rounded-lg p-4 text-center">
                            <div className="text-3xl font-bold text-teal-600">{seller.approvedLots}</div>
                            <div className="text-sm text-gray-500 mt-1">Approved</div>
                        </div>
                        <div className="bg-gray-50 rounded-lg p-4 text-center">
                            <div className="text-3xl font-bold text-gray-900">{seller.totalSales}</div>
                            <div className="text-sm text-gray-500 mt-1">Total Sales</div>
                        </div>
                        <div className="bg-amber-50 rounded-lg p-4 text-center">
                            <div className="text-3xl font-bold text-amber-600">{seller.rating}</div>
                            <div className="text-sm text-gray-500 mt-1">Rating</div>
                        </div>
                    </div>

                    <div className="space-y-4 mb-6">
                        <div>
                            <label className="text-sm font-medium text-gray-500 block mb-1">Email Address</label>
                            <div className="flex items-center gap-2 text-gray-900">
                                <i className="ri-mail-line text-gray-400"></i>
                                <span>{seller.email}</span>
                            </div>
                        </div>
                        <div>
                            <label className="text-sm font-medium text-gray-500 block mb-1">Phone Number</label>
                            <div className="flex items-center gap-2 text-gray-900">
                                <i className="ri-phone-line text-gray-400"></i>
                                <span>{seller.phoneNumber}</span>
                            </div>
                        </div>
                        <div>
                            <label className="text-sm font-medium text-gray-500 block mb-1">Member Since</label>
                            <div className="flex items-center gap-2 text-gray-900">
                                <i className="ri-calendar-line text-gray-400"></i>
                                <span>{formatDate(seller.createdAt)}</span>
                            </div>
                        </div>
                    </div>

                    <div className="flex gap-3 pt-4 border-t border-gray-200">
                        <button
                            onClick={() => {
                                onEdit(seller);
                                onClose();
                            }}
                            className="flex-1 px-4 py-2 bg-teal-600 text-white font-medium rounded-lg hover:bg-teal-700 transition-colors whitespace-nowrap"
                        >
                            Edit Seller Info
                        </button>
                        <button
                            onClick={onClose}
                            className="px-6 py-2 bg-gray-100 text-gray-700 font-medium rounded-lg hover:bg-gray-200 transition-colors whitespace-nowrap"
                        >
                            Close
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}