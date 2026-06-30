import { useCallback, useEffect, useMemo, useState } from 'react';
import LotCard from '../lots/components/LotCard';
import LotFormModal from '../lots/components/LotFormModal';
import Pagination from '@/components/base/Pagination';

const LOTS_PER_PAGE = 5;
const COMPS_PER_PAGE = 5;
const ACCESS_TOKEN_KEY = 'auth_access_token';
const DEFAULT_CURRENCY = 1; // USD

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
    avatarBlob?: ApiBlobDto | null;
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

interface CompensationItem {
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
}

interface ApiBlobDto {
    id: string;
    filePath?: string;
    key?: string;
    fileName?: string;
}

interface ApiSellerDto {
    id: string;
    name: string;
    description: string;
    email: string;
    phoneNumber: string;
    key?: string | null;
}

interface ApiMoneyDto {
    amount: number;
    currency: number | string;
}

interface ApiLotDto {
    id: string;
    name: string;
    description: string;
    price: ApiMoneyDto;
    compensation: ApiMoneyDto;
    stockCount: number;
    discountedPrice: ApiMoneyDto | null;
    type: number | string;
    stage: number | string;
    declineReason?: string | null;
    isActive: boolean;
    createdAt: string;
    seller: { id: string };
    category: { id: string; name?: string; title?: string };
    endOfAuction?: string | null;
    auctionStepPercent?: number | null;
    ticketPrice?: ApiMoneyDto | null;
    characteristics?: Characteristic[];
}

interface ApiCompensationDto {
    id: string;
    orderId: string;
    lotId: string;
    amount: ApiMoneyDto;
    status: 'Paid' | 'Pending' | 'Requested' | string;
    soldPrice: number;
    soldDate: string;
    buyerName: string;
    lotName: string;
    lotImage: string;
}

interface LotStatisticsDto {
    total: number;
    approved: number;
    pending: number;
    active: number;
}

interface ApiCategoryDto {
    id: string;
    name?: string;
    title?: string;
    description?: string;
    parentId?: string | null;
    subCategories?: ApiCategoryDto[];
}

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

    const dedup = new Map<string, { id: string; name: string }>();
    for (const item of result) {
        if (item.id && item.name) {
            dedup.set(item.id, item);
        }
    }

    return Array.from(dedup.values());
};

const getAuthHeaders = (): Record<string, string> => {
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    return token ? { Authorization: `Bearer ${token}` } : {};
};

const toMoney = (amount: number, currency: number | string = DEFAULT_CURRENCY): ApiMoneyDto => ({
    amount: Number.isFinite(amount) ? amount : 0,
    currency,
});

const decodeSubFromToken = (): string | null => {
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    if (!token) return null;

    try {
        const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/'))) as { sub?: string };
        return payload.sub ?? null;
    } catch {
        return null;
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

// add near other helpers
const buildSellerAvatarUrl = (blobKey?: string | null): string =>
    blobKey ? `/api/seller/avatar/${encodeURIComponent(blobKey)}` : '';

export default function SellerPage() {
    const [seller, setSeller] = useState<Seller | null>(null);
    const [isEditing, setIsEditing] = useState(false);
    const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
    const [showLotModal, setShowLotModal] = useState(false);
    const [editingLot, setEditingLot] = useState<Lot | null>(null);
    const [lots, setLots] = useState<Lot[]>([]);
    const [lotSearch, setLotSearch] = useState('');
    const [activeLotSearch, setActiveLotSearch] = useState('');
    const [lotTypeFilter, setLotTypeFilter] = useState<'All' | 'Simple' | 'Auction' | 'Draw'>('All');
    const [lotStageFilter, setLotStageFilter] = useState<'All' | 'Created' | 'PendingApproval' | 'Denied' | 'Approved'>('All');
    const [activeLotTypeFilter, setActiveLotTypeFilter] = useState<'All' | 'Simple' | 'Auction' | 'Draw'>('All');
    const [activeLotStageFilter, setActiveLotStageFilter] = useState<'All' | 'Created' | 'PendingApproval' | 'Denied' | 'Approved'>('All');
    const [imagePreview, setImagePreview] = useState('');
    const [deleteLotConfirm, setDeleteLotConfirm] = useState<string | null>(null);
    const [sellerTab, setSellerTab] = useState<'lots' | 'compensations'>('lots');
    const [showCreateForm, setShowCreateForm] = useState(false);
    const [showRequestModal, setShowRequestModal] = useState(false);
    const [compensationsList, setCompensationsList] = useState<CompensationItem[]>([]);
    const [categories, setCategories] = useState<Array<{ id: string; name: string }>>([]);

    const [lotsPage, setLotsPage] = useState(1);
    const [compsPage, setCompsPage] = useState(1);

    const [isLoading, setIsLoading] = useState(false);
    const [apiError, setApiError] = useState('');
    const [lotStats, setLotStats] = useState<LotStatisticsDto>({
        total: 0,
        approved: 0,
        pending: 0,
        active: 0
    });

    const [formData, setFormData] = useState({
        name: '',
        description: '',
        email: '',
        phoneNumber: '',
        avatarImage: '',
        avatar: null as ApiBlobDto | null
    });

    useEffect(() => {
        if (!seller) return;

        setFormData({
            name: seller.name,
            description: seller.description,
            email: seller.email,
            phoneNumber: seller.phoneNumber,
            avatarImage: seller.avatarImage,
            avatar: seller.avatarBlob ?? null
        });

        setImagePreview(seller.avatarImage || '');
    }, [seller]);

    const handleLotSearch = () => {
        setActiveLotSearch(lotSearch.trim());
        setActiveLotTypeFilter(lotTypeFilter);
        setActiveLotStageFilter(lotStageFilter);
        setLotsPage(1);
    };

    const filteredLots = useMemo(() => lots, [lots]);

    const lotsTotalPages = Math.max(1, Math.ceil(filteredLots.length / LOTS_PER_PAGE));
    const pagedLots = filteredLots.slice((lotsPage - 1) * LOTS_PER_PAGE, lotsPage * LOTS_PER_PAGE);

    const compsTotalPages = Math.max(1, Math.ceil(compensationsList.length / COMPS_PER_PAGE));
    const pagedComps = compensationsList.slice((compsPage - 1) * COMPS_PER_PAGE, compsPage * COMPS_PER_PAGE);

    const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        setImagePreview(URL.createObjectURL(file));

        const data = new FormData();
        data.append('file', file);

        const response = await fetch('/api/sellers/avatar', {
            method: 'POST',
            credentials: 'include',
            headers: { ...getAuthHeaders() },
            body: data
        });

        if (!response.ok) {
            setApiError(`Failed to upload avatar (${response.status})`);
            return;
        }

        const blob = (await response.json()) as ApiBlobDto;
        setFormData(prev => ({ ...prev, avatar: blob }));
        setImagePreview(buildSellerAvatarUrl(blob.key) || URL.createObjectURL(file));
    };

    const loadCurrentSeller = useCallback(async (): Promise<Seller | null> => {
        const sub = decodeSubFromToken();
        if (!sub) return null;

        const sellerResponse = await fetch(`/api/sellers/by-user/${encodeURIComponent(sub)}`, {
            method: 'GET',
            credentials: 'include',
            headers: { ...getAuthHeaders() }
        });

        if (sellerResponse.status === 404) return null;
        if (!sellerResponse.ok) return null;

        const current = (await sellerResponse.json()) as ApiSellerDto;
        const avatarKey = current.key ?? null;

        return {
            id: current.id,
            name: current.name ?? '',
            description: current.description ?? '',
            email: current.email ?? '',
            phoneNumber: current.phoneNumber ?? '',
            avatarImage: buildSellerAvatarUrl(avatarKey),
            avatarBlob: null,
            createdAt: new Date().toISOString(),
            totalLots: 0,
            approvedLots: 0,
            totalSales: 0,
            rating: 0
        };
    }, []);

    const loadCategories = useCallback(async () => {
        try {
            const response = await fetch('/api/categories', {
                method: 'GET',
                credentials: 'include'
            });

            if (!response.ok) {
                setCategories([]);
                return;
            }

            const payload = (await response.json()) as ApiCategoryDto[];
            const normalized = flattenCategories(Array.isArray(payload) ? payload : []);
            setCategories(normalized);
        } catch {
            setCategories([]);
        }
    }, []);

    const loadLots = useCallback(async (sellerId: string) => {
        const params = new URLSearchParams();
        params.set('sellerId', sellerId);
        params.set('pageNumber', '1');
        params.set('pageSize', '500');

        if (activeLotSearch) params.set('searchText', activeLotSearch);
        if (activeLotTypeFilter !== 'All') params.set('type', activeLotTypeFilter);
        if (activeLotStageFilter !== 'All') params.set('stage', activeLotStageFilter);

        const response = await fetch(`/api/lots?${params.toString()}`, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) throw new Error(`Failed to load lots (${response.status})`);

        const payload = (await response.json()) as ApiLotDto[];
        const mapped = (payload ?? []).map(dto => {
            const type = mapLotType(dto.type);
            const price = Number(dto.price?.amount ?? 0);
            const ticketPrice = dto.ticketPrice ? Number(dto.ticketPrice.amount) : undefined;

            return {
                id: dto.id,
                name: dto.name ?? '',
                description: dto.description ?? '',
                price,
                compensation: Number(dto.compensation?.amount ?? 0),
                stockCount: Number(dto.stockCount ?? 0),
                discountedPrice: dto.discountedPrice ? Number(dto.discountedPrice.amount) : null,
                type,
                stage: mapLotStage(dto.stage),
                isActive: !!dto.isActive,
                categoryId: dto.category?.id ?? '',
                categoryName: dto.category?.name ?? dto.category?.title ?? '',
                characteristics: dto.characteristics ?? [],
                endOfAuction: dto.endOfAuction ?? undefined,
                auctionStepPercent: dto.auctionStepPercent ?? undefined,
                currentBid: undefined,
                ticketPrice,
                totalTickets: type === 'Draw' && ticketPrice && ticketPrice > 0 ? Math.floor(price / ticketPrice) : undefined,
                ticketsSold: 0,
                createdAt: dto.createdAt ?? new Date().toISOString(),
                sellerId: dto.seller?.id ?? sellerId,
                denialReason: dto.declineReason ?? undefined
            } satisfies Lot;
        });

        setLots(mapped);
    }, [activeLotSearch, activeLotTypeFilter, activeLotStageFilter]);

    const loadLotStats = useCallback(async (sellerId: string) => {
        const response = await fetch(`/api/lots/statistics?sellerId=${sellerId}`, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            setLotStats({ total: 0, approved: 0, pending: 0, active: 0 });
            return;
        }

        const payload = (await response.json()) as LotStatisticsDto;
        setLotStats({
            total: payload.total ?? 0,
            approved: payload.approved ?? 0,
            pending: payload.pending ?? 0,
            active: payload.active ?? 0
        });
    }, []);

    const loadCompensations = useCallback(async (sellerId: string) => {
        const response = await fetch(`/api/compensation/seller/${sellerId}`, {
            method: 'GET',
            credentials: 'include',
            headers: { ...getAuthHeaders() }
        });

        if (!response.ok) {
            setCompensationsList([]);
            return;
        }

        const payload = (await response.json()) as ApiCompensationDto[];
        const mapped = (payload ?? []).map(c => {
            const soldPrice = Number(c.soldPrice ?? 0);
            const compensationAmount = Number(c.amount?.amount ?? 0);
            const status = String(c.status).toLowerCase() === 'paid'
                ? 'Paid'
                : String(c.status).toLowerCase() === 'requested'
                    ? 'Requested'
                    : 'Pending';

            return {
                id: c.id,
                lotId: c.lotId,
                lotName: c.lotName || 'Unknown lot',
                lotImage: c.lotImage || 'https://placehold.co/120x120?text=No+Image',
                soldAt: c.soldDate,
                soldPrice,
                compensationAmount,
                compensationRate: soldPrice > 0 ? Math.round((compensationAmount / soldPrice) * 100) : 0,
                buyerName: c.buyerName || 'Unknown buyer',
                status,
                paidAt: null,
                transactionId: c.orderId
            } satisfies CompensationItem;
        });

        setCompensationsList(mapped);
    }, []);

    const loadPageData = useCallback(async () => {
        setIsLoading(true);
        setApiError('');

        try {
            await loadCategories();

            const currentSeller = await loadCurrentSeller();
            setSeller(currentSeller);

            if (!currentSeller) {
                setLots([]);
                setCompensationsList([]);
                setLotStats({ total: 0, approved: 0, pending: 0, active: 0 });
                return;
            }

            await Promise.all([
                loadLots(currentSeller.id),
                loadLotStats(currentSeller.id),
                loadCompensations(currentSeller.id)
            ]);
        } catch (e) {
            setApiError(e instanceof Error ? e.message : 'Failed to load seller page.');
        } finally {
            setIsLoading(false);
        }
    }, [loadCategories, loadCompensations, loadCurrentSeller, loadLots, loadLotStats]);

    useEffect(() => {
        void loadPageData();
    }, [loadPageData]);

    const handleCreateSeller = async (e: React.FormEvent) => {
        e.preventDefault();
        setApiError('');

        try {
            const response = await fetch('/api/sellers', {
                method: 'POST',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json',
                    ...getAuthHeaders()
                },
                body: JSON.stringify({
                    id: '00000000-0000-0000-0000-000000000000',
                    name: formData.name,
                    description: formData.description,
                    email: formData.email,
                    phoneNumber: formData.phoneNumber,
                    avatar: formData.avatar
                })
            });

            if (!response.ok) {
                throw new Error(`Failed to create seller (${response.status})`);
            }

            setShowCreateForm(false);
            await loadPageData();
        } catch (e) {
            setApiError(e instanceof Error ? e.message : 'Failed to create seller.');
        }
    };

    const handleUpdateSeller = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!seller) return;

        setApiError('');

        try {
            const response = await fetch(`/api/sellers/${seller.id}`, {
                method: 'PUT',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json',
                    ...getAuthHeaders()
                },
                body: JSON.stringify({
                    id: seller.id,
                    name: formData.name,
                    description: formData.description,
                    email: formData.email,
                    phoneNumber: formData.phoneNumber,
                    avatar: formData.avatar
                })
            });

            if (!response.ok) {
                throw new Error(`Failed to update seller (${response.status})`);
            }

            setIsEditing(false);
            await loadPageData();
        } catch (e) {
            setApiError(e instanceof Error ? e.message : 'Failed to update seller.');
        }
    };

    const handleDeleteSeller = async () => {
        if (!seller) return;
        setApiError('');

        try {
            const response = await fetch(`/api/sellers/${seller.id}`, {
                method: 'DELETE',
                credentials: 'include',
                headers: { ...getAuthHeaders() }
            });

            if (!response.ok) {
                throw new Error(`Failed to suspend seller account (${response.status})`);
            }

            setSeller(null);
            setShowDeleteConfirm(false);
            setFormData({ name: '', description: '', email: '', phoneNumber: '', avatarImage: '', avatar: null });
            setImagePreview('');
            setLots([]);
            setCompensationsList([]);
            setLotStats({ total: 0, approved: 0, pending: 0, active: 0 });
        } catch (e) {
            setApiError(e instanceof Error ? e.message : 'Failed to suspend seller.');
        }
    };

    const handleEditClick = () => {
        if (!seller) return;

        setFormData({
            name: seller.name,
            description: seller.description,
            email: seller.email,
            phoneNumber: seller.phoneNumber,
            avatarImage: seller.avatarImage,
            avatar: seller.avatarBlob ?? null
        });
        setImagePreview(seller.avatarImage);
        setIsEditing(true);
    };

    const handleCancelEdit = () => {
        setIsEditing(false);

        if (!seller) return;

        setFormData({
            name: seller.name,
            description: seller.description,
            email: seller.email,
            phoneNumber: seller.phoneNumber,
            avatarImage: seller.avatarImage,
            avatar: seller.avatarBlob ?? null
        });
        setImagePreview(seller.avatarImage);
    };

    const handleOpenCreateLot = () => {
        setEditingLot(null);
        setShowLotModal(true);
    };

    const handleEditLot = (lot: Lot) => {
        setEditingLot(lot);
        setShowLotModal(true);
    };

    const buildLotPayload = (lotData: Partial<Lot>, baseLot?: Lot) => {
        if (!seller) throw new Error('Seller profile not found.');

        const merged = { ...baseLot, ...lotData };
        const resolvedCategoryId = merged.categoryId || categories[0]?.id || '';
        const resolvedCategoryName = categories.find(c => c.id === resolvedCategoryId)?.name
            || baseLot?.categoryName
            || 'Category';

        return {
            id: merged.id ?? '00000000-0000-0000-0000-000000000000',
            name: merged.name ?? '',
            description: merged.description ?? '',
            price: toMoney(Number(merged.price ?? 0)),
            compensation: toMoney(Number(merged.compensation ?? 0)),
            stockCount: Number(merged.stockCount ?? 1),
            discountedPrice: merged.discountedPrice == null ? null : toMoney(Number(merged.discountedPrice)),
            discount: 0,
            type: merged.type ?? 'Simple',
            stage: merged.stage ?? 'Created',
            declineReason: null,
            seller: {
                id: seller.id,
                name: seller.name,
                description: seller.description,
                email: seller.email,
                phoneNumber: seller.phoneNumber,
                avatarImageUrl: seller.avatarImage
            },
            category: {
                id: resolvedCategoryId,
                name: resolvedCategoryName,
                description: ''
            },
            isActive: merged.isActive ?? true,
            isCompensationPaid: false,
            createdAt: merged.createdAt ?? new Date().toISOString(),
            endOfAuction: merged.type === 'Auction' ? (merged.endOfAuction ?? null) : null,
            auctionStepPercent: merged.type === 'Auction' ? (merged.auctionStepPercent ?? 5) : null,
            ticketPrice: merged.type === 'Draw' && merged.ticketPrice != null ? toMoney(Number(merged.ticketPrice)) : null,
            characteristics: merged.characteristics ?? [],
            images: []
        };
    };

    const handleSaveLot = async (lotData: Partial<Lot>) => {
        setApiError('');

        try {
            if (editingLot) {
                const payload = buildLotPayload(lotData, editingLot);
                const response = await fetch(`/api/lots/${editingLot.id}`, {
                    method: 'PUT',
                    credentials: 'include',
                    headers: {
                        'Content-Type': 'application/json',
                        ...getAuthHeaders()
                    },
                    body: JSON.stringify(payload)
                });

                if (!response.ok) throw new Error(`Failed to update lot (${response.status})`);
            } else {
                const payload = buildLotPayload(lotData);
                const response = await fetch('/api/lots', {
                    method: 'POST',
                    credentials: 'include',
                    headers: {
                        'Content-Type': 'application/json',
                        ...getAuthHeaders()
                    },
                    body: JSON.stringify(payload)
                });

                if (!response.ok) throw new Error(`Failed to create lot (${response.status})`);
            }

            setShowLotModal(false);
            setEditingLot(null);
            await loadPageData();
        } catch (e) {
            setApiError(e instanceof Error ? e.message : 'Failed to save lot.');
        }
    };

    const handleDeleteLot = (id: string) => {
        setDeleteLotConfirm(id);
    };

    const confirmDeleteLot = async () => {
        if (!deleteLotConfirm) return;
        setApiError('');

        try {
            const response = await fetch(`/api/lots/${deleteLotConfirm}`, {
                method: 'DELETE',
                credentials: 'include',
                headers: { ...getAuthHeaders() }
            });

            if (!response.ok) throw new Error(`Failed to delete lot (${response.status})`);

            setDeleteLotConfirm(null);
            await loadPageData();
        } catch (e) {
            setApiError(e instanceof Error ? e.message : 'Failed to delete lot.');
        }
    };

    const handleToggleActive = async (id: string, isActive: boolean) => {
        const existing = lots.find(l => l.id === id);
        if (!existing) return;

        setApiError('');

        try {
            const payload = buildLotPayload({ ...existing, isActive }, existing);
            const response = await fetch(`/api/lots/${id}`, {
                method: 'PUT',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json',
                    ...getAuthHeaders()
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) throw new Error(`Failed to update lot status (${response.status})`);
            await loadPageData();
        } catch (e) {
            setApiError(e instanceof Error ? e.message : 'Failed to change lot status.');
        }
    };

    const handleOpenRequestCompensation = () => {
        setShowRequestModal(true);
    };

    const handleSubmitCompensationRequest = async () => {
        if (!seller) return;
        setApiError('');

        try {
            const response = await fetch(`/api/compensation/request/${seller.id}`, {
                method: 'POST',
                credentials: 'include',
                headers: { ...getAuthHeaders() }
            });

            if (!response.ok) throw new Error(`Failed to submit compensation request (${response.status})`);

            setShowRequestModal(false);
            await loadPageData();
        } catch (e) {
            setApiError(e instanceof Error ? e.message : 'Failed to request compensation.');
        }
    };

    if (!seller) {
        return (
            <div className="p-8">
                <div className="max-w-3xl mx-auto">
                    <div className="mb-6">
                        <h1 className="text-2xl font-bold text-gray-900">My Seller Info</h1>
                        <p className="text-sm text-gray-600 mt-1">You haven&apos;t created a seller account yet</p>
                    </div>

                    {apiError && (
                        <div className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                            {apiError}
                        </div>
                    )}

                    {isLoading ? (
                        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-12 text-center text-gray-600">
                            Loading...
                        </div>
                    ) : !showCreateForm ? (
                        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-12 text-center">
                            <div className="w-20 h-20 bg-teal-100 rounded-full flex items-center justify-center mx-auto mb-6">
                                <i className="ri-store-2-line text-teal-600 text-3xl"></i>
                            </div>
                            <h2 className="text-lg font-semibold text-gray-900 mb-2">Become a Seller</h2>
                            <p className="text-sm text-gray-600 mb-6 max-w-md mx-auto">
                                Create your seller account to start listing items on the marketplace.
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
                                            </div>
                                        </div>
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">Business Name</label>
                                        <input type="text" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm" required />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                                        <textarea value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} rows={4} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm" required />
                                    </div>
                                    <div className="grid grid-cols-2 gap-4">
                                        <div>
                                            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                                            <input type="email" value={formData.email} onChange={(e) => setFormData({ ...formData, email: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm" required />
                                        </div>
                                        <div>
                                            <label className="block text-sm font-medium text-gray-700 mb-1">Phone Number</label>
                                            <input type="tel" value={formData.phoneNumber} onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm" required />
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

                {apiError && (
                    <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                        {apiError}
                    </div>
                )}

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
                                    </div>
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Business Name</label>
                                <input type="text" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                                <textarea value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} rows={4} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm" required />
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                                    <input type="email" value={formData.email} onChange={(e) => setFormData({ ...formData, email: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm" required />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Phone Number</label>
                                    <input type="tel" value={formData.phoneNumber} onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm" required />
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
                                <div className="text-2xl font-bold text-emerald-600">{lotStats.active}</div>
                                <div className="text-xs text-gray-600 mt-1">Active</div>
                            </div>
                        </div>
                    </div>
                )}

                {!isEditing && (
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-5">
                        <div className="flex items-center justify-between">
                            <div className="flex items-center gap-4">
                                <div className="w-12 h-12 rounded-lg bg-orange-100 flex items-center justify-center">
                                    <i className="ri-money-dollar-circle-line text-orange-600 text-2xl"></i>
                                </div>
                                <div>
                                    <h3 className="text-base font-semibold text-gray-900">Request Compensation</h3>
                                    <p className="text-xs text-gray-500 mt-0.5">Submit pending compensations for admin review</p>
                                </div>
                            </div>
                            <button
                                onClick={handleOpenRequestCompensation}
                                className="px-4 py-2.5 bg-orange-600 text-white text-sm font-semibold rounded-lg hover:bg-orange-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                            >
                                <i className="ri-add-line"></i>
                                Request Compensation
                            </button>
                        </div>
                    </div>
                )}

                {!isEditing && (
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200">
                        <div className="px-6 py-4 border-b border-gray-200">
                            <div className="flex items-center justify-between mb-4">
                                <div className="flex items-center gap-2 bg-gray-100 p-1 rounded-full">
                                    <button
                                        onClick={() => setSellerTab('lots')}
                                        className={`px-4 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${sellerTab === 'lots' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'}`}
                                    >
                                        <i className="ri-auction-line mr-2"></i>My Lots
                                        <span className="ml-1.5 px-1.5 py-0.5 bg-teal-100 text-teal-700 text-xs rounded-full">{lots.length}</span>
                                    </button>
                                    <button
                                        onClick={() => setSellerTab('compensations')}
                                        className={`px-4 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${sellerTab === 'compensations' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'}`}
                                    >
                                        <i className="ri-money-dollar-circle-line mr-2"></i>Compensations
                                        <span className="ml-1.5 px-1.5 py-0.5 bg-emerald-100 text-emerald-700 text-xs rounded-full">{compensationsList.length}</span>
                                    </button>
                                </div>
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

                            {sellerTab === 'lots' && (
                                <div className="flex items-center gap-3">
                                    <div className="relative flex-1">
                                        <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                                        <input
                                            type="text"
                                            placeholder="Search lots..."
                                            value={lotSearch}
                                            onChange={(e) => setLotSearch(e.target.value)}
                                            onKeyDown={(e) => e.key === 'Enter' && handleLotSearch()}
                                            className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-md text-sm"
                                        />
                                    </div>
                                    <select
                                        value={lotTypeFilter}
                                        onChange={(e) => setLotTypeFilter(e.target.value as 'All' | 'Simple' | 'Auction' | 'Draw')}
                                        className="px-3 py-2 border border-gray-300 rounded-lg text-sm cursor-pointer"
                                    >
                                        <option value="All">All Types</option>
                                        <option value="Simple">Simple</option>
                                        <option value="Auction">Auction</option>
                                        <option value="Draw">Draw</option>
                                    </select>
                                    <select
                                        value={lotStageFilter}
                                        onChange={(e) => setLotStageFilter(e.target.value as 'All' | 'Created' | 'PendingApproval' | 'Denied' | 'Approved')}
                                        className="px-3 py-2 border border-gray-300 rounded-lg text-sm cursor-pointer"
                                    >
                                        <option value="All">All Stages</option>
                                        <option value="Created">Created</option>
                                        <option value="PendingApproval">Pending</option>
                                        <option value="Approved">Approved</option>
                                        <option value="Denied">Denied</option>
                                    </select>
                                    <button
                                        onClick={handleLotSearch}
                                        className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                                    >
                                        <i className="ri-search-line mr-1.5"></i>Search
                                    </button>
                                </div>
                            )}
                        </div>

                        <div className="p-6">
                            {isLoading ? (
                                <div className="text-center py-12 text-sm text-gray-500">Loading data...</div>
                            ) : sellerTab === 'lots' ? (
                                filteredLots.length === 0 ? (
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
                                    <>
                                        <div className="grid gap-4">
                                            {pagedLots.map(lot => (
                                                <LotCard
                                                    key={lot.id}
                                                    lot={lot}
                                                    onEdit={handleEditLot}
                                                    onDelete={handleDeleteLot}
                                                    onToggleActive={handleToggleActive}
                                                />
                                            ))}
                                        </div>
                                        <Pagination currentPage={lotsPage} totalPages={lotsTotalPages} onPageChange={(p) => setLotsPage(p)} />
                                    </>
                                )
                            ) : compensationsList.length === 0 ? (
                                <div className="text-center py-12">
                                    <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                                        <i className="ri-money-dollar-circle-line text-5xl text-gray-300"></i>
                                    </div>
                                    <p className="text-gray-500 text-sm">No compensations found</p>
                                </div>
                            ) : (
                                <>
                                    <div className="grid gap-4">
                                        {pagedComps.map(comp => (
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
                                                            <span className={`inline-flex items-center gap-1 px-2.5 py-1 text-xs font-medium rounded-full whitespace-nowrap ${comp.status === 'Paid' ? 'bg-emerald-100 text-emerald-700' : comp.status === 'Requested' ? 'bg-orange-100 text-orange-700' : 'bg-amber-100 text-amber-700'}`}>
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
                                                    </div>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                    <Pagination currentPage={compsPage} totalPages={compsTotalPages} onPageChange={(p) => setCompsPage(p)} />
                                </>
                            )}
                        </div>
                    </div>
                )}
            </div>

            {showRequestModal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
                        <div className="w-12 h-12 rounded-full bg-orange-100 flex items-center justify-center mx-auto mb-4">
                            <i className="ri-money-Dollar-circle-line text-orange-600 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900 text-center mb-2">Request Compensation</h3>
                        <p className="text-sm text-gray-600 text-center mb-6">
                            Submit compensation request for your pending compensations?
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

            {showDeleteConfirm && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
                        <div className="w-12 h-12 rounded-full bg-amber-100 flex items-center justify-center mx-auto mb-4">
                            <i className="ri-pause-circle-line text-amber-600 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900 text-center mb-2">Suspend Seller Account?</h3>
                        <p className="text-sm text-gray-600 text-center mb-6">
                            This will suspend your seller account and hide your lots.
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

            {deleteLotConfirm && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
                        <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center mx-auto mb-4">
                            <i className="ri-delete-bin-line text-red-600 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900 text-center mb-2">Delete Lot?</h3>
                        <p className="text-sm text-gray-600 text-center mb-6">
                            Are you sure you want to delete "<strong>{lots.find(l => l.id === deleteLotConfirm)?.name}</strong>"?
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

            <LotFormModal
                isOpen={showLotModal}
                onClose={() => { setShowLotModal(false); setEditingLot(null); }}
                onSave={handleSaveLot}
                lot={editingLot}
                categories={categories}
                hidePrice={true}
            />
        </div>
    );
}