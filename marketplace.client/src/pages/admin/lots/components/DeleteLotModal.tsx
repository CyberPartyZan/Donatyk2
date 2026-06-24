interface DeleteLotModalProps {
    isOpen: boolean;
    lotName: string;
    onConfirm: () => void;
    onCancel: () => void;
}

export default function DeleteLotModal({ isOpen, lotName, onConfirm, onCancel }: DeleteLotModalProps) {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-lg shadow-xl max-w-md w-full">
                <div className="p-6">
                    <div className="flex items-start gap-4">
                        <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center flex-shrink-0">
                            <i className="ri-error-warning-line text-2xl text-red-600"></i>
                        </div>
                        <div className="flex-1 min-w-0">
                            <h3 className="text-lg font-semibold text-gray-900 mb-1">Delete Lot</h3>
                            <p className="text-sm text-gray-600">
                                Are you sure you want to delete <strong className="text-gray-900">"{lotName}"</strong>? This action cannot be undone. All data associated with this lot will be permanently removed.
                            </p>
                        </div>
                    </div>
                </div>

                <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-gray-200 bg-gray-50 rounded-b-lg">
                    <button
                        onClick={onCancel}
                        className="px-5 py-2 bg-white border border-gray-300 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-100 transition-colors cursor-pointer whitespace-nowrap"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={onConfirm}
                        className="px-5 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors cursor-pointer whitespace-nowrap"
                    >
                        <i className="ri-delete-bin-line mr-1"></i>
                        Delete
                    </button>
                </div>
            </div>
        </div>
    );
}