import { useState } from 'react';

interface CategoryCardProps {
    category: {
        id: string;
        title: string;
        description: string;
        itemCount: number;
        parentId: string | null;
    };
    parentTitle: string | null;
    onEdit: (id: string) => void;
    onDelete: (id: string) => void;
}

export default function CategoryCard({ category, parentTitle, onEdit, onDelete }: CategoryCardProps) {
    const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

    const handleDelete = () => {
        onDelete(category.id);
        setShowDeleteConfirm(false);
    };

    return (
        <>
            <div className="bg-white rounded-lg border border-gray-200 p-6 hover:shadow-md transition-shadow">
                <div className="flex items-start justify-between mb-3">
                    <div>
                        <h3 className="text-lg font-semibold text-gray-900">{category.title}</h3>
                        {parentTitle && (
                            <p className="text-xs text-teal-600 mt-0.5 flex items-center gap-1">
                                <i className="ri-subtract-line"></i>
                                Subcategory of {parentTitle}
                            </p>
                        )}
                    </div>
                    <span className="px-3 py-1 bg-teal-50 text-teal-700 text-sm font-medium rounded-full whitespace-nowrap">
                        {category.itemCount} items
                    </span>
                </div>
                <p className="text-sm text-gray-600 mb-4 line-clamp-2">{category.description}</p>
                <div className="flex gap-2">
                    <button
                        onClick={() => onEdit(category.id)}
                        className="flex-1 px-4 py-2 bg-gray-50 text-gray-700 text-sm font-medium rounded-md hover:bg-gray-100 transition-colors whitespace-nowrap cursor-pointer"
                    >
                        <i className="ri-edit-line mr-2"></i>
                        Edit
                    </button>
                    <button
                        onClick={() => setShowDeleteConfirm(true)}
                        className="px-4 py-2 bg-red-50 text-red-600 text-sm font-medium rounded-md hover:bg-red-100 transition-colors whitespace-nowrap cursor-pointer"
                    >
                        <i className="ri-delete-bin-line"></i>
                    </button>
                </div>
            </div>

            {showDeleteConfirm && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg max-w-md w-full p-6">
                        <div className="flex items-center gap-3 mb-4">
                            <div className="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center">
                                <i className="ri-alert-line text-red-600 text-xl"></i>
                            </div>
                            <div>
                                <h3 className="text-lg font-semibold text-gray-900">Delete Category</h3>
                                <p className="text-sm text-gray-600">This action cannot be undone</p>
                            </div>
                        </div>
                        <p className="text-sm text-gray-700 mb-6">
                            Are you sure you want to delete <strong>{category.title}</strong>? This category has{' '}
                            <strong>{category.itemCount} items</strong> assigned to it.
                        </p>
                        <div className="flex gap-3">
                            <button
                                onClick={() => setShowDeleteConfirm(false)}
                                className="flex-1 px-4 py-2 bg-gray-100 text-gray-700 text-sm font-medium rounded-md hover:bg-gray-200 transition-colors whitespace-nowrap cursor-pointer"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleDelete}
                                className="flex-1 px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-md hover:bg-red-700 transition-colors whitespace-nowrap cursor-pointer"
                            >
                                Delete Category
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}