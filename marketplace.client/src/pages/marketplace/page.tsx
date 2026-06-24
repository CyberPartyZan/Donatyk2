import { useCallback, useEffect, useState } from 'react';
import MarketplaceHeader from './components/MarketplaceHeader';
import CategoryMenu from './components/CategoryMenu';
import FilterSidebar from './components/FilterSidebar';
import ProductGrid from './components/ProductGrid';

type LotTypeFilter = 'simple' | 'auction' | 'draw';

interface MarketplaceFilters {
    minPrice: string;
    maxPrice: string;
    minDiscount: string;
    maxDiscount: string;
    lotType: LotTypeFilter[];
}

interface MoneyDto {
    amount: number;
    currency: number | string;
}

interface ApiImageDto {
    url: string;
}

interface ApiLotDto {
    id: string;
    name: string;
    price: MoneyDto;
    discountedPrice: MoneyDto | null;
    discount: number;
    type: number | string;
    stage: number | string;
    isActive: boolean;
    createdAt: string;
    endOfAuction?: string | null;
    ticketPrice?: MoneyDto | null;
    images?: ApiImageDto[];
}

interface MarketplaceLot {
    id: string;
    name: string;
    price: number;
    discount: number;
    image: string;
    lotType: LotTypeFilter;
    createdAt: string;
    auctionEndsAt?: string;
    ticketPrice?: number;
    ticketsSold?: number;
}

const PAGE_SIZE = 12;

const defaultFilters: MarketplaceFilters = {
    minPrice: '',
    maxPrice: '',
    minDiscount: '',
    maxDiscount: '',
    lotType: [],
};

const mapLotType = (type: number | string): LotTypeFilter => {
    if (typeof type === 'string') {
        const normalized = type.toLowerCase();
        if (normalized === 'auction') return 'auction';
        if (normalized === 'draw') return 'draw';
        return 'simple';
    }

    if (type === 1) return 'auction';
    if (type === 2) return 'draw';
    return 'simple';
};

const isApprovedStage = (stage: number | string): boolean => {
    if (typeof stage === 'string') {
        return stage.toLowerCase() === 'approved';
    }

    return stage === 5;
};

export default function Marketplace() {
    const [showCategories, setShowCategories] = useState(false);
    const [filters, setFilters] = useState<MarketplaceFilters>(defaultFilters);
    const [appliedFilters, setAppliedFilters] = useState<MarketplaceFilters>(defaultFilters);
    const [searchText, setSearchText] = useState('');
    const [activeSearchText, setActiveSearchText] = useState('');
    const [sortBy, setSortBy] = useState('date');
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);

    const [lots, setLots] = useState<MarketplaceLot[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [requestVersion, setRequestVersion] = useState(0);

    const applySearch = () => {
        setActiveSearchText(searchText.trim());
        setAppliedFilters({ ...filters });
        setCurrentPage(1);
        setRequestVersion((v) => v + 1);
    };

    const fetchLots = useCallback(async () => {
        setIsLoading(true);
        setError(null);

        try {
            const params = new URLSearchParams();

            if (activeSearchText) params.set('searchText', activeSearchText);
            if (appliedFilters.minPrice) params.set('minPrice', appliedFilters.minPrice);
            if (appliedFilters.maxPrice) params.set('maxPrice', appliedFilters.maxPrice);
            if (appliedFilters.minDiscount) params.set('minDiscount', appliedFilters.minDiscount);
            if (appliedFilters.maxDiscount) params.set('maxDiscount', appliedFilters.maxDiscount);

            params.set('stage', 'Approved');
            params.set('pageNumber', String(currentPage));
            params.set('pageSize', String(PAGE_SIZE));

            const response = await fetch(`/api/lots?${params.toString()}`);
            if (!response.ok) {
                throw new Error(`Failed to load lots: ${response.status}`);
            }

            const data = (await response.json()) as ApiLotDto[];

            const mapped = data
                .filter((x) => x.isActive && isApprovedStage(x.stage))
                .map<MarketplaceLot>((x) => {
                    const price = Number(x.price?.amount ?? 0);
                    const discounted = x.discountedPrice ? Number(x.discountedPrice.amount) : price;
                    const effectiveDiscount = price > 0 ? ((price - discounted) / price) * 100 : 0;

                    return {
                        id: x.id,
                        name: x.name,
                        price,
                        discount: Number.isFinite(x.discount) && x.discount > 0 ? x.discount : effectiveDiscount,
                        image: x.images?.[0]?.url || 'https://placehold.co/600x600?text=No+Image',
                        lotType: mapLotType(x.type),
                        createdAt: x.createdAt,
                        auctionEndsAt: x.endOfAuction ?? undefined,
                        ticketPrice: x.ticketPrice ? Number(x.ticketPrice.amount) : undefined,
                        ticketsSold: 0,
                    };
                });

            setLots(mapped);

            const totalCountHeader = response.headers.get('X-Total-Count');
            const totalCount = totalCountHeader ? Number(totalCountHeader) : NaN;

            if (Number.isFinite(totalCount) && totalCount >= 0) {
                setTotalPages(Math.max(1, Math.ceil(totalCount / PAGE_SIZE)));
            } else {
                const hasNextPage = data.length === PAGE_SIZE;
                setTotalPages(hasNextPage ? currentPage + 1 : currentPage);
            }
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Unknown error');
            setLots([]);
            setTotalPages(1);
        } finally {
            setIsLoading(false);
        }
    }, [activeSearchText, appliedFilters, currentPage, requestVersion]);

    useEffect(() => {
        void fetchLots();
    }, [fetchLots]);

    return (
        <div className="min-h-screen bg-gray-50">
            <MarketplaceHeader
                onCategoriesClick={() => setShowCategories(!showCategories)}
                searchText={searchText}
                onSearchTextChange={setSearchText}
                onSearch={applySearch}
            />

            {showCategories && <CategoryMenu onClose={() => setShowCategories(false)} />}

            <div className="max-w-[1600px] mx-auto px-6 py-8">
                <div className="flex gap-8">
                    <FilterSidebar filters={filters} setFilters={setFilters} />

                    <div className="flex-1">
                        <div className="bg-white rounded-lg shadow-sm p-4 mb-6 flex items-center justify-between">
                            <h2 className="text-lg font-semibold text-gray-900">All Products</h2>
                            <div className="flex items-center gap-3">
                                <span className="text-sm text-gray-600 whitespace-nowrap">Sort by:</span>
                                <select
                                    value={sortBy}
                                    onChange={(e) => setSortBy(e.target.value)}
                                    className="px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 cursor-pointer"
                                >
                                    <option value="date">Date Created</option>
                                    <option value="price-low">Price: Low to High</option>
                                    <option value="price-high">Price: High to Low</option>
                                </select>
                            </div>
                        </div>

                        <ProductGrid
                            lots={lots}
                            filters={appliedFilters}
                            sortBy={sortBy}
                            isLoading={isLoading}
                            error={error}
                            currentPage={currentPage}
                            totalPages={totalPages}
                            onPageChange={setCurrentPage}
                        />
                    </div>
                </div>
            </div>
        </div>
    );
}
