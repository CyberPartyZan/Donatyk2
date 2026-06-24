import { useState, useRef, useEffect } from 'react';

interface Category {
    id: string;
    title: string;
    description: string;
    itemCount: number;
    parentId: string | null;
}

interface AddCategoryModalProps {
    isOpen: boolean;
    onClose: () => void;
    onAdd: (title: string, description: string, parentId: string | null) => Promise<boolean> | boolean;
    onEdit?: (id: string, title: string, description: string, parentId: string | null) => Promise<boolean> | boolean;
    editingCategory?: Category | null;
    categories: Category[];
}

export default function AddCategoryModal({ isOpen, onClose, onAdd, onEdit, editingCategory, categories }: AddCategoryModalProps) {
    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');
    const [parentId, setParentId] = useState<string | null>(null);
    const [parentSearch, setParentSearch] = useState('');
    const [showParentDropdown, setShowParentDropdown] = useState(false);
    const [submitError, setSubmitError] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    const isEditing = !!editingCategory;

    useEffect(() => {
        if (isOpen && editingCategory) {
            setTitle(editingCategory.title);
            setDescription(editingCategory.description);
            setParentId(editingCategory.parentId);
            setParentSearch(editingCategory.parentId ? categories.find(c => c.id === editingCategory.parentId)?.title || '' : '');
            setSubmitError(null);
        } else if (isOpen && !editingCategory) {
            setTitle('');
            setDescription('');
            setParentId(null);
            setParentSearch('');
            setSubmitError(null);
        }
    }, [isOpen, editingCategory, categories]);

    useEffect(() => {
        const handleClickOutside = (e: MouseEvent) => {
            if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
                setShowParentDropdown(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    if (!isOpen) return null;

    const filteredCategories = categories.filter(c =>
        c.id !== (editingCategory?.id || '') &&
        c.title.toLowerCase().includes(parentSearch.toLowerCase())
    );

    const selectedParent = parentId ? categories.find(c => c.id === parentId) : null;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!title.trim() || !description.trim()) return;

        setIsSubmitting(true);
        setSubmitError(null);

        try {
            const success = isEditing && onEdit
                ? await onEdit(editingCategory.id, title, description, parentId)
                : await onAdd(title, description, parentId);

            if (success === false) {
                setSubmitError('Unable to save category. Please try again.');
                return;
            }

            onClose();
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleClose = () => {
        setTitle('');
        setDescription('');
        setParentId(null);
        setParentSearch('');
        setSubmitError(null);
        onClose();
    };

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-lg max-w-lg w-full p-6">
                <div className="flex items-center justify-between mb-6">
                    <h2 className="text-xl font-semibold text-gray-900">
                        {isEditing ? 'Edit Category' : 'Add New Category'}
                    </h2>
                    <button
                        onClick={handleClose}
                        className="w-8 h-8 flex items-center justify-center text-gray-400 hover:text-gray-600 transition-colors"
                        disabled={isSubmitting}
                    >
                        <i className="ri-close-line text-xl"></i>
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                            Category Title <span className="text-red-500">*</span>
                        </label>
                        <input
                            type="text"
                            value={title}
                            onChange={(e) => setTitle(e.target.value)}
                            className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                            placeholder="e.g., Electronics, Fashion, Home & Garden"
                            required
                            disabled={isSubmitting}
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                            Description <span className="text-red-500">*</span>
                        </label>
                        <textarea
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            rows={4}
                            className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 resize-none"
                            placeholder="Describe what types of items belong to this category..."
                            required
                            disabled={isSubmitting}
                        />
                    </div>

                    <div ref={dropdownRef} className="relative">
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                            Parent Category <span className="text-gray-400 text-xs">(optional)</span>
                        </label>
                        <div
                            onClick={() => !isSubmitting && setShowParentDropdown(!showParentDropdown)}
                            className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 cursor-pointer flex items-center justify-between bg-white"
                        >
                            {selectedParent ? (
                                <span className="text-gray-900">{selectedParent.title}</span>
                            ) : (
                                <span className="text-gray-400">No parent category</span>
                            )}
                            {showParentDropdown ? (
                                <i className="ri-arrow-up-s-line text-gray-400"></i>
                            ) : (
                                <i className="ri-arrow-down-s-line text-gray-400"></i>
                            )}
                        </div>

                        {showParentDropdown && (
                            <div className="absolute z-10 mt-1 w-full bg-white border border-gray-200 rounded-md shadow-lg max-h-60 overflow-hidden">
                                <div className="p-2 border-b border-gray-100">
                                    <div className="relative">
                                        <i className="ri-search-line absolute left-2 top-1/2 -translate-y-1/2 text-gray-400 text-xs"></i>
                                        <input
                                            type="text"
                                            value={parentSearch}
                                            onChange={(e) => setParentSearch(e.target.value)}
                                            placeholder="Search categories..."
                                            className="w-full pl-7 pr-3 py-1.5 border border-gray-200 rounded text-xs focus:outline-none focus:ring-1 focus:ring-teal-500"
                                            onClick={(e) => e.stopPropagation()}
                                            disabled={isSubmitting}
                                        />
                                    </div>
                                </div>
                                <div className="overflow-y-auto max-h-44">
                                    <div
                                        onClick={() => {
                                            setParentId(null);
                                            setParentSearch('');
                                            setShowParentDropdown(false);
                                        }}
                                        className={`px-3 py-2 text-sm cursor-pointer hover:bg-gray-50 flex items-center justify-between ${parentId === null ? 'bg-teal-50 text-teal-700' : 'text-gray-700'
                                            }`}
                                    >
                                        <span>No parent category</span>
                                        {parentId === null && <i className="ri-check-line text-teal-600"></i>}
                                    </div>
                                    {filteredCategories.map(cat => (
                                        <div
                                            key={cat.id}
                                            onClick={() => {
                                                setParentId(cat.id);
                                                setParentSearch(cat.title);
                                                setShowParentDropdown(false);
                                            }}
                                            className={`px-3 py-2 text-sm cursor-pointer hover:bg-gray-50 flex items-center justify-between ${parentId === cat.id ? 'bg-teal-50 text-teal-700' : 'text-gray-700'
                                                }`}
                                        >
                                            <span>{cat.title}</span>
                                            {parentId === cat.id && <i className="ri-check-line text-teal-600"></i>}
                                        </div>
                                    ))}
                                    {filteredCategories.length === 0 && parentSearch && (
                                        <div className="px-3 py-2 text-sm text-gray-400 text-center">No categories found</div>
                                    )}
                                </div>
                            </div>
                        )}
                    </div>

                    {submitError && <p className="text-sm text-red-600">{submitError}</p>}

                    <div className="flex gap-3 pt-2">
                        <button
                            type="button"
                            onClick={handleClose}
                            className="flex-1 px-4 py-2 bg-gray-100 text-gray-700 text-sm font-medium rounded-md hover:bg-gray-200 transition-colors whitespace-nowrap"
                            disabled={isSubmitting}
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="flex-1 px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors whitespace-nowrap disabled:opacity-70"
                            disabled={isSubmitting}
                        >
                            <i className={`${isEditing ? 'ri-edit-line' : 'ri-add-line'} mr-2`}></i>
                            {isSubmitting ? 'Saving...' : (isEditing ? 'Save Changes' : 'Add Category')}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}