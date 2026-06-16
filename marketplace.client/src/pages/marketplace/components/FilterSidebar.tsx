
interface FilterSidebarProps {
    filters: {
        minPrice: string;
        maxPrice: string;
        minDiscount: string;
        maxDiscount: string;
        lotType: string[];
    };
    setFilters: (filters: any) => void;
}

export default function FilterSidebar({ filters, setFilters }: FilterSidebarProps) {
    const toggleLotType = (type: string) => {
        const newLotTypes = filters.lotType.includes(type)
            ? filters.lotType.filter((t) => t !== type)
            : [...filters.lotType, type];
        setFilters({ ...filters, lotType: newLotTypes });
    };

    return (
        <div className="w-72 flex-shrink-0">
            <div className="bg-white rounded-lg shadow-sm p-6 sticky top-24">
                <h3 className="text-lg font-bold text-gray-900 mb-6">Filters</h3>

                <div className="space-y-6">
                    <div>
                        <label className="block text-sm font-semibold text-gray-900 mb-3">Price Range</label>
                        <div className="flex items-center gap-3">
                            <input
                                type="number"
                                placeholder="Min"
                                value={filters.minPrice}
                                onChange={(e) => setFilters({ ...filters, minPrice: e.target.value })}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                            />
                            <span className="text-gray-400">-</span>
                            <input
                                type="number"
                                placeholder="Max"
                                value={filters.maxPrice}
                                onChange={(e) => setFilters({ ...filters, maxPrice: e.target.value })}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                            />
                        </div>
                    </div>

                    <div>
                        <label className="block text-sm font-semibold text-gray-900 mb-3">Discount Range</label>
                        <div className="flex items-center gap-3">
                            <input
                                type="number"
                                placeholder="Min %"
                                value={filters.minDiscount}
                                onChange={(e) => setFilters({ ...filters, minDiscount: e.target.value })}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                            />
                            <span className="text-gray-400">-</span>
                            <input
                                type="number"
                                placeholder="Max %"
                                value={filters.maxDiscount}
                                onChange={(e) => setFilters({ ...filters, maxDiscount: e.target.value })}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                            />
                        </div>
                    </div>

                    <div>
                        <label className="block text-sm font-semibold text-gray-900 mb-3">Lot Type</label>
                        <div className="space-y-2">
                            <label className="flex items-center gap-3 cursor-pointer">
                                <input
                                    type="checkbox"
                                    checked={filters.lotType.includes('auction')}
                                    onChange={() => toggleLotType('auction')}
                                    className="w-4 h-4 text-teal-500 border-gray-300 rounded focus:ring-teal-500 cursor-pointer"
                                />
                                <span className="text-sm text-gray-700">Auction</span>
                            </label>
                            <label className="flex items-center gap-3 cursor-pointer">
                                <input
                                    type="checkbox"
                                    checked={filters.lotType.includes('draw')}
                                    onChange={() => toggleLotType('draw')}
                                    className="w-4 h-4 text-teal-500 border-gray-300 rounded focus:ring-teal-500 cursor-pointer"
                                />
                                <span className="text-sm text-gray-700">Draw</span>
                            </label>
                            <label className="flex items-center gap-3 cursor-pointer">
                                <input
                                    type="checkbox"
                                    checked={filters.lotType.includes('simple')}
                                    onChange={() => toggleLotType('simple')}
                                    className="w-4 h-4 text-teal-500 border-gray-300 rounded focus:ring-teal-500 cursor-pointer"
                                />
                                <span className="text-sm text-gray-700">Buy Now</span>
                            </label>
                        </div>
                    </div>

                    <button
                        onClick={() => setFilters({ minPrice: '', maxPrice: '', minDiscount: '', maxDiscount: '', lotType: [] })}
                        className="w-full py-2.5 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors text-sm font-medium whitespace-nowrap cursor-pointer"
                    >
                        Clear Filters
                    </button>
                </div>
            </div>
        </div>
    );
}
