import { useCallback, useEffect, useMemo, useState } from 'react';
import LotCard from './components/LotCard';
import LotFormModal from './components/LotFormModal';
import DeleteLotModal from './components/DeleteLotModal';
import DeclineLotModal from './components/DeclineLotModal';
import ApproveLotModal from './components/ApproveLotModal';
import Pagination from '@/components/base/Pagination';

const ITEMS_PER_PAGE = 6;
const ACCESS_TOKEN_KEY = 'auth_access_token';
const DEFAULT_CURRENCY = 1; // USD

interface Characteristic {
    key: string;
    value: string;
}

interface MoneyDto {
    amount: number;
    currency: number | string;
}

interface ApiImageDto {
    id?: string;
    url: string;
    data?: number[] | null;
}

interface ApiSellerDto {
    id: string;
    name: string;
    description: string;
    email: string;
    phoneNumber: string;
    avatarImageUrl: string;
}

interface ApiCategoryDto {
    id: string;
    name?: string;
    title?: string;
    description?: string;
    parentId?: string | null;
    subCategories?: ApiCategoryDto[];
}

interface ApiLotDto {
    id: string;
    name: string;
    description: string;
    price: MoneyDto;
    compensation: MoneyDto;
    stockCount: number;
    discountedPrice: MoneyDto | null;
    discount: number;
    type: number | string;
    stage: number | string;
    declineReason?: string | null;
    seller: ApiSellerDto;
    category: ApiCategoryDto;
    isActive: boolean;
    isCompensationPaid: boolean;
    createdAt: string;
    endOfAuction?: string | null;
    auctionStepPercent?: number | null;
    ticketPrice?: MoneyDto | null;
    characteristics?: Characteristic[];
    images?: ApiImageDto[];
}

interface ApiLotMutationDto {
    id?: string;
    name: string;
    description: string;
    price: MoneyDto;
    compensation: MoneyDto;
    stockCount: number;
    discountedPrice: MoneyDto | null;
    discount: number;
    type: 'Simple' | 'Auction' | 'Draw';
    stage: 'Created' | 'PendingApproval' | 'Denied' | 'Approved';
    declineReason?: string | null;
    seller: ApiSellerDto;
    category: {
        id: string;
        name: string;
        description: string;
        parentId?: string | null;
    };
    isActive: boolean;
    isCompensationPaid: boolean;
    createdAt: string;
    endOfAuction?: string | null;
    auctionStepPercent?: number | null;
    ticketPrice?: MoneyDto | null;
    characteristics: Characteristic[];
    images: ApiImageDto[];
}

interface Lot {
    id: string;
    name: string;
    description: string;
    price: number;
    priceCurrency: number | string;
    compensation: number;
    compensationCurrency: number | string;
    stockCount: number;
    discountedPrice: number | null;
    discount: number;
    type: 'Simple' | 'Auction' | 'Draw';
    stage: 'Created' | 'PendingApproval' | 'Denied' | 'Approved';
    isActive: boolean;
    categoryId: string;
    categoryName: string;
    categoryDescription: string;
    characteristics: Characteristic[];
    images: string[];
    endOfAuction?: string;
    auctionStepPercent?: number;
    currentBid?: number;
    ticketPrice?: number;
    ticketPriceCurrency?: number | string;
    totalTickets?: number;
    ticketsSold?: number;
    createdAt: string;
    sellerId: string;
    seller: ApiSellerDto;
    isCompensationPaid: boolean;
    denialReason?: string;
}

const getAuthHeader = (): Record<string, string> => {
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    return token ? { Authorization: `Bearer ${token}` } : {};
};

const getResponseMessage = async (response: Response): Promise<string> => {
    try {
        const payload = await response.json() as { message?: string; title?: string; detail?: string };
        return payload.message ?? payload.detail ?? payload.title ?? `Request failed: ${response.status}`;
    } catch {
        return `Request failed: ${response.status}`;
    }
};

const mapLotType = (type: number | string): Lot['type'] => {
    if (typeof type === 'string') {
        const normalized = type.toLowerCase();
        if (normalized === 'auction') return 'Auction';
        if (normalized === 'draw') return 'Draw';
        return 'Simple';
    }

    if (type === 1) return 'Auction';
    if (type === 2) return 'Draw';
    return 'Simple';
};

const mapLotStage = (stage: number | string): Lot['stage'] => {
    if (typeof stage === 'string') {
        const normalized = stage.toLowerCase();
        if (normalized === 'pendingapproval') return 'PendingApproval';
        if (normalized === 'denied') return 'Denied';
        if (normalized === 'approved') return 'Approved';
        return 'Created';
    }

    if (stage === 1) return 'PendingApproval';
    if (stage === 2) return 'Denied';
    if (stage === 5) return 'Approved';
    return 'Created';
};

const flattenCategories = (apiCategories: ApiCategoryDto[]): Array<{ id: string; name: string }> => {
    const result: Array<{ id: string; name: string }> = [];

    const walk = (nodes: ApiCategoryDto[]) => {
        for (const node of nodes) {
            result.push({
                id: node.id,
                name: (node.name ?? node.title ?? '').trim(),
            });

            if (node.subCategories?.length) {
                walk(node.subCategories);
            }
        }
    };

    walk(apiCategories);

    const map = new Map<string, { id: string; name: string }>();
    for (const item of result) {
        if (item.id && item.name) {
            map.set(item.id, item);
        }
    }

    return Array.from(map.values());
};

const toMoney = (amount: number, currency: number | string): MoneyDto => ({
    amount: Number.isFinite(amount) ? amount : 0,
    currency: currency ?? DEFAULT_CURRENCY,
});

const mapApiLot = (dto: ApiLotDto): Lot => {
    const type = mapLotType(dto.type);
    const stage = mapLotStage(dto.stage);
    const price = Number(dto.price?.amount ?? 0);
    const compensation = Number(dto.compensation?.amount ?? 0);
    const discountedPrice = dto.discountedPrice ? Number(dto.discountedPrice.amount) : null;
    const ticketPrice = dto.ticketPrice ? Number(dto.ticketPrice.amount) : undefined;
    const totalTickets = type === 'Draw' && ticketPrice && ticketPrice > 0
        ? Math.floor(price / ticketPrice)
        : undefined;

    return {
        id: dto.id,
        name: dto.name,
        description: dto.description,
        price,
        priceCurrency: dto.price?.currency ?? DEFAULT_CURRENCY,
        compensation,
        compensationCurrency: dto.compensation?.currency ?? DEFAULT_CURRENCY,
        stockCount: dto.stockCount,
        discountedPrice,
        discount: Number(dto.discount ?? 0),
        type,
        stage,
        isActive: !!dto.isActive,
        categoryId: dto.category?.id ?? '',
        categoryName: dto.category?.name ?? dto.category?.title ?? '',
        categoryDescription: dto.category?.description ?? '',
        characteristics: dto.characteristics ?? [],
        images: (dto.images ?? []).map(i => i.url).filter(Boolean),
        endOfAuction: dto.endOfAuction ?? undefined,
        auctionStepPercent: dto.auctionStepPercent ?? undefined,
        currentBid: undefined,
        ticketPrice,
        ticketPriceCurrency: dto.ticketPrice?.currency ?? DEFAULT_CURRENCY,
        totalTickets,
        ticketsSold: 0,
        createdAt: dto.createdAt,
        sellerId: dto.seller?.id ?? '',
        seller: {
            id: dto.seller?.id ?? '',
            name: dto.seller?.name ?? '',
            description: dto.seller?.description ?? '',
            email: dto.seller?.email ?? '',
            phoneNumber: dto.seller?.phoneNumber ?? '',
            avatarImageUrl: dto.seller?.avatarImageUrl ?? '',
        },
        isCompensationPaid: !!dto.isCompensationPaid,
        denialReason: dto.declineReason ?? undefined,
    };
};

const toApiMutation = (lot: Lot): ApiLotMutationDto => ({
    id: lot.id,
    name: lot.name,
    description: lot.description,
    price: toMoney(lot.price, lot.priceCurrency),
    compensation: toMoney(lot.compensation, lot.compensationCurrency),
    stockCount: lot.stockCount,
    discountedPrice: lot.discountedPrice == null ? null : toMoney(lot.discountedPrice, lot.priceCurrency),
    discount: lot.discount ?? 0,
    type: lot.type,
    stage: lot.stage,
    declineReason: lot.denialReason ?? null,
    seller: lot.seller,
    category: {
        id: lot.categoryId,
        name: lot.categoryName,
        description: lot.categoryDescription ?? '',
        parentId: null,
    },
    isActive: lot.isActive,
    isCompensationPaid: lot.isCompensationPaid,
    createdAt: lot.createdAt,
    endOfAuction: lot.endOfAuction ?? null,
    auctionStepPercent: lot.auctionStepPercent ?? null,
    ticketPrice: lot.type === 'Draw' && lot.ticketPrice != null
        ? toMoney(lot.ticketPrice, lot.ticketPriceCurrency ?? lot.priceCurrency)
        : null,
    characteristics: lot.characteristics ?? [],
    images: (lot.images ?? []).map((url) => ({ url })),
});

export default function LotsAdmin() {
    const [activeTab, setActiveTab] = useState<'lots' | 'pending'>('lots');
    const [lots, setLots] = useState<Lot[]>([]);
    const [pendingLots, setPendingLots] = useState<Lot[]>([]);
    const [categories, setCategories] = useState<Array<{ id: string; name: string }>>([]);
    const [lotsTotalCount, setLotsTotalCount] = useState(0);
    const [pendingTotalCount, setPendingTotalCount] = useState(0);

    const [searchQuery, setSearchQuery] = useState('');
    const [activeSearchQuery, setActiveSearchQuery] = useState('');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingLot, setEditingLot] = useState<Lot | null>(null);
    const [filterType, setFilterType] = useState<'All' | 'Simple' | 'Auction' | 'Draw'>('All');
    const [filterStage, setFilterStage] = useState<'All' | 'Created' | 'Approved' | 'Denied'>('All');
    const [filterActive, setFilterActive] = useState<'All' | 'Active' | 'Inactive'>('All');
    const [currentPage, setCurrentPage] = useState(1);
    const [deleteLotId, setDeleteLotId] = useState<string | null>(null);
    const [declineLotId, setDeclineLotId] = useState<string | null>(null);
    const [approveLotId, setApproveLotId] = useState<string | null>(null);

    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [mutationError, setMutationError] = useState<string | null>(null);

    const loadCategories = useCallback(async () => {
        try {
            const response = await fetch('/api/categories');
            if (!response.ok) return;
            const data = (await response.json()) as ApiCategoryDto[];
            const normalized = flattenCategories(Array.isArray(data) ? data : []);
            setCategories(normalized);
        } catch {
            setCategories([]);
        }
    }, []);

    const loadLots = useCallback(async () => {
        setIsLoading(true);
        setError(null);

        try {
            const lotsParams = new URLSearchParams();
            const pendingParams = new URLSearchParams();

            if (activeSearchQuery) {
                lotsParams.set('searchText', activeSearchQuery);
                pendingParams.set('searchText', activeSearchQuery);
            }

            if (filterType !== 'All') {
                lotsParams.set('type', filterType);
                pendingParams.set('type', filterType);
            }

            if (filterStage !== 'All') {
                lotsParams.set('stage', filterStage);
            }

            lotsParams.set('pageNumber', '1');
            lotsParams.set('pageSize', '200');

            pendingParams.set('stage', 'PendingApproval');
            pendingParams.set('pageNumber', '1');
            pendingParams.set('pageSize', '200');

            const [lotsResponse, pendingResponse] = await Promise.all([
                fetch(`/api/lots?${lotsParams.toString()}`),
                fetch(`/api/lots?${pendingParams.toString()}`),
            ]);

            if (!lotsResponse.ok) {
                throw new Error(await getResponseMessage(lotsResponse));
            }

            if (!pendingResponse.ok) {
                throw new Error(await getResponseMessage(pendingResponse));
            }

            const [lotsPayload, pendingPayload] = await Promise.all([
                lotsResponse.json() as Promise<ApiLotDto[]>,
                pendingResponse.json() as Promise<ApiLotDto[]>,
            ]);

            const lotsCountRaw = Number(lotsResponse.headers.get('X-Total-Count') ?? 0);
            const pendingCountRaw = Number(pendingResponse.headers.get('X-Total-Count') ?? 0);

            let mappedLots = (Array.isArray(lotsPayload) ? lotsPayload : [])
                .map(mapApiLot)
                .filter((lot) => filterStage !== 'All' ? true : lot.stage !== 'PendingApproval');

            if (filterActive === 'Active') {
                mappedLots = mappedLots.filter((lot) => lot.isActive);
            } else if (filterActive === 'Inactive') {
                mappedLots = mappedLots.filter((lot) => !lot.isActive);
            }

            const mappedPendingLots = (Array.isArray(pendingPayload) ? pendingPayload : [])
                .map(mapApiLot)
                .filter((lot) => lot.stage === 'PendingApproval');

            setLots(mappedLots);
            setPendingLots(mappedPendingLots);

            // if stage is All, lots tab excludes pending in UI so adjust total similarly
            const adjustedLotsTotal = filterStage === 'All'
                ? Math.max(0, lotsCountRaw - pendingCountRaw)
                : lotsCountRaw;

            setLotsTotalCount(adjustedLotsTotal);
            setPendingTotalCount(pendingCountRaw);
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load lots');
            setLots([]);
            setPendingLots([]);
        } finally {
            setIsLoading(false);
        }
    }, [activeSearchQuery, filterActive, filterStage, filterType]);

    useEffect(() => {
        void loadCategories();
    }, [loadCategories]);

    useEffect(() => {
        void loadLots();
    }, [loadLots]);

    useEffect(() => {
        setCurrentPage(1);
    }, [activeTab, activeSearchQuery, filterType, filterStage, filterActive]);

    const currentLots = activeTab === 'lots' ? lots : pendingLots;
    const totalItemsForTab = activeTab === 'lots' ? lotsTotalCount : pendingTotalCount;
    const totalPages = Math.max(1, Math.ceil(totalItemsForTab / ITEMS_PER_PAGE));

    useEffect(() => {
        if (currentPage > totalPages) {
            setCurrentPage(totalPages);
        }
    }, [currentPage, totalPages]);

    const handleEditLot = (lot: Lot) => {
        setEditingLot(lot);
        setIsModalOpen(true);
    };

    const updateLot = useCallback(async (lot: Lot): Promise<boolean> => {
        const payload = toApiMutation(lot);

        const response = await fetch(`/api/lots/${lot.id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                ...getAuthHeader(),
            },
            credentials: 'include',
            body: JSON.stringify(payload),
        });

        if (!response.ok) {
            setMutationError(await getResponseMessage(response));
            return false;
        }

        return true;
    }, []);

    const handleSaveLot = async (lotData: Partial<Lot>) => {
        setMutationError(null);

        if (!editingLot) {
            setMutationError('Create flow is not available on this admin page.');
            return;
        }

        const resolvedCategory = categories.find((c) => c.id === (lotData.categoryId ?? editingLot.categoryId));

        const mergedLot: Lot = {
            ...editingLot,
            ...lotData,
            categoryName: resolvedCategory?.name ?? editingLot.categoryName,
            categoryDescription: editingLot.categoryDescription ?? '',
            characteristics: lotData.characteristics ?? editingLot.characteristics,
            images: lotData.images ?? editingLot.images,
            denialReason: editingLot.denialReason,
        };

        const success = await updateLot(mergedLot);
        if (!success) return;

        await loadLots();
        setIsModalOpen(false);
        setEditingLot(null);
    };

    const handleDeleteLot = (id: string) => {
        setDeleteLotId(id);
    };

    const confirmDeleteLot = async () => {
        if (!deleteLotId) return;
        setMutationError(null);

        try {
            const response = await fetch(`/api/lots/${deleteLotId}`, {
                method: 'DELETE',
                headers: {
                    ...getAuthHeader(),
                },
                credentials: 'include',
            });

            if (!response.ok) {
                setMutationError(await getResponseMessage(response));
                return;
            }

            setDeleteLotId(null);
            await loadLots();
        } catch (e) {
            setMutationError(e instanceof Error ? e.message : 'Failed to delete lot');
        }
    };

    // approve flow
    const handleApproveLot = (id: string) => {
        setApproveLotId(id);
    };

    const confirmApproveLot = async () => {
        if (!approveLotId) return;
        setMutationError(null);

        try {
            const response = await fetch(`/api/lots/${approveLotId}/approve`, {
                method: 'POST',
                headers: {
                    ...getAuthHeader(),
                },
                credentials: 'include',
            });

            if (!response.ok) {
                setMutationError(await getResponseMessage(response));
                return;
            }

            setApproveLotId(null);
            await loadLots();
        } catch (e) {
            setMutationError(e instanceof Error ? e.message : 'Failed to approve lot');
        }
    };

    const handleDeclineLot = (id: string) => {
        setDeclineLotId(id);
    };

    const confirmDeclineLot = async (reason: string) => {
        if (!declineLotId) return;
        setMutationError(null);

        try {
            const response = await fetch(`/api/lots/${declineLotId}/decline`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...getAuthHeader(),
                },
                credentials: 'include',
                body: JSON.stringify({ reason }),
            });

            if (!response.ok) {
                setMutationError(await getResponseMessage(response));
                return;
            }

            setDeclineLotId(null);
            await loadLots();
        } catch (e) {
            setMutationError(e instanceof Error ? e.message : 'Failed to decline lot');
        }
    };

    const handleToggleActive = async (id: string, isActive: boolean) => {
        setMutationError(null);
        const target = lots.find((lot) => lot.id === id);
        if (!target) return;

        const success = await updateLot({ ...target, isActive });
        if (!success) return;

        await loadLots();
    };

    // stats
    const stats = {
        total: lotsTotalCount,
        approved: lots.filter((l) => l.stage === 'Approved').length,
        pending: pendingTotalCount,
        active: lots.filter((l) => l.isActive).length,
    };

    return (
        <div className="bg-white rounded-lg shadow-sm">
            <div className="border-b border-gray-200 px-6 py-4">
                <div className="flex items-center justify-between mb-4">
                    <h2 className="text-2xl font-semibold text-gray-800">Lots Management</h2>
                </div>

                {error && (
                    <div className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                        {error}
                    </div>
                )}

                {mutationError && (
                    <div className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                        {mutationError}
                    </div>
                )}

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
                        onClick={() => setActiveTab('lots')}
                        className={`px-4 py-2 text-sm font-medium rounded-lg transition-colors cursor-pointer whitespace-nowrap ${activeTab === 'lots'
                            ? 'bg-teal-600 text-white'
                            : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                            }`}
                    >
                        Lots ({lotsTotalCount})
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
                            onKeyDown={(e) => {
                                if (e.key === 'Enter') {
                                    setActiveSearchQuery(searchQuery.trim());
                                }
                            }}
                            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                        />
                    </div>

                    {activeTab === 'lots' && (
                        <>
                            <select
                                value={filterType}
                                onChange={(e) => setFilterType(e.target.value as 'All' | 'Simple' | 'Auction' | 'Draw')}
                                className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                            >
                                <option value="All">All Types</option>
                                <option value="Simple">Simple</option>
                                <option value="Auction">Auction</option>
                                <option value="Draw">Draw</option>
                            </select>

                            <select
                                value={filterStage}
                                onChange={(e) => setFilterStage(e.target.value as 'All' | 'Created' | 'Approved' | 'Denied')}
                                className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                            >
                                <option value="All">All Stages</option>
                                <option value="Created">Created</option>
                                <option value="Approved">Approved</option>
                                <option value="Denied">Denied</option>
                            </select>

                            <select
                                value={filterActive}
                                onChange={(e) => setFilterActive(e.target.value as 'All' | 'Active' | 'Inactive')}
                                className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                            >
                                <option value="All">All Status</option>
                                <option value="Active">Active</option>
                                <option value="Inactive">Inactive</option>
                            </select>
                        </>
                    )}

                    <button
                        onClick={() => setActiveSearchQuery(searchQuery.trim())}
                        className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                    >
                        <i className="ri-search-line"></i>
                        Search
                    </button>
                </div>
            </div>

            <div className="p-6">
                {isLoading ? (
                    <div className="text-center py-12">
                        <i className="ri-loader-4-line text-4xl text-gray-300 animate-spin mb-4"></i>
                        <p className="text-gray-500">Loading lots...</p>
                    </div>
                ) : currentLots.length === 0 ? (
                    <div className="text-center py-12">
                        <i className="ri-inbox-line text-6xl text-gray-300 mb-4"></i>
                        <p className="text-gray-500">No lots found</p>
                    </div>
                ) : (
                    <>
                        <div className="grid gap-4">
                            {currentLots.map((lot) => (
                                <LotCard
                                    key={lot.id}
                                    lot={lot}
                                    onEdit={handleEditLot}
                                    onDelete={handleDeleteLot}
                                    onApprove={activeTab === 'pending' ? handleApproveLot : undefined}
                                    onDecline={activeTab === 'pending' ? handleDeclineLot : undefined}
                                    onToggleActive={activeTab === 'lots' ? handleToggleActive : undefined}
                                />
                            ))}
                        </div>
                        <Pagination
                            currentPage={currentPage}
                            totalPages={totalPages}
                            onPageChange={(p) => setCurrentPage(p)}
                        />
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
                categories={categories}
            />

            <DeleteLotModal
                isOpen={deleteLotId !== null}
                lotName={(lots.find((l) => l.id === deleteLotId) ?? pendingLots.find((l) => l.id === deleteLotId))?.name || ''}
                onConfirm={confirmDeleteLot}
                onCancel={() => setDeleteLotId(null)}
            />

            <DeclineLotModal
                isOpen={declineLotId !== null}
                lotName={pendingLots.find((l) => l.id === declineLotId)?.name || ''}
                onConfirm={confirmDeclineLot}
                onCancel={() => setDeclineLotId(null)}
            />

            <ApproveLotModal
                isOpen={approveLotId !== null}
                lotName={pendingLots.find((l) => l.id === approveLotId)?.name || ''}
                onConfirm={confirmApproveLot}
                onCancel={() => setApproveLotId(null)}
            />
        </div>
    );
}