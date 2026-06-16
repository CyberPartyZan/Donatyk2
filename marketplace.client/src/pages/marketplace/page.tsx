
import { useState } from 'react';
import { Link } from 'react-router-dom';
import MarketplaceHeader from './components/MarketplaceHeader';
import CategoryMenu from './components/CategoryMenu';
import FilterSidebar from './components/FilterSidebar';
import ProductGrid from './components/ProductGrid';

export default function Marketplace() {
    const [showCategories, setShowCategories] = useState(false);
    const [filters, setFilters] = useState({
        minPrice: '',
        maxPrice: '',
        minDiscount: '',
        maxDiscount: '',
        lotType: [] as string[],
    });
    const [sortBy, setSortBy] = useState('date');

    return (
        <div className="min-h-screen bg-gray-50">
            <MarketplaceHeader onCategoriesClick={() => setShowCategories(!showCategories)} />

            {showCategories && (
                <CategoryMenu onClose={() => setShowCategories(false)} />
            )}

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

                        <ProductGrid filters={filters} sortBy={sortBy} />
                    </div>
                </div>
            </div>
        </div>
    );
}
