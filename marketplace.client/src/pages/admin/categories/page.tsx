import { useState, useEffect, useRef } from 'react';
import { categories as initialCategories } from '../../../mocks/categories';
import CategoryCard from './components/CategoryCard';
import AddCategoryModal from './components/AddCategoryModal';
import Pagination from '@/components/base/Pagination';

const ITEMS_PER_PAGE = 9;

interface Category {
    id: string;
    title: string;
    description: string;
    itemCount: number;
    parentId: string | null;
}

export default function CategoriesPage() {
    const [categories, setCategories] = useState<Category[]>(initialCategories);
    const [isAddModalOpen, setIsAddModalOpen] = useState(false);
    const [editingCategoryId, setEditingCategoryId] = useState<string | null>(null);
    const [searchQuery, setSearchQuery] = useState('');
    const [currentPage, setCurrentPage] = useState(1);

    const editingCategory = editingCategoryId ? categories.find(c => c.id === editingCategoryId) || null : null;

    const handleAddCategory = (title: string, description: string, parentId: string | null) => {
        const newCategory: Category = {
            id: String(Date.now()),
            title,
            description,
            itemCount: 0,
            parentId
        };
        setCategories([newCategory, ...categories]);
    };

    const handleEditCategory = (id: string, title: string, description: string, parentId: string | null) => {
        setCategories(categories.map(cat =>
            cat.id === id ? { ...cat, title, description, parentId } : cat
        ));
    };

    const handleDeleteCategory = (id: string) => {
        setCategories(categories.filter(cat => cat.id !== id));
    };

    const getParentTitle = (parentId: string | null) => {
        if (!parentId) return null;
        return categories.find(c => c.id === parentId)?.title || null;
    };

    const filteredCategories = categories.filter(cat =>
        cat.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
        cat.description.toLowerCase().includes(searchQuery.toLowerCase())
    );

    return (
        <div className="min-h-screen bg-gray-50">
            <div className="max-w-7xl mx-auto px-6 py-8">
                {/* Header */}
                <div className="mb-8">
                    <h1 className="text-3xl font-bold text-gray-900 mb-2">Categories</h1>
                    <p className="text-sm text-gray-600">Manage your marketplace categories and organize items</p>
                </div>

                {/* Actions Bar */}
                <div className="bg-white rounded-lg border border-gray-200 p-4 mb-6">
                    <div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
                        <div className="relative flex-1 w-full sm:max-w-md">
                            <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"></i>
                            <input
                                type="text"
                                value={searchQuery}
                                onChange={(e) => setSearchQuery(e.target.value)}
                                placeholder="Search categories..."
                                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                            />
                        </div>
                        <button
                            onClick={() => { setEditingCategoryId(null); setIsAddModalOpen(true); }}
                            className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors whitespace-nowrap"
                        >
                            <i className="ri-add-line mr-2"></i>
                            Add Category
                        </button>
                    </div>
                </div>

                {/* Stats */}
                <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
                    <div className="bg-white rounded-lg border border-gray-200 p-4">
                        <div className="flex items-center gap-3">
                            <div className="w-10 h-10 bg-teal-100 rounded-lg flex items-center justify-center">
                                <i className="ri-folder-line text-teal-600 text-lg"></i>
                            </div>
                            <div>
                                <p className="text-sm text-gray-600">Total Categories</p>
                                <p className="text-2xl font-bold text-gray-900">{categories.length}</p>
                            </div>
                        </div>
                    </div>
                    <div className="bg-white rounded-lg border border-gray-200 p-4">
                        <div className="flex items-center gap-3">
                            <div className="w-10 h-10 bg-amber-100 rounded-lg flex items-center justify-center">
                                <i className="ri-shopping-bag-line text-amber-600 text-lg"></i>
                            </div>
                            <div>
                                <p className="text-sm text-gray-600">Total Items</p>
                                <p className="text-2xl font-bold text-gray-900">
                                    {categories.reduce((sum, cat) => sum + cat.itemCount, 0)}
                                </p>
                            </div>
                        </div>
                    </div>
                    <div className="bg-white rounded-lg border border-gray-200 p-4">
                        <div className="flex items-center gap-3">
                            <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                                <i className="ri-bar-chart-line text-green-600 text-lg"></i>
                            </div>
                            <div>
                                <p className="text-sm text-gray-600">Avg Items/Category</p>
                                <p className="text-2xl font-bold text-gray-900">
                                    {categories.length > 0
                                        ? Math.round(categories.reduce((sum, cat) => sum + cat.itemCount, 0) / categories.length)
                                        : 0
                                    }
                                </p>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Categories Grid */}
                {filteredCategories.length > 0 ? (
                    <>
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                            {filteredCategories.slice((currentPage - 1) * ITEMS_PER_PAGE, currentPage * ITEMS_PER_PAGE).map(category => (
                                <CategoryCard
                                    key={category.id}
                                    category={category}
                                    parentTitle={getParentTitle(category.parentId)}
                                    onEdit={(id) => {
                                        setEditingCategoryId(id);
                                        setIsAddModalOpen(true);
                                    }}
                                    onDelete={handleDeleteCategory}
                                />
                            ))}
                        </div>
                        <Pagination currentPage={currentPage} totalPages={Math.ceil(filteredCategories.length / ITEMS_PER_PAGE)} onPageChange={(p) => setCurrentPage(p)} />
                    </>
                ) : (
                    <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
                        <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                            <i className="ri-folder-line text-gray-400 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900 mb-2">No categories found</h3>
                        <p className="text-sm text-gray-600 mb-4">
                            {searchQuery ? 'Try adjusting your search terms' : 'Get started by adding your first category'}
                        </p>
                        {!searchQuery && (
                            <button
                                onClick={() => { setEditingCategoryId(null); setIsAddModalOpen(true); }}
                                className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors whitespace-nowrap"
                            >
                                <i className="ri-add-line mr-2"></i>
                                Add Category
                            </button>
                        )}
                    </div>
                )}
            </div>

            <AddCategoryModal
                isOpen={isAddModalOpen}
                onClose={() => { setIsAddModalOpen(false); setEditingCategoryId(null); }}
                onAdd={handleAddCategory}
                onEdit={handleEditCategory}
                editingCategory={editingCategory}
                categories={categories}
            />
        </div>
    );
}