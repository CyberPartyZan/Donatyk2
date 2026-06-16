import { useState } from 'react';
import { mockSellers } from '../../../mocks/sellers';
import SellerCard from './components/SellerCard';
import SellerDetailModal from './components/SellerDetailModal';
import EditSellerModal from './components/EditSellerModal';

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

export default function SellersAdmin() {
    const [sellers, setSellers] = useState<Seller[]>(mockSellers);
    const [searchQuery, setSearchQuery] = useState('');
    const [selectedSeller, setSelectedSeller] = useState<Seller | null>(null);
    const [editingSeller, setEditingSeller] = useState<Seller | null>(null);

    const filteredSellers = sellers.filter(seller =>
        seller.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        seller.email.toLowerCase().includes(searchQuery.toLowerCase())
    );

    const handleDelete = (id: string) => {
        setSellers(sellers.filter(s => s.id !== id));
    };

    const handleSave = (updatedSeller: Seller) => {
        setSellers(sellers.map(s => s.id === updatedSeller.id ? updatedSeller : s));
        setEditingSeller(null);
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

                <div className="relative">
                    <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"></i>
                    <input
                        type="text"
                        placeholder="Search sellers by name or email..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-teal-500 focus:border-transparent text-sm"
                    />
                </div>
            </div>

            <div className="p-6">
                {filteredSellers.length === 0 ? (
                    <div className="text-center py-12">
                        <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                            <i className="ri-store-2-line text-gray-400 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-medium text-gray-900 mb-2">No sellers found</h3>
                        <p className="text-gray-600">
                            {searchQuery ? 'Try adjusting your search terms' : 'No sellers have been registered yet'}
                        </p>
                    </div>
                ) : (
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                        {filteredSellers.map(seller => (
                            <SellerCard
                                key={seller.id}
                                seller={seller}
                                onEdit={setEditingSeller}
                                onDelete={handleDelete}
                                onView={setSelectedSeller}
                            />
                        ))}
                    </div>
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
                />
            )}
        </div>
    );
}