import { useState } from 'react';

interface Characteristic {
    key: string;
    value: string;
}

interface Lot {
    id: string;
    name: string;
    description: string;
    price: number;
    compensation: number;
    stockCount: number;
    discountedPrice: number | null;
    type: 'Simple' | 'Auction' | 'Draw';
    stage: 'Created' | 'PendingApproval' | 'Denied' | 'Approved';
    isActive: boolean;
    categoryId: string;
    categoryName: string;
    characteristics: Characteristic[];
    endOfAuction?: string;
    auctionStepPercent?: number;
    currentBid?: number;
    ticketPrice?: number;
    totalTickets?: number;
    ticketsSold?: number;
    createdAt: string;
    denialReason?: string;
}

interface LotCardProps {
    lot: Lot;
    onEdit: (lot: Lot) => void;
    onDelete: (id: string) => void;
    onApprove?: (id: string) => void;
    onDecline?: (id: string) => void;
    onToggleActive?: (id: string, isActive: boolean) => void;
}

export default function LotCard({ lot, onEdit, onDelete, onApprove, onDecline, onToggleActive }: LotCardProps) {
    const [expanded, setExpanded] = useState(false);

    const getTypeColor = (type: string) => {
        switch (type) {
            case 'Auction': return 'bg-amber-100 text-amber-700';
            case 'Draw': return 'bg-purple-100 text-purple-700';
            default: return 'bg-blue-100 text-blue-700';
        }
    };

    const getStageColor = (stage: string) => {
        switch (stage) {
            case 'Approved': return 'bg-green-100 text-green-700';
            case 'PendingApproval': return 'bg-yellow-100 text-yellow-700';
            case 'Denied': return 'bg-red-100 text-red-700';
            default: return 'bg-gray-100 text-gray-700';
        }
    };

    return (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
            <div className="p-5">
                <div className="flex items-start justify-between gap-4 mb-3">
                    <div className="flex-1 min-w-0">
                        <h3 className="text-lg font-semibold text-gray-900 mb-1 truncate">{lot.name}</h3>
                        <p className="text-sm text-gray-600 line-clamp-2">{lot.description}</p>
                    </div>
                    <div className="flex items-center gap-2 flex-shrink-0">
                        <span className={`px-2 py-1 rounded-full text-xs font-medium whitespace-nowrap ${getTypeColor(lot.type)}`}>
                            {lot.type}
                        </span>
                        <span className={`px-2 py-1 rounded-full text-xs font-medium whitespace-nowrap ${getStageColor(lot.stage)}`}>
                            {lot.stage}
                        </span>
                    </div>
                </div>

                <div className="grid grid-cols-2 gap-3 mb-4">
                    <div>
                        <p className="text-xs text-gray-500 mb-1">Price</p>
                        <p className="text-sm font-semibold text-gray-900">${lot.price.toLocaleString()}</p>
                    </div>
                    <div>
                        <p className="text-xs text-gray-500 mb-1">Compensation</p>
                        <p className="text-sm font-semibold text-gray-900">${lot.compensation.toLocaleString()}</p>
                    </div>
                    <div>
                        <p className="text-xs text-gray-500 mb-1">Stock</p>
                        <p className="text-sm font-semibold text-gray-900">{lot.stockCount}</p>
                    </div>
                    <div>
                        <p className="text-xs text-gray-500 mb-1">Category</p>
                        <p className="text-sm font-semibold text-gray-900">{lot.categoryName}</p>
                    </div>
                </div>

                {lot.type === 'Auction' && (
                    <div className="bg-amber-50 rounded-lg p-3 mb-4">
                        <div className="grid grid-cols-2 gap-3">
                            <div>
                                <p className="text-xs text-amber-700 mb-1">Current Bid</p>
                                <p className="text-sm font-semibold text-amber-900">${lot.currentBid?.toLocaleString() || 0}</p>
                            </div>
                            <div>
                                <p className="text-xs text-amber-700 mb-1">Ends</p>
                                <p className="text-sm font-semibold text-amber-900">
                                    {lot.endOfAuction ? new Date(lot.endOfAuction).toLocaleDateString() : 'N/A'}
                                </p>
                            </div>
                        </div>
                    </div>
                )}

                {lot.type === 'Draw' && (
                    <div className="bg-purple-50 rounded-lg p-3 mb-4">
                        <div className="grid grid-cols-3 gap-3">
                            <div>
                                <p className="text-xs text-purple-700 mb-1">Ticket Price</p>
                                <p className="text-sm font-semibold text-purple-900">${lot.ticketPrice}</p>
                            </div>
                            <div>
                                <p className="text-xs text-purple-700 mb-1">Sold</p>
                                <p className="text-sm font-semibold text-purple-900">{lot.ticketsSold}/{lot.totalTickets}</p>
                            </div>
                            <div>
                                <p className="text-xs text-purple-700 mb-1">Left</p>
                                <p className="text-sm font-semibold text-purple-900">{(lot.totalTickets || 0) - (lot.ticketsSold || 0)}</p>
                            </div>
                        </div>
                    </div>
                )}

                {lot.stage === 'Denied' && lot.denialReason && (
                    <div className="bg-red-50 border border-red-200 rounded-lg p-3 mb-4">
                        <p className="text-xs text-red-700 font-medium mb-1">Denial Reason</p>
                        <p className="text-sm text-red-900">{lot.denialReason}</p>
                    </div>
                )}

                <button
                    onClick={() => setExpanded(!expanded)}
                    className="text-sm text-teal-600 hover:text-teal-700 font-medium flex items-center gap-1 mb-3 cursor-pointer whitespace-nowrap"
                >
                    <i className={`ri-arrow-${expanded ? 'up' : 'down'}-s-line`}></i>
                    {expanded ? 'Hide' : 'Show'} Characteristics
                </button>

                {expanded && (
                    <div className="bg-gray-50 rounded-lg p-3 mb-4">
                        <div className="grid grid-cols-2 gap-2">
                            {lot.characteristics.map((char, index) => (
                                <div key={index} className="text-sm">
                                    <span className="text-gray-600">{char.key}:</span>{' '}
                                    <span className="text-gray-900 font-medium">{char.value}</span>
                                </div>
                            ))}
                        </div>
                    </div>
                )}

                <div className="flex items-center gap-2 flex-wrap">
                    {lot.stage === 'PendingApproval' && onApprove && onDecline && (
                        <>
                            <button
                                onClick={() => onApprove(lot.id)}
                                className="px-4 py-2 bg-green-600 text-white text-sm font-medium rounded-lg hover:bg-green-700 transition-colors cursor-pointer whitespace-nowrap"
                            >
                                <i className="ri-check-line mr-1"></i>
                                Approve
                            </button>
                            <button
                                onClick={() => onDecline(lot.id)}
                                className="px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors cursor-pointer whitespace-nowrap"
                            >
                                <i className="ri-close-line mr-1"></i>
                                Decline
                            </button>
                        </>
                    )}

                    {lot.stage === 'Approved' && onToggleActive && (
                        <button
                            onClick={() => onToggleActive(lot.id, !lot.isActive)}
                            className={`px-4 py-2 text-sm font-medium rounded-lg transition-colors cursor-pointer whitespace-nowrap ${lot.isActive
                                    ? 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                                    : 'bg-teal-600 text-white hover:bg-teal-700'
                                }`}
                        >
                            <i className={`ri-${lot.isActive ? 'pause' : 'play'}-line mr-1`}></i>
                            {lot.isActive ? 'Deactivate' : 'Activate'}
                        </button>
                    )}

                    <button
                        onClick={() => onEdit(lot)}
                        className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                    >
                        <i className="ri-edit-line mr-1"></i>
                        Edit
                    </button>

                    <button
                        onClick={() => onDelete(lot.id)}
                        className="px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors cursor-pointer whitespace-nowrap"
                    >
                        <i className="ri-delete-bin-line mr-1"></i>
                        Delete
                    </button>
                </div>
            </div>
        </div>
    );
}