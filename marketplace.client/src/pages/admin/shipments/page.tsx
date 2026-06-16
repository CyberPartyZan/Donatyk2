import { useState } from 'react';
import { mockShipments } from '@/mocks/shipments';

interface Shipment {
    id: string;
    orderId: string;
    orderNumber: string;
    lotName: string;
    lotImage: string;
    buyerName: string;
    carrier: string;
    trackingNumber: string;
    address: string;
    recipientName: string;
    recipientPhone: string;
    status: 'Pending' | 'In Transit' | 'Delivered';
    processedAt: string | null;
    deliveredAt: string | null;
    notes: string;
}

export default function ShipmentsAdmin() {
    const [shipments, setShipments] = useState<Shipment[]>(mockShipments);
    const [activeTab, setActiveTab] = useState<'all' | 'unprocessed'>('all');
    const [searchQuery, setSearchQuery] = useState('');
    const [processingId, setProcessingId] = useState<string | null>(null);
    const [trackingInput, setTrackingInput] = useState('');

    const unprocessed = shipments.filter(s => s.status === 'Pending');
    const processed = shipments.filter(s => s.status !== 'Pending');
    const currentList = activeTab === 'all' ? shipments : unprocessed;

    const filtered = currentList.filter(s => {
        const q = searchQuery.toLowerCase();
        return (
            s.lotName.toLowerCase().includes(q) ||
            s.orderNumber.toLowerCase().includes(q) ||
            s.buyerName.toLowerCase().includes(q) ||
            s.address.toLowerCase().includes(q)
        );
    });

    const openProcessModal = (id: string) => {
        setProcessingId(id);
        setTrackingInput('');
    };

    const handleProcess = (id: string) => {
        if (!trackingInput.trim()) return;
        setShipments(shipments.map(s =>
            s.id === id
                ? { ...s, status: 'In Transit' as const, trackingNumber: trackingInput.trim(), processedAt: new Date().toISOString() }
                : s
        ));
        setProcessingId(null);
        setTrackingInput('');
    };

    const statusColors: Record<string, string> = {
        'Pending': 'bg-amber-100 text-amber-700',
        'In Transit': 'bg-blue-100 text-blue-700',
        'Delivered': 'bg-emerald-100 text-emerald-700',
    };

    const statusIcons: Record<string, string> = {
        'Pending': 'ri-time-line',
        'In Transit': 'ri-truck-line',
        'Delivered': 'ri-check-double-line',
    };

    return (
        <div className="p-8">
            <div className="max-w-5xl mx-auto space-y-6">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">Shipments</h1>
                    <p className="text-sm text-gray-600 mt-1">Manage and track all outgoing shipments</p>
                </div>

                {/* Stats */}
                <div className="grid grid-cols-4 gap-4">
                    <div className="bg-white rounded-lg border border-gray-200 p-4">
                        <p className="text-sm text-gray-600 mb-1">Total</p>
                        <p className="text-2xl font-bold text-gray-900">{shipments.length}</p>
                    </div>
                    <div className="bg-amber-50 rounded-lg border border-amber-200 p-4">
                        <p className="text-sm text-amber-600 mb-1">Pending</p>
                        <p className="text-2xl font-bold text-amber-900">{unprocessed.length}</p>
                    </div>
                    <div className="bg-blue-50 rounded-lg border border-blue-200 p-4">
                        <p className="text-sm text-blue-600 mb-1">In Transit</p>
                        <p className="text-2xl font-bold text-blue-900">{shipments.filter(s => s.status === 'In Transit').length}</p>
                    </div>
                    <div className="bg-emerald-50 rounded-lg border border-emerald-200 p-4">
                        <p className="text-sm text-emerald-600 mb-1">Delivered</p>
                        <p className="text-2xl font-bold text-emerald-900">{shipments.filter(s => s.status === 'Delivered').length}</p>
                    </div>
                </div>

                {/* Tabs */}
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2 bg-gray-100 p-1 rounded-full">
                        <button
                            onClick={() => setActiveTab('all')}
                            className={`px-4 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${activeTab === 'all' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'
                                }`}
                        >
                            <i className="ri-list-check mr-2"></i>All Shipments
                        </button>
                        <button
                            onClick={() => setActiveTab('unprocessed')}
                            className={`px-4 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${activeTab === 'unprocessed' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'
                                }`}
                        >
                            <i className="ri-timer-line mr-2"></i>Unprocessed
                            {unprocessed.length > 0 && (
                                <span className="ml-1.5 px-1.5 py-0.5 bg-amber-500 text-white text-xs rounded-full">{unprocessed.length}</span>
                            )}
                        </button>
                    </div>
                    <div className="relative w-64">
                        <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                        <input
                            type="text"
                            placeholder="Search shipments..."
                            value={searchQuery}
                            onChange={e => setSearchQuery(e.target.value)}
                            className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                        />
                    </div>
                </div>

                {/* Shipments List */}
                {filtered.length === 0 ? (
                    <div className="text-center py-16 bg-white rounded-lg border border-gray-200">
                        <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                            <i className="ri-truck-line text-5xl text-gray-300"></i>
                        </div>
                        <p className="text-gray-500 text-sm">No shipments found</p>
                    </div>
                ) : (
                    <div className="grid gap-4">
                        {filtered.map(shipment => (
                            <div key={shipment.id} className="bg-white rounded-lg border border-gray-200 p-5">
                                <div className="flex items-start gap-4">
                                    <div className="w-20 h-20 rounded-lg bg-gray-100 overflow-hidden flex-shrink-0 border border-gray-200">
                                        <img src={shipment.lotImage} alt={shipment.lotName} className="w-full h-full object-cover object-top" />
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <div className="flex items-start justify-between mb-2">
                                            <div>
                                                <h3 className="text-base font-semibold text-gray-900">{shipment.lotName}</h3>
                                                <p className="text-sm text-gray-600">Order #{shipment.orderNumber} • Buyer: {shipment.buyerName}</p>
                                            </div>
                                            <span className={`inline-flex items-center gap-1.5 px-3 py-1 text-xs font-medium rounded-full whitespace-nowrap ${statusColors[shipment.status]}`}>
                                                <i className={statusIcons[shipment.status]}></i>
                                                {shipment.status}
                                            </span>
                                        </div>

                                        <div className="grid grid-cols-2 gap-3 mt-3">
                                            <div>
                                                <p className="text-xs text-gray-500 mb-0.5">Carrier</p>
                                                <p className="text-sm font-medium text-gray-900">{shipment.carrier}</p>
                                                {shipment.status !== 'Pending' && (
                                                    <p className="text-xs text-gray-500 font-mono">{shipment.trackingNumber}</p>
                                                )}
                                            </div>
                                            <div>
                                                <p className="text-xs text-gray-500 mb-0.5">Recipient</p>
                                                <p className="text-sm font-medium text-gray-900">{shipment.recipientName}</p>
                                                <p className="text-xs text-gray-500">{shipment.recipientPhone}</p>
                                            </div>
                                        </div>

                                        <div className="mt-3">
                                            <p className="text-xs text-gray-500 mb-0.5">Shipping Address</p>
                                            <p className="text-sm text-gray-700">{shipment.address}</p>
                                        </div>

                                        {shipment.notes && (
                                            <p className="text-xs text-gray-400 mt-2 flex items-center gap-1">
                                                <i className="ri-information-line"></i>{shipment.notes}
                                            </p>
                                        )}

                                        {shipment.processedAt && (
                                            <p className="text-xs text-gray-400 mt-1">
                                                Processed: {new Date(shipment.processedAt).toLocaleString()}
                                                {shipment.deliveredAt && ` • Delivered: ${new Date(shipment.deliveredAt).toLocaleString()}`}
                                            </p>
                                        )}
                                    </div>

                                    <div className="flex-shrink-0">
                                        {shipment.status === 'Pending' && (
                                            <button
                                                onClick={() => openProcessModal(shipment.id)}
                                                className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                                            >
                                                <i className="ri-play-circle-line mr-1.5"></i>Process
                                            </button>
                                        )}
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            {/* Process Confirmation */}
            {processingId && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
                        <div className="w-12 h-12 rounded-full bg-teal-100 flex items-center justify-center mx-auto mb-4">
                            <i className="ri-truck-line text-teal-600 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900 text-center mb-2">Process Shipment</h3>
                        <p className="text-sm text-gray-600 text-center mb-4">
                            Enter the tracking number to mark this shipment as <strong>In Transit</strong>.
                        </p>
                        {(() => {
                            const s = shipments.find(sh => sh.id === processingId);
                            return s ? (
                                <div className="bg-gray-50 rounded-lg p-3 mb-4 text-sm">
                                    <p className="font-medium text-gray-900">{s.lotName}</p>
                                    <p className="text-gray-600">{s.carrier}</p>
                                    <p className="text-gray-500 text-xs mt-1">{s.address}</p>
                                </div>
                            ) : null;
                        })()}
                        <div className="mb-4">
                            <label className="block text-sm font-medium text-gray-700 mb-1">Tracking Number</label>
                            <input
                                type="text"
                                value={trackingInput}
                                onChange={e => setTrackingInput(e.target.value)}
                                placeholder="e.g. FDX-8942-5612-7830"
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                autoFocus
                            />
                        </div>
                        <div className="flex items-center gap-3">
                            <button onClick={() => { setProcessingId(null); setTrackingInput(''); }} className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">
                                Cancel
                            </button>
                            <button
                                onClick={() => handleProcess(processingId)}
                                disabled={!trackingInput.trim()}
                                className={`flex-1 px-4 py-2 text-white text-sm font-medium rounded-lg transition-colors cursor-pointer whitespace-nowrap ${!trackingInput.trim() ? 'bg-gray-300 cursor-not-allowed' : 'bg-teal-600 hover:bg-teal-700'
                                    }`}
                            >
                                Confirm Process
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}