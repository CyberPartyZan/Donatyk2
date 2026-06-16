import { useState } from 'react';
import { mockCompensations } from '@/mocks/compensations';
import { mockSellers } from '@/mocks/sellers';

interface Compensation {
    id: string;
    lotId: string;
    lotName: string;
    lotImage: string;
    soldAt: string;
    soldPrice: number;
    compensationAmount: number;
    compensationRate: number;
    buyerName: string;
    status: 'Paid' | 'Pending' | 'Requested';
    paidAt: string | null;
    transactionId: string;
    sellerId?: string;
    sellerName?: string;
}

const allCompensations: Compensation[] = mockCompensations.map(comp => {
    const sellerId = comp.id === 'comp-1' || comp.id === 'comp-2' || comp.id === 'comp-3' || comp.id === 'comp-4' || comp.id === 'comp-5' || comp.id === 'comp-6' || comp.id === 'comp-7' ? 'seller-1' : 'seller-2';
    return {
        ...comp,
        sellerId,
        sellerName: mockSellers.find(s => s.id === sellerId)?.name || 'Premium Luxury Goods'
    };
});

export default function CompensationsAdmin() {
    const [compensations, setCompensations] = useState<Compensation[]>(allCompensations);
    const [activeTab, setActiveTab] = useState<'all' | 'pending' | 'requests'>('requests');
    const [searchQuery, setSearchQuery] = useState('');
    const [activeSearchQuery, setActiveSearchQuery] = useState('');
    const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
    const [processModalOpen, setProcessModalOpen] = useState(false);
    const [approveModalOpen, setApproveModalOpen] = useState(false);
    const [approvingId, setApprovingId] = useState<string | null>(null);
    const [approvalFile, setApprovalFile] = useState<File | null>(null);
    const [approvalFileName, setApprovalFileName] = useState('');
    const [processError, setProcessError] = useState('');

    const pendingComps = compensations.filter(c => c.status === 'Pending');
    const requestedComps = compensations.filter(c => c.status === 'Requested');
    const paidComps = compensations.filter(c => c.status === 'Paid');

    const getTabList = () => {
        if (activeTab === 'requests') return requestedComps;
        if (activeTab === 'pending') return pendingComps;
        return compensations;
    };
    const currentComps = getTabList();

    const filteredComps = currentComps.filter(c =>
        c.lotName.toLowerCase().includes(activeSearchQuery.toLowerCase()) ||
        c.sellerName?.toLowerCase().includes(activeSearchQuery.toLowerCase()) ||
        c.buyerName.toLowerCase().includes(activeSearchQuery.toLowerCase())
    );

    const groupedBySeller = filteredComps.reduce((acc, comp) => {
        const key = comp.sellerId || 'unknown';
        if (!acc[key]) {
            acc[key] = {
                sellerId: key,
                sellerName: comp.sellerName || 'Unknown Seller',
                compensations: []
            };
        }
        acc[key].compensations.push(comp);
        return acc;
    }, {} as Record<string, { sellerId: string; sellerName: string; compensations: Compensation[] }>);

    const toggleSelect = (id: string) => {
        const next = new Set(selectedIds);
        if (next.has(id)) next.delete(id);
        else next.add(id);
        setSelectedIds(next);
    };

    const toggleSelectAll = () => {
        const selectableIds = filteredComps.filter(c => c.status === 'Pending' || c.status === 'Requested').map(c => c.id);
        if (selectableIds.every(id => selectedIds.has(id))) {
            setSelectedIds(new Set());
        } else {
            setSelectedIds(new Set(selectableIds));
        }
    };

    const selectedProcessable = Array.from(selectedIds).filter(id => {
        const c = compensations.find(co => co.id === id);
        return c && (c.status === 'Pending' || c.status === 'Requested');
    });

    const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            setApprovalFile(file);
            setApprovalFileName(file.name);
            setProcessError('');
        }
    };

    const handleProcessPayments = () => {
        if (!approvalFile) {
            setProcessError('Please upload a PDF approval document to confirm payment.');
            return;
        }
        const now = new Date().toISOString();
        setCompensations(prev =>
            prev.map(c =>
                selectedIds.has(c.id) && (c.status === 'Pending' || c.status === 'Requested')
                    ? { ...c, status: 'Paid' as const, paidAt: now }
                    : c
            )
        );
        setSelectedIds(new Set());
        setProcessModalOpen(false);
        setApprovalFile(null);
        setApprovalFileName('');
        setProcessError('');
    };

    const openApproveModal = (id: string) => {
        setApprovingId(id);
        setApprovalFile(null);
        setApprovalFileName('');
        setProcessError('');
        setApproveModalOpen(true);
    };

    const handleSingleApprove = () => {
        if (!approvalFile || !approvingId) {
            setProcessError('Please upload a PDF approval document to confirm payment.');
            return;
        }
        const now = new Date().toISOString();
        setCompensations(prev =>
            prev.map(c =>
                c.id === approvingId
                    ? { ...c, status: 'Paid' as const, paidAt: now }
                    : c
            )
        );
        setApproveModalOpen(false);
        setApprovingId(null);
        setApprovalFile(null);
        setApprovalFileName('');
        setProcessError('');
    };

    const sellerList = Object.values(groupedBySeller);

    const stats = {
        total: compensations.length,
        pending: pendingComps.length,
        requested: requestedComps.length,
        paid: paidComps.length,
        totalRequestedAmount: requestedComps.reduce((sum, c) => sum + c.compensationAmount, 0),
        totalPendingAmount: pendingComps.reduce((sum, c) => sum + c.compensationAmount, 0)
    };

    const statusBadge = (status: string) => {
        if (status === 'Paid') return 'bg-emerald-100 text-emerald-700';
        if (status === 'Requested') return 'bg-orange-100 text-orange-700';
        return 'bg-amber-100 text-amber-700';
    };

    const statusIcon = (status: string) => {
        if (status === 'Paid') return 'ri-check-double-line';
        if (status === 'Requested') return 'ri-question-answer-line';
        return 'ri-time-line';
    };

    return (
        <div>
            <div className="mb-6">
                <h2 className="text-2xl font-bold text-gray-900">Compensations Management</h2>
                <p className="text-gray-600 mt-1">Review and process seller compensation payments</p>
            </div>

            {/* Stats */}
            <div className="grid grid-cols-5 gap-4 mb-6">
                <div className="bg-white rounded-lg border border-gray-200 p-4">
                    <p className="text-sm text-gray-600 mb-1">Total</p>
                    <p className="text-2xl font-bold text-gray-900">{stats.total}</p>
                </div>
                <div className="bg-white rounded-lg border border-gray-200 p-4">
                    <p className="text-sm text-orange-600 mb-1">Requested</p>
                    <p className="text-2xl font-bold text-orange-600">{stats.requested}</p>
                </div>
                <div className="bg-white rounded-lg border border-gray-200 p-4">
                    <p className="text-sm text-amber-600 mb-1">Pending</p>
                    <p className="text-2xl font-bold text-amber-600">{stats.pending}</p>
                </div>
                <div className="bg-white rounded-lg border border-gray-200 p-4">
                    <p className="text-sm text-green-600 mb-1">Paid</p>
                    <p className="text-2xl font-bold text-green-600">{stats.paid}</p>
                </div>
                <div className="bg-white rounded-lg border border-gray-200 p-4">
                    <p className="text-sm text-gray-600 mb-1">Requested Amount</p>
                    <p className="text-2xl font-bold text-orange-600">${stats.totalRequestedAmount.toLocaleString()}</p>
                </div>
            </div>

            {/* Tabs & Actions */}
            <div className="bg-white rounded-lg border border-gray-200">
                <div className="px-6 py-4 border-b border-gray-200">
                    <div className="flex items-center justify-between mb-4">
                        <div className="flex items-center gap-2 bg-gray-100 p-1 rounded-full">
                            <button
                                onClick={() => setActiveTab('requests')}
                                className={`px-4 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${activeTab === 'requests' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'
                                    }`}
                            >
                                <i className="ri-question-answer-line mr-2"></i>
                                Requests
                                <span className="ml-1.5 px-1.5 py-0.5 bg-orange-100 text-orange-700 text-xs rounded-full">{stats.requested}</span>
                            </button>
                            <button
                                onClick={() => setActiveTab('pending')}
                                className={`px-4 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${activeTab === 'pending' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'
                                    }`}
                            >
                                <i className="ri-time-line mr-2"></i>
                                Pending
                                <span className="ml-1.5 px-1.5 py-0.5 bg-amber-100 text-amber-700 text-xs rounded-full">{stats.pending}</span>
                            </button>
                            <button
                                onClick={() => setActiveTab('all')}
                                className={`px-4 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${activeTab === 'all' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'
                                    }`}
                            >
                                <i className="ri-file-list-3-line mr-2"></i>
                                All
                                <span className="ml-1.5 px-1.5 py-0.5 bg-gray-200 text-gray-700 text-xs rounded-full">{stats.total}</span>
                            </button>
                        </div>

                        {((activeTab === 'pending' || activeTab === 'requests') && selectedProcessable.length > 0) && (
                            <button
                                onClick={() => setProcessModalOpen(true)}
                                className="px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                            >
                                <i className="ri-check-double-line"></i>
                                Process Selected ({selectedProcessable.length})
                            </button>
                        )}
                    </div>

                    <div className="flex items-center gap-3">
                        <div className="flex-1 relative">
                            <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                            <input
                                type="text"
                                placeholder="Search by lot name, seller, or buyer..."
                                value={searchQuery}
                                onChange={(e) => setSearchQuery(e.target.value)}
                                onKeyDown={(e) => { if (e.key === 'Enter') setActiveSearchQuery(searchQuery); }}
                                className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                            />
                        </div>
                        <button
                            onClick={() => setActiveSearchQuery(searchQuery)}
                            className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                        >
                            <i className="ri-search-line"></i>
                            Search
                        </button>
                    </div>
                </div>

                <div className="p-6">
                    {filteredComps.length === 0 ? (
                        <div className="text-center py-12">
                            <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                                <i className="ri-money-dollar-circle-line text-5xl text-gray-300"></i>
                            </div>
                            <p className="text-gray-500 text-sm">
                                {activeTab === 'requests' ? 'No compensation requests' : activeTab === 'pending' ? 'No pending compensations' : 'No compensations found'}
                            </p>
                        </div>
                    ) : (
                        <div className="space-y-8">
                            {sellerList.map(group => (
                                <div key={group.sellerId}>
                                    <div className="flex items-center gap-3 mb-3">
                                        <div className="w-10 h-10 rounded-full bg-gray-200 overflow-hidden flex-shrink-0">
                                            <img
                                                src={mockSellers.find(s => s.id === group.sellerId)?.avatarImage || ''}
                                                alt={group.sellerName}
                                                className="w-full h-full object-cover object-top"
                                            />
                                        </div>
                                        <div>
                                            <h3 className="text-base font-semibold text-gray-900">{group.sellerName}</h3>
                                            <p className="text-xs text-gray-500">
                                                {group.compensations.length} compensation{group.compensations.length !== 1 ? 's' : ''}
                                                {' · '}
                                                ${group.compensations.reduce((sum, c) => sum + c.compensationAmount, 0).toLocaleString()} total
                                            </p>
                                        </div>
                                        {(activeTab === 'pending' || activeTab === 'requests') && (
                                            <button
                                                onClick={toggleSelectAll}
                                                className="ml-auto text-xs text-teal-600 hover:text-teal-700 cursor-pointer whitespace-nowrap"
                                            >
                                                {group.compensations.filter(c => c.status !== 'Paid').every(c => selectedIds.has(c.id))
                                                    ? 'Deselect All'
                                                    : 'Select All'}
                                            </button>
                                        )}
                                    </div>

                                    <div className="space-y-2">
                                        {group.compensations.map(comp => (
                                            <div
                                                key={comp.id}
                                                className={`flex items-center gap-4 p-4 rounded-lg border transition-colors ${selectedIds.has(comp.id)
                                                        ? 'bg-emerald-50 border-emerald-300'
                                                        : 'bg-gray-50 border-gray-200'
                                                    }`}
                                            >
                                                {comp.status !== 'Paid' && (
                                                    <input
                                                        type="checkbox"
                                                        checked={selectedIds.has(comp.id)}
                                                        onChange={() => toggleSelect(comp.id)}
                                                        className="w-4 h-4 text-teal-600 border-gray-300 rounded focus:ring-teal-500 cursor-pointer flex-shrink-0"
                                                    />
                                                )}
                                                {comp.status === 'Paid' && (
                                                    <div className="w-4 flex-shrink-0"></div>
                                                )}
                                                <div className="w-14 h-14 rounded-lg bg-white overflow-hidden flex-shrink-0 border border-gray-200">
                                                    <img src={comp.lotImage} alt={comp.lotName} className="w-full h-full object-cover object-top" />
                                                </div>
                                                <div className="flex-1 min-w-0">
                                                    <div className="flex items-start justify-between mb-1">
                                                        <div>
                                                            <h4 className="text-sm font-semibold text-gray-900">{comp.lotName}</h4>
                                                            <p className="text-xs text-gray-600">
                                                                Buyer: {comp.buyerName} · TXN: {comp.transactionId}
                                                            </p>
                                                        </div>
                                                        <span className={`px-2.5 py-1 text-xs font-medium rounded-full whitespace-nowrap ${statusBadge(comp.status)}`}>
                                                            <i className={`${statusIcon(comp.status)} mr-1`}></i>
                                                            {comp.status}
                                                        </span>
                                                    </div>
                                                    <div className="grid grid-cols-4 gap-4 mt-2">
                                                        <div>
                                                            <p className="text-xs text-gray-500">Sold Price</p>
                                                            <p className="text-sm font-semibold text-gray-900">${comp.soldPrice.toLocaleString()}</p>
                                                        </div>
                                                        <div>
                                                            <p className="text-xs text-gray-500">Compensation ({comp.compensationRate}%)</p>
                                                            <p className="text-sm font-semibold text-emerald-600">${comp.compensationAmount.toLocaleString()}</p>
                                                        </div>
                                                        <div>
                                                            <p className="text-xs text-gray-500">Sold Date</p>
                                                            <p className="text-sm text-gray-700">{new Date(comp.soldAt).toLocaleDateString()}</p>
                                                        </div>
                                                        <div>
                                                            <p className="text-xs text-gray-500">
                                                                {comp.status === 'Paid' && comp.paidAt ? 'Paid Date' : 'Status'}
                                                            </p>
                                                            <p className="text-sm text-gray-700">
                                                                {comp.status === 'Paid' && comp.paidAt
                                                                    ? new Date(comp.paidAt).toLocaleDateString()
                                                                    : comp.status === 'Requested' ? 'Awaiting review' : 'Awaiting payment'}
                                                            </p>
                                                            {comp.status === 'Requested' && (
                                                                <button
                                                                    onClick={() => openApproveModal(comp.id)}
                                                                    className="mt-1 text-xs text-teal-600 hover:text-teal-700 cursor-pointer flex items-center gap-1"
                                                                >
                                                                    <i className="ri-check-line text-xs"></i>Approve
                                                                </button>
                                                            )}
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>

            {/* Batch Process Payments Modal */}
            {processModalOpen && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-lg w-full p-6">
                        <div className="flex items-center justify-between mb-6">
                            <h2 className="text-lg font-semibold text-gray-900">Process Compensation Payments</h2>
                            <button
                                onClick={() => { setProcessModalOpen(false); setProcessError(''); setApprovalFile(null); setApprovalFileName(''); }}
                                className="w-8 h-8 flex items-center justify-center text-gray-400 hover:text-gray-600 cursor-pointer"
                            >
                                <i className="ri-close-line text-xl"></i>
                            </button>
                        </div>

                        <div className="mb-4">
                            <p className="text-sm text-gray-600 mb-3">
                                You are about to process <strong>{selectedProcessable.length}</strong> compensation{selectedProcessable.length !== 1 ? 's' : ''}:
                            </p>
                            <div className="bg-gray-50 rounded-lg p-3 space-y-1 max-h-40 overflow-y-auto">
                                {selectedProcessable.map(id => {
                                    const comp = compensations.find(c => c.id === id);
                                    if (!comp) return null;
                                    return (
                                        <div key={id} className="text-sm flex items-center justify-between">
                                            <span className="text-gray-900">{comp.lotName}</span>
                                            <span className="text-emerald-600 font-medium">${comp.compensationAmount.toLocaleString()}</span>
                                        </div>
                                    );
                                })}
                            </div>
                            <p className="text-sm font-semibold text-gray-900 mt-3">
                                Total: ${selectedProcessable.reduce((sum, id) => {
                                    const comp = compensations.find(c => c.id === id);
                                    return sum + (comp?.compensationAmount || 0);
                                }, 0).toLocaleString()}
                            </p>
                        </div>

                        <div className="mb-4">
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                Payment Approval Document (PDF) <span className="text-red-500">*</span>
                            </label>
                            <div className="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center hover:border-teal-400 transition-colors cursor-pointer">
                                <input
                                    type="file"
                                    accept=".pdf,application/pdf"
                                    onChange={handleFileSelect}
                                    className="hidden"
                                    id="approval-pdf-upload-batch"
                                />
                                <label htmlFor="approval-pdf-upload-batch" className="cursor-pointer">
                                    {approvalFileName ? (
                                        <div className="flex items-center justify-center gap-2 text-teal-600">
                                            <i className="ri-file-pdf-line text-2xl"></i>
                                            <span className="text-sm font-medium">{approvalFileName}</span>
                                        </div>
                                    ) : (
                                        <div>
                                            <i className="ri-upload-cloud-2-line text-3xl text-gray-400 mb-2 block"></i>
                                            <p className="text-sm text-gray-600 mb-1">Click to upload PDF approval document</p>
                                            <p className="text-xs text-gray-400">This confirms the compensation has been paid</p>
                                        </div>
                                    )}
                                </label>
                            </div>
                            {processError && (
                                <p className="text-xs text-red-500 mt-2 flex items-center gap-1">
                                    <i className="ri-error-warning-line"></i>
                                    {processError}
                                </p>
                            )}
                        </div>

                        <div className="flex items-center gap-3">
                            <button
                                onClick={() => { setProcessModalOpen(false); setProcessError(''); setApprovalFile(null); setApprovalFileName(''); }}
                                className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleProcessPayments}
                                className="flex-1 px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 transition-colors cursor-pointer whitespace-nowrap"
                            >
                                <i className="ri-check-double-line mr-2"></i>
                                Confirm & Process Payments
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Single Approve Modal */}
            {approveModalOpen && approvingId && (() => {
                const comp = compensations.find(c => c.id === approvingId);
                if (!comp) return null;
                return (
                    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                        <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
                            <h3 className="text-lg font-semibold text-gray-900 mb-4">Approve Compensation Request</h3>
                            <div className="bg-gray-50 rounded-lg p-4 mb-4">
                                <p className="text-sm font-medium text-gray-900">{comp.lotName}</p>
                                <p className="text-xs text-gray-600">Buyer: {comp.buyerName} · TXN: {comp.transactionId}</p>
                                <p className="text-sm font-semibold text-emerald-600 mt-2">${comp.compensationAmount.toLocaleString()}</p>
                            </div>
                            <div className="mb-4">
                                <label className="block text-sm font-medium text-gray-700 mb-2">
                                    Payment Approval Document (PDF) <span className="text-red-500">*</span>
                                </label>
                                <div className="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center hover:border-teal-400 transition-colors cursor-pointer">
                                    <input
                                        type="file"
                                        accept=".pdf,application/pdf"
                                        onChange={handleFileSelect}
                                        className="hidden"
                                        id="approval-pdf-upload-single"
                                    />
                                    <label htmlFor="approval-pdf-upload-single" className="cursor-pointer">
                                        {approvalFileName ? (
                                            <div className="flex items-center justify-center gap-2 text-teal-600">
                                                <i className="ri-file-pdf-line text-2xl"></i>
                                                <span className="text-sm font-medium">{approvalFileName}</span>
                                            </div>
                                        ) : (
                                            <div>
                                                <i className="ri-upload-cloud-2-line text-3xl text-gray-400 mb-2 block"></i>
                                                <p className="text-sm text-gray-600 mb-1">Click to upload PDF approval document</p>
                                                <p className="text-xs text-gray-400">This confirms the compensation has been paid</p>
                                            </div>
                                        )}
                                    </label>
                                </div>
                                {processError && (
                                    <p className="text-xs text-red-500 mt-2 flex items-center gap-1">
                                        <i className="ri-error-warning-line"></i>
                                        {processError}
                                    </p>
                                )}
                            </div>
                            <div className="flex items-center gap-3">
                                <button
                                    onClick={() => { setApproveModalOpen(false); setApprovingId(null); setApprovalFile(null); setApprovalFileName(''); setProcessError(''); }}
                                    className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap"
                                >
                                    Cancel
                                </button>
                                <button
                                    onClick={handleSingleApprove}
                                    className="flex-1 px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 transition-colors cursor-pointer whitespace-nowrap"
                                >
                                    <i className="ri-check-line mr-2"></i>Approve & Pay
                                </button>
                            </div>
                        </div>
                    </div>
                );
            })()}
        </div>
    );
}