import { useState } from 'react';

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

interface SellerCardProps {
    seller: Seller;
    onEdit: (seller: Seller) => void;
    onDelete: (id: string) => void;
    onView: (seller: Seller) => void;
}

export default function SellerCard({ seller, onEdit, onDelete, onView }: SellerCardProps) {
    const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

    const handleDelete = () => {
        onDelete(seller.id);
        setShowDeleteConfirm(false);
    };

    return (
        <>
            <div className="bg-white rounded-lg border border-gray-200 hover:border-teal-500 transition-all duration-200 overflow-hidden">
                <div className="p-6">
                    <div className="flex items-start gap-4">
                        <img
                            src={seller.avatarImage}
                            alt={seller.name}
                            className="w-20 h-20 rounded-lg object-cover flex-shrink-0"
                        />
                        <div className="flex-1 min-w-0">
                            <h3 className="text-lg font-semibold text-gray-900 mb-1 truncate">
                                {seller.name}
                            </h3>
                            <p className="text-sm text-gray-600 mb-3 line-clamp-2">
                                {seller.description}
                            </p>
                            <div className="flex items-center gap-4 text-sm text-gray-500 mb-3">
                                <div className="flex items-center gap-1">
                                    <i className="ri-mail-line"></i>
                                    <span className="truncate">{seller.email}</span>
                                </div>
                                <div className="flex items-center gap-1">
                                    <i className="ri-phone-line"></i>
                                    <span>{seller.phoneNumber}</span>
                                </div>
                            </div>
                            <div className="flex items-center gap-1 text-sm">
                                <i className="ri-star-fill text-amber-500"></i>
                                <span className="font-medium text-gray-900">{seller.rating}</span>
                                <span className="text-gray-500">({seller.totalSales} sales)</span>
                            </div>
                        </div>
                    </div>

                    <div className="grid grid-cols-3 gap-4 mt-4 pt-4 border-t border-gray-100">
                        <div className="text-center">
                            <div className="text-2xl font-bold text-gray-900">{seller.totalLots}</div>
                            <div className="text-xs text-gray-500 mt-1">Total Lots</div>
                        </div>
                        <div className="text-center">
                            <div className="text-2xl font-bold text-teal-600">{seller.approvedLots}</div>
                            <div className="text-xs text-gray-500 mt-1">Approved</div>
                        </div>
                        <div className="text-center">
                            <div className="text-2xl font-bold text-gray-900">{seller.totalSales}</div>
                            <div className="text-xs text-gray-500 mt-1">Sales</div>
                        </div>
                    </div>

                    <div className="flex items-center gap-2 mt-4 pt-4 border-t border-gray-100">
                        <button
                            onClick={() => onView(seller)}
                            className="flex-1 px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors whitespace-nowrap"
                        >
                            View Details
                        </button>
                        <button
                            onClick={() => onEdit(seller)}
                            className="px-4 py-2 bg-gray-100 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-200 transition-colors whitespace-nowrap"
                        >
                            <i className="ri-edit-line"></i>
                        </button>
                        <button
                            onClick={() => setShowDeleteConfirm(true)}
                            className="px-4 py-2 bg-red-50 text-red-600 text-sm font-medium rounded-lg hover:bg-red-100 transition-colors whitespace-nowrap"
                        >
                            <i className="ri-delete-bin-line"></i>
                        </button>
                    </div>
                </div>
            </div>

            {showDeleteConfirm && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg max-w-md w-full p-6">
                        <div className="flex items-center gap-3 mb-4">
                            <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center flex-shrink-0">
                                <i className="ri-alert-line text-red-600 text-xl"></i>
                            </div>
                            <div>
                                <h3 className="text-lg font-semibold text-gray-900">Delete Seller</h3>
                                <p className="text-sm text-gray-600">This action cannot be undone</p>
                            </div>
                        </div>
                        <p className="text-gray-700 mb-6">
                            Are you sure you want to delete <strong>{seller.name}</strong>? All associated lots and data will be permanently removed.
                        </p>
                        <div className="flex gap-3">
                            <button
                                onClick={() => setShowDeleteConfirm(false)}
                                className="flex-1 px-4 py-2 bg-gray-100 text-gray-700 font-medium rounded-lg hover:bg-gray-200 transition-colors whitespace-nowrap"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleDelete}
                                className="flex-1 px-4 py-2 bg-red-600 text-white font-medium rounded-lg hover:bg-red-700 transition-colors whitespace-nowrap"
                            >
                                Delete Seller
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}