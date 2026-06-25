import { useCallback, useEffect, useMemo, useState } from 'react';
import SellerCard from './components/SellerCard';
import SellerDetailModal from './components/SellerDetailModal';
import EditSellerModal from './components/EditSellerModal';
import Pagination from '@/components/base/Pagination';
import { deleteSeller, getSellers, updateSeller, type SellerApiDto } from '@/api/sellers';

const ITEMS_PER_PAGE = 6;

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

const mapApiSeller = (dto: SellerApiDto): Seller => ({
    id: dto.id,
    name: dto.name ?? '',
    description: dto.description ?? '',
    email: dto.email ?? '',
    phoneNumber: dto.phoneNumber ?? '',
    avatarImage: dto.avatarImageUrl ?? '',
    createdAt: new Date().toISOString(),
    totalLots: 0,
    approvedLots: 0,
    totalSales: 0,
    rating: 0,
});

const toApiSeller = (seller: Seller): SellerApiDto => ({
    id: seller.id,
    name: seller.name,
    description: seller.description,
    email: seller.email,
    phoneNumber: seller.phoneNumber,
    avatarImageUrl: seller.avatarImage,
});

export default function SellersAdmin() {
    const [sellers, setSellers] = useState<Seller[]>([]);
    const [searchQuery, setSearchQuery] = useState('');
    const [activeSearchQuery, setActiveSearchQuery] = useState('');
    const [selectedSeller, setSelectedSeller] = useState<Seller | null>(null);
    const [editingSeller, setEditingSeller] = useState<Seller | null>(null);
    const [currentPage, setCurrentPage] = useState(1);
    const [isLoading, setIsLoading] = useState(false);
    const [isSaving, setIsSaving] = useState(false);
    const [deletingSellerId, setDeletingSellerId] = useState<string | null>(null);
    const [error, setError] = useState<string>('');

    const fetchSellers = useCallback(async () => {
        setIsLoading(true);
        setError('');

        try {
            const payload = await getSellers({
                search: activeSearchQuery,
                page: 1,
                pageSize: 500,
            });

            setSellers((payload ?? []).map(mapApiSeller));
        } catch (e) {
            const message = e instanceof Error ? e.message : 'Failed to load sellers.';
            setError(message);
            setSellers([]);
        } finally {
            setIsLoading(false);
        }
    }, [activeSearchQuery]);

    useEffect(() => {
        void fetchSellers();
    }, [fetchSellers]);

    useEffect(() => {
        setCurrentPage(1);
    }, [activeSearchQuery]);

    const handleSearch = () => {
        setActiveSearchQuery(searchQuery.trim());
    };

    const filteredSellers = useMemo(() => sellers, [sellers]);

    const totalPages = Math.ceil(filteredSellers.length / ITEMS_PER_PAGE);
    const pagedSellers = filteredSellers.slice(
        (currentPage - 1) * ITEMS_PER_PAGE,
        currentPage * ITEMS_PER_PAGE
    );

    useEffect(() => {
        if (totalPages > 0 && currentPage > totalPages) {
            setCurrentPage(totalPages);
        }
    }, [currentPage, totalPages]);

    const handleDelete = async (id: string) => {
        setDeletingSellerId(id);
        setError('');

        try {
            await deleteSeller(id);
            setSellers(prev => prev.filter(s => s.id !== id));
            setSelectedSeller(prev => (prev?.id === id ? null : prev));
            setEditingSeller(prev => (prev?.id === id ? null : prev));
        } catch (e) {
            const message = e instanceof Error ? e.message : 'Failed to delete seller.';
            setError(message);
            throw e;
        } finally {
            setDeletingSellerId(null);
        }
    };

    const handleSave = async (updatedSeller: Seller) => {
        setIsSaving(true);
        setError('');

        try {
            await updateSeller(updatedSeller.id, toApiSeller(updatedSeller));
            setSellers(prev => prev.map(s => (s.id === updatedSeller.id ? updatedSeller : s)));
            setSelectedSeller(prev => (prev?.id === updatedSeller.id ? updatedSeller : prev));
            setEditingSeller(null);
        } catch (e) {
            const message = e instanceof Error ? e.message : 'Failed to update seller.';
            setError(message);
            throw e;
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <div className="bg-white rounded-lg shadow-sm">
            <div className="p-6 border-b border-gray-200">
                <div className="flex items-center justify-between mb-4">
                    <div>
                        <h2 className="text-2xl font-semibold text-gray-900">Sellers Management</h2>
                        <p className="text-sm text-gray-600 mt-1">Manage all seller accounts and their information</p>
                    </div>
                    <div className="flex items-center gap-2 px-4 py-2 bg-teal-50 rounded-lg">
                        <i className="ri-store-2-line text-teal-600"></i>
                        <span className="text-sm font-medium text-teal-900">{sellers.length} Total Sellers</span>
                    </div>
                </div>

                <div className="flex items-center gap-3">
                    <div className="relative flex-1">
                        <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                        <input
                            type="text"
                            placeholder="Search sellers by name or email..."
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                            className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                        />
                    </div>
                    <button
                        onClick={handleSearch}
                        className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                    >
                        <i className="ri-search-line mr-1.5"></i>Search
                    </button>
                </div>

                {error && (
                    <div className="mt-4 px-4 py-3 rounded-md bg-red-50 text-red-700 text-sm border border-red-200">
                        {error}
                    </div>
                )}
            </div>

            <div className="p-6">
                {isLoading ? (
                    <div className="text-center py-12 text-gray-600">Loading sellers...</div>
                ) : filteredSellers.length === 0 ? (
                    <div className="text-center py-12">
                        <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                            <i className="ri-store-2-line text-gray-400 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-medium text-gray-900 mb-2">No sellers found</h3>
                        <p className="text-gray-600">
                            {activeSearchQuery ? 'Try adjusting your search terms' : 'No sellers have been registered yet'}
                        </p>
                    </div>
                ) : (
                    <>
                        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                            {pagedSellers.map(seller => (
                                <SellerCard
                                    key={seller.id}
                                    seller={seller}
                                    onEdit={setEditingSeller}
                                    onDelete={handleDelete}
                                    onView={setSelectedSeller}
                                    isDeleting={deletingSellerId === seller.id}
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

            {selectedSeller && (
                <SellerDetailModal
                    seller={selectedSeller}
                    onClose={() => setSelectedSeller(null)}
                    onEdit={setEditingSeller}
                />
            )}

            {editingSeller && (
                <EditSellerModal
                    seller={editingSeller}
                    onClose={() => setEditingSeller(null)}
                    onSave={handleSave}
                    isSaving={isSaving}
                />
            )}
        </div>
    );
}