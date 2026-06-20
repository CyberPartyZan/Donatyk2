import { useState } from 'react';
import { mockLots, mockCategories } from '../../../mocks/lots';
import LotCard from './components/LotCard';
import LotFormModal from './components/LotFormModal';
import Pagination from '@/components/base/Pagination';

const ITEMS_PER_PAGE = 6;

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
    goalId?: string;
    characteristics: Characteristic[];
    endOfAuction?: string;
    auctionStepPercent?: number;
    currentBid?: number;
    ticketPrice?: number;
    totalTickets?: number;
    ticketsSold?: number;
    createdAt: string;
    sellerId: string;
    denialReason?: string;
}

export default function LotsAdmin() {
    const [activeTab, setActiveTab] = useState<'my-lots' | 'pending'>('my-lots');
    const [lots, setLots] = useState<Lot[]>(mockLots);
    const [searchQuery, setSearchQuery] = useState('');
    const [activeSearchQuery, setActiveSearchQuery] = useState('');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingLot, setEditingLot] = useState<Lot | null>(null);
    const [filterType, setFilterType] = useState<'All' | 'Simple' | 'Auction' | 'Draw'>('All');
    const [filterStage, setFilterStage] = useState<'All' | 'Created' | 'Approved' | 'Denied'>('All');
    const [filterActive, setFilterActive] = useState<'All' | 'Active' | 'Inactive'>('All');
    const [currentPage, setCurrentPage] = useState(1);

    const myLots = lots.filter(lot => lot.stage !== 'PendingApproval');
    const pendingLots = lots.filter(lot => lot.stage === 'PendingApproval');

    const currentLots = activeTab === 'my-lots' ? myLots : pendingLots;

    const filteredLots = currentLots.filter(lot => {
        const matchesSearch = lot.name.toLowerCase().includes(activeSearchQuery.toLowerCase()) ||
            lot.description.toLowerCase().includes(activeSearchQuery.toLowerCase());
        const matchesType = filterType === 'All' || lot.type === filterType;
        const matchesStage = filterStage === 'All' || lot.stage === filterStage;
        const matchesActive = filterActive === 'All' ||
            (filterActive === 'Active' ? lot.isActive : !lot.isActive);
        return matchesSearch && matchesType && matchesStage && matchesActive;
    });

    const handleAddLot = () => {
        setEditingLot(null);
        setIsModalOpen(true);
    };

    const handleEditLot = (lot: Lot) => {
        setEditingLot(lot);
        setIsModalOpen(true);
    };

    const handleSaveLot = (lotData: Partial<Lot>) => {
        if (editingLot) {
            setLots(lots.map(lot =>
                lot.id === editingLot.id
                    ? {
                        ...lot,
                        ...lotData,
                        categoryName: mockCategories.find(c => c.id === lotData.categoryId)?.name || lot.categoryName
                    }
                    : lot
            ));
        } else {
            const newLot: Lot = {
                id: Date.now().toString(),
                ...lotData as Omit<Lot, 'id' | 'createdAt' | 'sellerId' | 'categoryName'>,
                categoryName: mockCategories.find(c => c.id === lotData.categoryId)?.name || '',
                createdAt: new Date().toISOString(),
                sellerId: 'seller-1',
                currentBid: lotData.type === 'Auction' ? 0 : undefined,
                ticketsSold: lotData.type === 'Draw' ? 0 : undefined
            };
            setLots([newLot, ...lots]);
        }
        setIsModalOpen(false);
        setEditingLot(null);
    };

    const handleDeleteLot = (id: string) => {
        const lot = lots.find(l => l.id === id);
        if (window.confirm(`Are you sure you want to delete "${lot?.name}"?`)) {
            setLots(lots.filter(l => l.id !== id));
        }
    };

    const handleApproveLot = (id: string) => {
        setLots(lots.map(lot =>
            lot.id === id
                ? { ...lot, stage: 'Approved' as const }
                : lot
        ));
    };

    const handleDeclineLot = (id: string) => {
        const reason = window.prompt('Enter reason for declining this lot:');
        if (reason) {
            setLots(lots.map(lot =>
                lot.id === id
                    ? { ...lot, stage: 'Denied' as const, denialReason: reason }
                    : lot
            ));
        }
    };

    const handleToggleActive = (id: string, isActive: boolean) => {
        setLots(lots.map(lot =>
            lot.id === id
                ? { ...lot, isActive }
                : lot
        ));
    };

    const stats = {
        total: myLots.length,
        approved: myLots.filter(l => l.stage === 'Approved').length,
        pending: pendingLots.length,
        active: myLots.filter(l => l.isActive).length
    };

    return (
        <div className="bg-white rounded-lg shadow-sm">
            <div className="border-b border-gray-200 px-6 py-4">
                <div className="flex items-center justify-between mb-4">
                    <h2 className="text-2xl font-semibold text-gray-800">Lots Management</h2>
                </div>

                <div className="grid grid-cols-4 gap-4 mb-4">
                    <div className="bg-blue-50 rounded-lg p-4">
                        <p className="text-sm text-blue-600 mb-1">Total Lots</p>
                        <p className="text-2xl font-bold text-blue-900">{stats.total}</p>
                    </div>
                    <div className="bg-green-50 rounded-lg p-4">
                        <p className="text-sm text-green-600 mb-1">Approved</p>
                        <p className="text-2xl font-bold text-green-900">{stats.approved}</p>
                    </div>
                    <div className="bg-yellow-50 rounded-lg p-4">
                        <p className="text-sm text-yellow-600 mb-1">Pending</p>
                        <p className="text-2xl font-bold text-yellow-900">{stats.pending}</p>
                    </div>
                    <div className="bg-teal-50 rounded-lg p-4">
                        <p className="text-sm text-teal-600 mb-1">Active</p>
                        <p className="text-2xl font-bold text-teal-900">{stats.active}</p>
                    </div>
                </div>

                <div className="flex items-center gap-3 mb-4">
                    <button
                        onClick={() => setActiveTab('my-lots')}
                        className={`px-4 py-2 text-sm font-medium rounded-lg transition-colors cursor-pointer whitespace-nowrap ${activeTab === 'my-lots'
                                ? 'bg-teal-600 text-white'
                                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                            }`}
                    >
                        Lots ({myLots.length})
                    </button>
                    <button
                        onClick={() => setActiveTab('pending')}
                        className={`px-4 py-2 text-sm font-medium rounded-lg transition-colors cursor-pointer whitespace-nowrap ${activeTab === 'pending'
                                ? 'bg-teal-600 text-white'
                                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                            }`}
                    >
                        Pending Approval ({pendingLots.length})
                    </button>
                </div>

                <div className="flex items-center gap-3">
                    <div className="flex-1 relative">
                        <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"></i>
                        <input
                            type="text"
                            placeholder="Search lots..."
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            onKeyDown={(e) => { if (e.key === 'Enter') { setActiveSearchQuery(searchQuery); } }}
                            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                        />
                    </div>

                    {activeTab === 'my-lots' && (
                        <>
                            <select
                                value={filterType}
                                onChange={(e) => setFilterType(e.target.value as any)}
                                className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                            >
                                <option value="All">All Types</option>
                                <option value="Simple">Simple</option>
                                <option value="Auction">Auction</option>
                                <option value="Draw">Draw</option>
                            </select>

                            <select
                                value={filterStage}
                                onChange={(e) => setFilterStage(e.target.value as any)}
                                className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                            >
                                <option value="All">All Stages</option>
                                <option value="Created">Created</option>
                                <option value="Approved">Approved</option>
                                <option value="Denied">Denied</option>
                            </select>

                            <select
                                value={filterActive}
                                onChange={(e) => setFilterActive(e.target.value as any)}
                                className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                            >
                                <option value="All">All Status</option>
                                <option value="Active">Active</option>
                                <option value="Inactive">Inactive</option>
                            </select>
                        </>
                    )}

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
                {filteredLots.length === 0 ? (
                    <div className="text-center py-12">
                        <i className="ri-inbox-line text-6xl text-gray-300 mb-4"></i>
                        <p className="text-gray-500">No lots found</p>
                    </div>
                ) : (
                    <>
                        <div className="grid gap-4">
                            {filteredLots.slice((currentPage - 1) * ITEMS_PER_PAGE, currentPage * ITEMS_PER_PAGE).map(lot => (
                                <LotCard
                                    key={lot.id}
                                    lot={lot}
                                    onEdit={handleEditLot}
                                    onDelete={handleDeleteLot}
                                    onApprove={activeTab === 'pending' ? handleApproveLot : undefined}
                                    onDecline={activeTab === 'pending' ? handleDeclineLot : undefined}
                                    onToggleActive={activeTab === 'my-lots' ? handleToggleActive : undefined}
                                />
                            ))}
                        </div>
                        <Pagination currentPage={currentPage} totalPages={Math.ceil(filteredLots.length / ITEMS_PER_PAGE)} onPageChange={(p) => setCurrentPage(p)} />
                    </>
                )}
            </div>

            <LotFormModal
                isOpen={isModalOpen}
                onClose={() => {
                    setIsModalOpen(false);
                    setEditingLot(null);
                }}
                onSave={handleSaveLot}
                lot={editingLot}
                categories={mockCategories}
            />
        </div>
    );
}