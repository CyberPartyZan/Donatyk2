import { useState } from 'react';
import { mockSeller } from '../../../mocks/seller';
import { mockLots, mockCategories } from '../../../mocks/lots';
import { mockCompensations } from '../../../mocks/compensations';
import LotCard from '../lots/components/LotCard';
import LotFormModal from '../lots/components/LotFormModal';

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

export default function SellerPage() {
    const [seller, setSeller] = useState<Seller | null>(mockSeller);
    const [isEditing, setIsEditing] = useState(false);
    const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
    const [showLotModal, setShowLotModal] = useState(false);
    const [editingLot, setEditingLot] = useState<Lot | null>(null);
    const [lots, setLots] = useState<Lot[]>(mockLots.filter(l => l.sellerId === 'seller-1'));
    const [lotSearch, setLotSearch] = useState('');
    const [lotTypeFilter, setLotTypeFilter] = useState<'All' | 'Simple' | 'Auction' | 'Draw'>('All');
    const [lotStageFilter, setLotStageFilter] = useState<'All' | 'Created' | 'PendingApproval' | 'Denied' | 'Approved'>('All');
    const [imagePreview, setImagePreview] = useState(seller?.avatarImage || '');
    const [deleteLotConfirm, setDeleteLotConfirm] = useState<string | null>(null);
    const [sellerTab, setSellerTab] = useState<'lots' | 'compensations'>('lots');
    const [showCreateForm, setShowCreateForm] = useState(false);
    const [showRequestModal, setShowRequestModal] = useState(false);
    const [compensationsList, setCompensationsList] = useState(mockCompensations);

    const [formData, setFormData] = useState({
        name: seller?.name || '',
        description: seller?.description || '',
        email: seller?.email || '',
        phoneNumber: seller?.phoneNumber || '',
        avatarImage: seller?.avatarImage || ''
    });

    const filteredLots = lots.filter(lot => {
        const matchesSearch =
            lot.name.toLowerCase().includes(lotSearch.toLowerCase()) ||
            lot.description.toLowerCase().includes(lotSearch.toLowerCase());
        const matchesType = lotTypeFilter === 'All' || lot.type === lotTypeFilter;
        const matchesStage = lotStageFilter === 'All' || lot.stage === lotStageFilter;
        return matchesSearch && matchesType && matchesStage;
    });

    const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            const reader = new FileReader();
            reader.onloadend = () => {
                const result = reader.result as string;
                setImagePreview(result);
                setFormData({ ...formData, avatarImage: result });
            };
            reader.readAsDataURL(file);
        }
    };

    const handleCreateSeller = (e: React.FormEvent) => {
        e.preventDefault();
        const newSeller: Seller = {
            id: 'seller-' + Date.now(),
            ...formData,
            createdAt: new Date().toISOString(),
            totalLots: 0,
            approvedLots: 0,
            totalSales: 0,
            rating: 0
        };
        setSeller(newSeller);
        setIsEditing(false);
    };

    const handleUpdateSeller = (e: React.FormEvent) => {
        e.preventDefault();
        if (seller) {
            setSeller({ ...seller, ...formData });
            setIsEditing(false);
        }
    };

    const handleDeleteSeller = () => {
        setSeller(null);
        setShowDeleteConfirm(false);
        setFormData({ name: '', description: '', email: '', phoneNumber: '', avatarImage: '' });
        setImagePreview('');
    };

    const handleEditClick = () => {
        if (seller) {
            setFormData({
                name: seller.name,
                description: seller.description,
                email: seller.email,
                phoneNumber: seller.phoneNumber,
                avatarImage: seller.avatarImage
            });
            setImagePreview(seller.avatarImage);
        }
        setIsEditing(true);
    };

    const handleCancelEdit = () => {
        setIsEditing(false);
        if (seller) {
            setFormData({
                name: seller.name,
                description: seller.description,
                email: seller.email,
                phoneNumber: seller.phoneNumber,
                avatarImage: seller.avatarImage
            });
            setImagePreview(seller.avatarImage);
        }
    };

    const handleOpenCreateLot = () => {
        setEditingLot(null);
        setShowLotModal(true);
    };

    const handleEditLot = (lot: Lot) => {
        setEditingLot(lot);
        setShowLotModal(true);
    };

    const handleSaveLot = (lotData: Partial<Lot>) => {
        if (editingLot) {
            setLots(lots.map(l =>
                l.id === editingLot.id
                    ? {
                        ...l,
                        ...lotData,
                        categoryName: mockCategories.find(c => c.id === lotData.categoryId)?.name || l.categoryName
                    }
                    : l
            ));
        } else {
            const newLot: Lot = {
                id: Date.now().toString(),
                ...(lotData as Omit<Lot, 'id' | 'createdAt' | 'sellerId' | 'categoryName'>),
                categoryName: mockCategories.find(c => c.id === lotData.categoryId)?.name || '',
                createdAt: new Date().toISOString(),
                sellerId: seller?.id || 'seller-1',
                currentBid: lotData.type === 'Auction' ? 0 : undefined,
                ticketsSold: lotData.type === 'Draw' ? 0 : undefined
            };
            setLots([newLot, ...lots]);
        }
        setShowLotModal(false);
        setEditingLot(null);
    };

    const handleDeleteLot = (id: string) => {
        setDeleteLotConfirm(id);
    };

    const confirmDeleteLot = () => {
        if (deleteLotConfirm) {
            setLots(lots.filter(l => l.id !== deleteLotConfirm));
            setDeleteLotConfirm(null);
        }
    };

    const handleToggleActive = (id: string, isActive: boolean) => {
        setLots(lots.map(l => l.id === id ? { ...l, isActive } : l));
    };

    const handleOpenRequestCompensation = () => {
        setShowRequestModal(true);
    };

    const handleSubmitCompensationRequest = (e: React.FormEvent) => {
        e.preventDefault();
        const newComp: any = {
            id: 'comp-req-newest',
            lotId: 'req-' + Date.now(),
            lotName: 'Compensation Request',
            lotImage: 'https://readdy.ai/api/search-image?query=vintage%20luxury%20product%20photography%20on%20clean%20white%20background&width=400&height=400&seq=comp-req-new&orientation=squarish',
            soldAt: new Date().toISOString().split('T')[0] + 'T12:00:00',
            soldPrice: 0,
            compensationAmount: 0,
            compensationRate: 0,
            buyerName: 'Request Submitted',
            status: 'Requested',
            paidAt: null,
            transactionId: 'TXN-REQ-84730'
        };
        setCompensationsList([newComp, ...compensationsList]);
        setShowRequestModal(false);
    };

    const lotStats = {
        total: lots.length,
        approved: lots.filter(l => l.stage === 'Approved').length,
        pending: lots.filter(l => l.stage === 'PendingApproval').length,
        active: lots.filter(l => l.isActive).length
    };

    if (!seller) {
        return (
            <div className="p-8">
                <div className="max-w-3xl mx-auto">
                    <div className="mb-6">
                        <h1 className="text-2xl font-bold text-gray-900">My Seller Info</h1>
                        <p className="text-sm text-gray-600 mt-1">You haven't created a seller account yet</p>
                    </div>

                    {!showCreateForm ? (
                        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-12 text-center">
                            <div className="w-20 h-20 bg-teal-100 rounded-full flex items-center justify-center mx-auto mb-6">
                                <i className="ri-store-2-line text-teal-600 text-3xl"></i>
                            </div>
                            <h2 className="text-lg font-semibold text-gray-900 mb-2">Become a Seller</h2>
                            <p className="text-sm text-gray-600 mb-6 max-w-md mx-auto">
                                Create your seller account to start listing items on the marketplace. You'll be able to manage your lots, track compensations, and contribute to charitable goals.
                            </p>
                            <button
                                onClick={() => setShowCreateForm(true)}
                                className="px-6 py-3 bg-teal-600 text-white text-sm font-semibold rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2 mx-auto"
                            >
                                <i className="ri-add-line"></i>
                                Create a Seller Account
                            </button>
                        </div>
                    ) : (
                        <div>
                            <div className="flex items-center justify-between mb-4">
                                <h2 className="text-lg font-semibold text-gray-900">Create Your Seller Profile</h2>
                                <button
                                    onClick={() => setShowCreateForm(false)}
                                    className="px-3 py-1.5 text-sm text-gray-600 hover:text-gray-900 cursor-pointer whitespace-nowrap"
                                >
                                    <i className="ri-arrow-left-line mr-1"></i>
                                    Back
                                </button>
                            </div>
                            <form onSubmit={handleCreateSeller} className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                                <div className="space-y-5">
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-2">Profile Picture</label>
                                        <div className="flex items-center gap-4">
                                            <div className="w-24 h-24 rounded-lg bg-gray-100 flex items-center justify-center overflow-hidden border-2 border-gray-200">
                                                {imagePreview ? (
                                                    <img src={imagePreview} alt="Preview" className="w-full h-full object-cover" />
                                                ) : (
                                                    <i className="ri-image-line text-3xl text-gray-400"></i>
                                                )}
                                            </div>
                                            <div>
                                                <label className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap inline-block">
                                                    <i className="ri-upload-2-line mr-2"></i>Upload Photo
                                                    <input type="file" accept="image/*" onChange={handleImageUpload} className="hidden" />
                                                </label>
                                                <p className="text-xs text-gray-500 mt-1">JPG, PNG or GIF. Max 5MB.</p>
                                            </div>
                                        </div>
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">Business Name</label>
                                        <input type="text" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" placeholder="Enter your business name" required />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                                        <textarea value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} rows={4} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" placeholder="Tell buyers about your business" required />
                                    </div>
                                    <div className="grid grid-cols-2 gap-4">
                                        <div>
                                            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                                            <input type="email" value={formData.email} onChange={(e) => setFormData({ ...formData, email: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" placeholder="contact@example.com" required />
                                        </div>
                                        <div>
                                            <label className="block text-sm font-medium text-gray-700 mb-1">Phone Number</label>
                                            <input type="tel" value={formData.phoneNumber} onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" placeholder="+1 (555) 000-0000" required />
                                        </div>
                                    </div>
                                </div>
                                <div className="flex items-center gap-3 mt-6 pt-6 border-t border-gray-200">
                                    <button type="submit" className="px-6 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap">
                                        <i className="ri-check-line mr-2"></i>Create Seller Account
                                    </button>
                                </div>
                            </form>
                        </div>
                    )}
                </div>
            </div>
        );
    }

    return (
        <div className="p-8">
            <div className="max-w-5xl mx-auto space-y-6">

                {/* Header */}
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">My Seller Info</h1>
                        <p className="text-sm text-gray-600 mt-1">Manage your seller profile and listings</p>
                    </div>
                    {!isEditing && (
                        <div className="flex items-center gap-3">
                            <button onClick={handleEditClick} className="px-4 py-2 bg-gray-100 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-200 transition-colors cursor-pointer whitespace-nowrap">
                                <i className="ri-edit-line mr-2"></i>Edit Profile
                            </button>
                            <button onClick={() => setShowDeleteConfirm(true)} className="px-4 py-2 bg-amber-50 text-amber-700 text-sm font-medium rounded-lg hover:bg-amber-100 transition-colors cursor-pointer whitespace-nowrap">
                                <i className="ri-pause-circle-line mr-2"></i>Suspend Account
                            </button>
                        </div>
                    )}
                </div>

                {/* Profile Card / Edit Form */}
                {isEditing ? (
                    <form onSubmit={handleUpdateSeller} className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                        <div className="space-y-5">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">Profile Picture</label>
                                <div className="flex items-center gap-4">
                                    <div className="w-24 h-24 rounded-lg bg-gray-100 flex items-center justify-center overflow-hidden border-2 border-gray-200">
                                        {imagePreview ? (
                                            <img src={imagePreview} alt="Preview" className="w-full h-full object-cover" />
                                        ) : (
                                            <i className="ri-image-line text-3xl text-gray-400"></i>
                                        )}
                                    </div>
                                    <div>
                                        <label className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap inline-block">
                                            <i className="ri-upload-2-line mr-2"></i>Change Photo
                                            <input type="file" accept="image/*" onChange={handleImageUpload} className="hidden" />
                                        </label>
                                        <p className="text-xs text-gray-500 mt-1">JPG, PNG or GIF. Max 5MB.</p>
                                    </div>
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Business Name</label>
                                <input type="text" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                                <textarea value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} rows={4} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                                    <input type="email" value={formData.email} onChange={(e) => setFormData({ ...formData, email: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Phone Number</label>
                                    <input type="tel" value={formData.phoneNumber} onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                                </div>
                            </div>
                        </div>
                        <div className="flex items-center gap-3 mt-6 pt-6 border-t border-gray-200">
                            <button type="submit" className="px-6 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap">
                                <i className="ri-check-line mr-2"></i>Save Changes
                            </button>
                            <button type="button" onClick={handleCancelEdit} className="px-6 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">
                                Cancel
                            </button>
                        </div>
                    </form>
                ) : (
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
                        <div className="p-6">
                            <div className="flex items-start gap-6">
                                <div className="w-32 h-32 rounded-lg bg-gray-100 flex items-center justify-center overflow-hidden border-2 border-gray-200 flex-shrink-0">
                                    <img src={seller.avatarImage} alt={seller.name} className="w-full h-full object-cover object-top" />
                                </div>
                                <div className="flex-1">
                                    <h2 className="text-xl font-bold text-gray-900 mb-1">{seller.name}</h2>
                                    <div className="flex items-center gap-2 mb-3">
                                        <i className="ri-star-fill text-amber-400 text-sm"></i>
                                        <span className="text-sm font-semibold text-gray-900">{seller.rating}</span>
                                        <span className="text-gray-300">•</span>
                                        <span className="text-sm text-gray-600">{seller.totalSales} sales</span>
                                    </div>
                                    <p className="text-sm text-gray-600 leading-relaxed mb-4">{seller.description}</p>
                                    <div className="grid grid-cols-2 gap-4">
                                        <div className="flex items-center gap-2 text-sm">
                                            <i className="ri-mail-line text-gray-400"></i>
                                            <span className="text-gray-700">{seller.email}</span>
                                        </div>
                                        <div className="flex items-center gap-2 text-sm">
                                            <i className="ri-phone-line text-gray-400"></i>
                                            <span className="text-gray-700">{seller.phoneNumber}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div className="grid grid-cols-4 border-t border-gray-200">
                            <div className="px-6 py-4 border-r border-gray-200">
                                <div className="text-2xl font-bold text-gray-900">{lotStats.total}</div>
                                <div className="text-xs text-gray-600 mt-1">Total Lots</div>
                            </div>
                            <div className="px-6 py-4 border-r border-gray-200">
                                <div className="text-2xl font-bold text-teal-600">{lotStats.approved}</div>
                                <div className="text-xs text-gray-600 mt-1">Approved</div>
                            </div>
                            <div className="px-6 py-4 border-r border-gray-200">
                                <div className="text-2xl font-bold text-yellow-500">{lotStats.pending}</div>
                                <div className="text-xs text-gray-600 mt-1">Pending</div>
                            </div>
                            <div className="px-6 py-4">
                                <div className="text-2xl font-bold text-amber-500">{seller.rating}</div>
                                <div className="text-xs text-gray-600 mt-1">Rating</div>
                            </div>
                        </div>
                    </div>
                )}

                {/* Request Compensation */}
                {!isEditing && (
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-5">
                        <div className="flex items-center justify-between">
                            <div className="flex items-center gap-4">
                                <div className="w-12 h-12 rounded-lg bg-orange-100 flex items-center justify-center">
                                    <i className="ri-money-dollar-circle-line text-orange-600 text-2xl"></i>
                                </div>
                                <div>
                                    <h3 className="text-base font-semibold text-gray-900">Request Compensation</h3>
                                    <p className="text-xs text-gray-500 mt-0.5">Submit a new compensation request for admin review</p>
                                </div>
                            </div>
                            <button
                                onClick={handleOpenRequestCompensation}
                                className="px-5 py-2.5 bg-orange-600 text-white text-sm font-semibold rounded-lg hover:bg-orange-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                            >
                                <i className="ri-add-line"></i>
                                Request Compensation
                            </button>
                        </div>
                    </div>
                )}

                {/* Lots & Compensations Section */}
                {!isEditing && (
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200">
                        <div className="px-6 py-4 border-b border-gray-200">
                            <div className="flex items-center justify-between mb-4">
                                <div className="flex items-center gap-2 bg-gray-100 p-1 rounded-full">
                                    <button
                                        onClick={() => setSellerTab('lots')}
                                        className={`px-4 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${sellerTab === 'lots' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'
                                            }`}
                                    >
                                        <i className="ri-auction-line mr-2"></i>My Lots
                                        <span className="ml-1.5 px-1.5 py-0.5 bg-teal-100 text-teal-700 text-xs rounded-full">{lots.length}</span>
                                    </button>
                                    <button
                                        onClick={() => setSellerTab('compensations')}
                                        className={`px-4 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${sellerTab === 'compensations' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'
                                            }`}
                                    >
                                        <i className="ri-money-dollar-circle-line mr-2"></i>Compensations
                                        <span className="ml-1.5 px-1.5 py-0.5 bg-emerald-100 text-emerald-700 text-xs rounded-full">{compensationsList.length}</span>
                                    </button>
                                </div>
                                <div className="flex items-center gap-3">
                                    {sellerTab === 'lots' && (
                                        <button
                                            onClick={handleOpenCreateLot}
                                            className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                                        >
                                            <i className="ri-add-line"></i>
                                            Add New Lot
                                        </button>
                                    )}
                                </div>
                            </div>

                            {sellerTab === 'lots' && (
                                <>
                                    {/* Filters */}
                                    <div className="flex items-center gap-3">
                                        <div className="flex-1 relative">
                                            <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                                            <input
                                                type="text"
                                                placeholder="Search lots..."
                                                value={lotSearch}
                                                onChange={(e) => setLotSearch(e.target.value)}
                                                className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                            />
                                        </div>
                                        <select
                                            value={lotTypeFilter}
                                            onChange={(e) => setLotTypeFilter(e.target.value as any)}
                                            className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                                        >
                                            <option value="All">All Types</option>
                                            <option value="Simple">Simple</option>
                                            <option value="Auction">Auction</option>
                                            <option value="Draw">Draw</option>
                                        </select>
                                        <select
                                            value={lotStageFilter}
                                            onChange={(e) => setLotStageFilter(e.target.value as any)}
                                            className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                                        >
                                            <option value="All">All Stages</option>
                                            <option value="Created">Created</option>
                                            <option value="PendingApproval">Pending</option>
                                            <option value="Approved">Approved</option>
                                            <option value="Denied">Denied</option>
                                        </select>
                                    </div>
                                </>
                            )}
                        </div>

                        <div className="p-6">
                            {sellerTab === 'lots' && (
                                <>
                                    {filteredLots.length === 0 ? (
                                        <div className="text-center py-12">
                                            <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                                                <i className="ri-inbox-line text-5xl text-gray-300"></i>
                                            </div>
                                            <p className="text-gray-500 text-sm mb-4">No lots found</p>
                                            <button
                                                onClick={handleOpenCreateLot}
                                                className="px-5 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                                            >
                                                <i className="ri-add-line mr-2"></i>Create Your First Lot
                                            </button>
                                        </div>
                                    ) : (
                                        <div className="grid gap-4">
                                            {filteredLots.map(lot => (
                                                <LotCard
                                                    key={lot.id}
                                                    lot={lot}
                                                    onEdit={handleEditLot}
                                                    onDelete={handleDeleteLot}
                                                    onToggleActive={handleToggleActive}
                                                />
                                            ))}
                                        </div>
                                    )}
                                </>
                            )}

                            {sellerTab === 'compensations' && (
                                <>
                                    {compensationsList.length === 0 ? (
                                        <div className="text-center py-12">
                                            <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                                                <i className="ri-money-dollar-circle-line text-5xl text-gray-300"></i>
                                            </div>
                                            <p className="text-gray-500 text-sm">No compensations received yet</p>
                                            <p className="text-xs text-gray-400 mt-1">Compensations appear here when your lots are sold</p>
                                        </div>
                                    ) : (
                                        <div className="grid gap-4">
                                            {compensationsList.map(comp => (
                                                <div key={comp.id} className="bg-white rounded-lg border border-gray-200 p-5 hover:border-emerald-200 transition-colors">
                                                    <div className="flex items-start gap-4">
                                                        <div className="w-20 h-20 rounded-lg bg-gray-100 overflow-hidden flex-shrink-0 border border-gray-200">
                                                            <img src={comp.lotImage} alt={comp.lotName} className="w-full h-full object-cover object-top" />
                                                        </div>
                                                        <div className="flex-1 min-w-0">
                                                            <div className="flex items-start justify-between mb-2">
                                                                <div>
                                                                    <h4 className="text-base font-semibold text-gray-900">{comp.lotName}</h4>
                                                                    <p className="text-sm text-gray-600">Sold to {comp.buyerName} • TXN: {comp.transactionId}</p>
                                                                </div>
                                                                <span className={`inline-flex items-center gap-1 px-2.5 py-1 text-xs font-medium rounded-full whitespace-nowrap ${comp.status === 'Paid' ? 'bg-emerald-100 text-emerald-700' : comp.status === 'Requested' ? 'bg-orange-100 text-orange-700' : 'bg-amber-100 text-amber-700'
                                                                    }`}>
                                                                    <i className={comp.status === 'Paid' ? 'ri-check-double-line' : comp.status === 'Requested' ? 'ri-question-answer-line' : 'ri-time-line'}></i>
                                                                    {comp.status}
                                                                </span>
                                                            </div>
                                                            <div className="grid grid-cols-3 gap-4 mt-3">
                                                                <div>
                                                                    <p className="text-xs text-gray-500 mb-0.5">Sold Price</p>
                                                                    <p className="text-sm font-semibold text-gray-900">${comp.soldPrice.toLocaleString()}</p>
                                                                </div>
                                                                <div>
                                                                    <p className="text-xs text-gray-500 mb-0.5">Compensation ({comp.compensationRate}%)</p>
                                                                    <p className="text-sm font-semibold text-emerald-600">${comp.compensationAmount.toLocaleString()}</p>
                                                                </div>
                                                                <div>
                                                                    <p className="text-xs text-gray-500 mb-0.5">Sold Date</p>
                                                                    <p className="text-sm text-gray-700">{new Date(comp.soldAt).toLocaleDateString()}</p>
                                                                </div>
                                                            </div>
                                                            {comp.paidAt && (
                                                                <p className="text-xs text-gray-400 mt-2 flex items-center gap-1">
                                                                    <i className="ri-bank-card-line"></i>
                                                                    Paid on {new Date(comp.paidAt).toLocaleDateString()}
                                                                </p>
                                                            )}
                                                        </div>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </>
                            )}
                        </div>
                    </div>
                )}
            </div>

            {/* Request Compensation Modal */}
            {showRequestModal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
                        <div className="w-12 h-12 rounded-full bg-orange-100 flex items-center justify-center mx-auto mb-4">
                            <i className="ri-money-dollar-circle-line text-orange-600 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900 text-center mb-2">Request Compensation</h3>
                        <p className="text-sm text-gray-600 text-center mb-6">
                            Are you sure you want to submit a compensation request? This will be reviewed by an admin before processing. Once submitted, the request cannot be modified.
                        </p>
                        <div className="flex items-center gap-3">
                            <button type="button" onClick={() => setShowRequestModal(false)} className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">
                                Cancel
                            </button>
                            <button onClick={handleSubmitCompensationRequest} className="flex-1 px-4 py-2 bg-orange-600 text-white text-sm font-medium rounded-lg hover:bg-orange-700 transition-colors cursor-pointer whitespace-nowrap">
                                <i className="ri-send-plane-line mr-2"></i>Submit Request
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Suspend Account Confirm */}
            {showDeleteConfirm && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
                        <div className="w-12 h-12 rounded-full bg-amber-100 flex items-center justify-center mx-auto mb-4">
                            <i className="ri-pause-circle-line text-amber-600 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900 text-center mb-2">Suspend Seller Account?</h3>
                        <p className="text-sm text-gray-600 text-center mb-6">
                            This will temporarily suspend your seller account. Your lots will be hidden from the marketplace until you reactivate. This action can be undone later.
                        </p>
                        <div className="flex items-center gap-3">
                            <button onClick={() => setShowDeleteConfirm(false)} className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">
                                Cancel
                            </button>
                            <button onClick={handleDeleteSeller} className="flex-1 px-4 py-2 bg-amber-600 text-white text-sm font-medium rounded-lg hover:bg-amber-700 transition-colors cursor-pointer whitespace-nowrap">
                                Suspend Account
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Delete Lot Confirm */}
            {deleteLotConfirm && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
                        <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center mx-auto mb-4">
                            <i className="ri-delete-bin-line text-red-600 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900 text-center mb-2">Delete Lot?</h3>
                        <p className="text-sm text-gray-600 text-center mb-6">
                            Are you sure you want to delete "<strong>{lots.find(l => l.id === deleteLotConfirm)?.name}</strong>"? This action cannot be undone.
                        </p>
                        <div className="flex items-center gap-3">
                            <button onClick={() => setDeleteLotConfirm(null)} className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">
                                Cancel
                            </button>
                            <button onClick={confirmDeleteLot} className="flex-1 px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors cursor-pointer whitespace-nowrap">
                                Delete Lot
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Lot Form Modal */}
            <LotFormModal
                isOpen={showLotModal}
                onClose={() => { setShowLotModal(false); setEditingLot(null); }}
                onSave={handleSaveLot}
                lot={editingLot}
                categories={mockCategories}
                hidePrice={true}
            />
        </div>
    );
}
