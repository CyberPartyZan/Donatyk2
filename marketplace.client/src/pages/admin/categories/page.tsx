import { useCallback, useEffect, useMemo, useState } from 'react';
import CategoryCard from './components/CategoryCard';
import AddCategoryModal from './components/AddCategoryModal';
import Pagination from '@/components/base/Pagination';

const ITEMS_PER_PAGE = 9;
const ACCESS_TOKEN_KEY = 'auth_access_token';

interface Category {
    id: string;
    title: string;
    description: string;
    itemCount: number;
    parentId: string | null;
}

interface ApiCategoryDto {
    id: string;
    name?: string;
    title?: string;
    description?: string;
    parentId?: string | null;
    itemCount?: number;
    subCategories?: ApiCategoryDto[];
}

interface CategoryMutationDto {
    id?: string;
    name: string;
    description: string;
    parentId: string | null;
    subCategories: ApiCategoryDto[];
}

const flattenCategories = (apiCategories: ApiCategoryDto[]): Category[] => {
    const result: Category[] = [];

    const walk = (nodes: ApiCategoryDto[], inheritedParentId: string | null = null) => {
        for (const node of nodes) {
            const currentParentId = node.parentId ?? inheritedParentId ?? null;

            result.push({
                id: node.id,
                title: node.name ?? node.title ?? '',
                description: node.description ?? '',
                itemCount: node.itemCount ?? 0,
                parentId: currentParentId,
            });

            if (node.subCategories?.length) {
                walk(node.subCategories, node.id);
            }
        }
    };

    walk(apiCategories);

    const map = new Map<string, Category>();
    for (const category of result) {
        map.set(category.id, category);
    }

    return Array.from(map.values());
};

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

export default function CategoriesPage() {
    const [categories, setCategories] = useState<Category[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [mutationError, setMutationError] = useState<string | null>(null);

    const [isAddModalOpen, setIsAddModalOpen] = useState(false);
    const [editingCategoryId, setEditingCategoryId] = useState<string | null>(null);
    const [searchQuery, setSearchQuery] = useState('');
    const [currentPage, setCurrentPage] = useState(1);

    const loadCategories = useCallback(async () => {
        setIsLoading(true);
        setError(null);

        try {
            const response = await fetch('/api/categories');
            if (!response.ok) {
                throw new Error(`Failed to load categories: ${response.status}`);
            }

            const data = (await response.json()) as ApiCategoryDto[];
            const normalized = flattenCategories(Array.isArray(data) ? data : []);
            setCategories(normalized);
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load categories');
            setCategories([]);
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        void loadCategories();
    }, [loadCategories]);

    useEffect(() => {
        setCurrentPage(1);
    }, [searchQuery]);

    const editingCategory = editingCategoryId ? categories.find(c => c.id === editingCategoryId) || null : null;

    const handleAddCategory = async (title: string, description: string, parentId: string | null): Promise<boolean> => {
        setMutationError(null);

        const payload: CategoryMutationDto = {
            name: title.trim(),
            description: description.trim(),
            parentId,
            subCategories: [],
        };

        try {
            const response = await fetch('/api/categories', {
                method: 'POST',
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

            await loadCategories();
            setCurrentPage(1);
            return true;
        } catch (e) {
            setMutationError(e instanceof Error ? e.message : 'Failed to create category');
            return false;
        }
    };

    const handleEditCategory = async (id: string, title: string, description: string, parentId: string | null): Promise<boolean> => {
        setMutationError(null);

        const payload: CategoryMutationDto = {
            id,
            name: title.trim(),
            description: description.trim(),
            parentId,
            subCategories: [],
        };

        try {
            const response = await fetch(`/api/categories/${id}`, {
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

            await loadCategories();
            return true;
        } catch (e) {
            setMutationError(e instanceof Error ? e.message : 'Failed to update category');
            return false;
        }
    };

    const handleDeleteCategory = async (id: string): Promise<boolean> => {
        setMutationError(null);

        try {
            const response = await fetch(`/api/categories/${id}`, {
                method: 'DELETE',
                headers: {
                    ...getAuthHeader(),
                },
                credentials: 'include',
            });

            if (!response.ok) {
                setMutationError(await getResponseMessage(response));
                return false;
            }

            await loadCategories();
            return true;
        } catch (e) {
            setMutationError(e instanceof Error ? e.message : 'Failed to delete category');
            return false;
        }
    };

    const getParentTitle = (parentId: string | null) => {
        if (!parentId) return null;
        return categories.find(c => c.id === parentId)?.title || null;
    };

    const filteredCategories = useMemo(() => {
        const q = searchQuery.toLowerCase().trim();
        if (!q) return categories;

        return categories.filter(cat =>
            cat.title.toLowerCase().includes(q) ||
            cat.description.toLowerCase().includes(q)
        );
    }, [categories, searchQuery]);

    const totalPages = Math.max(1, Math.ceil(filteredCategories.length / ITEMS_PER_PAGE));

    useEffect(() => {
        if (currentPage > totalPages) {
            setCurrentPage(totalPages);
        }
    }, [currentPage, totalPages]);

    const pagedCategories = useMemo(
        () => filteredCategories.slice((currentPage - 1) * ITEMS_PER_PAGE, currentPage * ITEMS_PER_PAGE),
        [filteredCategories, currentPage]
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
                            onClick={() => { setMutationError(null); setEditingCategoryId(null); setIsAddModalOpen(true); }}
                            className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors whitespace-nowrap"
                        >
                            <i className="ri-add-line mr-2"></i>
                            Add Category
                        </button>
                    </div>
                    {mutationError && (
                        <p className="mt-3 text-sm text-red-600">{mutationError}</p>
                    )}
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

                {isLoading ? (
                    <div className="bg-white rounded-lg border border-gray-200 p-12 text-center text-gray-600">
                        Loading categories...
                    </div>
                ) : error ? (
                    <div className="bg-white rounded-lg border border-red-200 p-12 text-center">
                        <h3 className="text-lg font-semibold text-red-700 mb-2">Failed to load categories</h3>
                        <p className="text-sm text-gray-600 mb-4">{error}</p>
                        <button
                            onClick={() => void loadCategories()}
                            className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors"
                        >
                            Retry
                        </button>
                    </div>
                ) : filteredCategories.length > 0 ? (
                    <>
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                            {pagedCategories.map(category => (
                                <CategoryCard
                                    key={category.id}
                                    category={category}
                                    parentTitle={getParentTitle(category.parentId)}
                                    onEdit={(id) => {
                                        setMutationError(null);
                                        setEditingCategoryId(id);
                                        setIsAddModalOpen(true);
                                    }}
                                    onDelete={handleDeleteCategory}
                                />
                            ))}
                        </div>
                        <Pagination
                            currentPage={currentPage}
                            totalPages={totalPages}
                            onPageChange={(p) => setCurrentPage(p)}
                        />
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
                                onClick={() => { setMutationError(null); setEditingCategoryId(null); setIsAddModalOpen(true); }}
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