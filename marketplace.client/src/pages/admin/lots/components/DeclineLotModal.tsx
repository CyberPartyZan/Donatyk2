import { useState } from 'react';

interface DeclineLotModalProps {
    isOpen: boolean;
    lotName: string;
    onConfirm: (reason: string) => void;
    onCancel: () => void;
}

export default function DeclineLotModal({ isOpen, lotName, onConfirm, onCancel }: DeclineLotModalProps) {
    const [reason, setReason] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = () => {
        const trimmed = reason.trim();
        if (!trimmed) {
            setError('Please provide a reason for declining this lot.');
            return;
        }
        if (trimmed.length > 500) {
            setError('Reason must be under 500 characters.');
            return;
        }
        onConfirm(trimmed);
        setReason('');
        setError('');
    };

    const handleCancel = () => {
        setReason('');
        setError('');
        onCancel();
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-lg shadow-xl max-w-md w-full">
                <div className="p-6">
                    <div className="flex items-start gap-4 mb-4">
                        <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center flex-shrink-0">
                            <i className="ri-close-circle-line text-2xl text-red-600"></i>
                        </div>
                        <div className="flex-1 min-w-0">
                            <h3 className="text-lg font-semibold text-gray-900 mb-1">Decline Lot</h3>
                            <p className="text-sm text-gray-600">
                                You are about to decline <strong className="text-gray-900">"{lotName}"</strong>. Please provide a reason so the seller understands why their lot was rejected.
                            </p>
                        </div>
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1.5">
                            Reason for Declining <span className="text-red-500">*</span>
                        </label>
                        <textarea
                            value={reason}
                            onChange={(e) => {
                                setReason(e.target.value);
                                if (e.target.value.trim()) setError('');
                            }}
                            rows={4}
                            maxLength={500}
                            placeholder="Explain why this lot is being declined..."
                            className={`w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-red-500 focus:border-transparent resize-none ${error ? 'border-red-400' : 'border-gray-300'
                                }`}
                        />
                        <div className="flex items-center justify-between mt-1">
                            {error ? (
                                <p className="text-xs text-red-500">{error}</p>
                            ) : (
                                <p className="text-xs text-gray-400">Be specific — this helps the seller improve</p>
                            )}
                            <p className="text-xs text-gray-400">{reason.length}/500</p>
                        </div>
                    </div>
                </div>

                <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-gray-200 bg-gray-50 rounded-b-lg">
                    <button
                        onClick={handleCancel}
                        className="px-5 py-2 bg-white border border-gray-300 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-100 transition-colors cursor-pointer whitespace-nowrap"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={handleSubmit}
                        className="px-5 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors cursor-pointer whitespace-nowrap"
                    >
                        <i className="ri-close-line mr-1"></i>
                        Decline Lot
                    </button>
                </div>
            </div>
        </div>
    );
}