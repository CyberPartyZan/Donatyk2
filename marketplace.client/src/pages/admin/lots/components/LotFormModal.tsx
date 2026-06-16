import { useState, useEffect, useRef } from 'react';
import { mockGoals } from '@/mocks/goals';

interface Characteristic {
    key: string;
    value: string;
}

interface Lot {
    id: string;
    name: string;
    description: string;
    price: number;
    compensation: number;
    stockCount: number;
    discountedPrice: number | null;
    type: 'Simple' | 'Auction' | 'Draw';
    stage: 'Created' | 'PendingApproval' | 'Denied' | 'Approved';
    isActive: boolean;
    categoryId: string;
    categoryName: string;
    goalId?: string;
    characteristics: Characteristic[];
    images: string[];
    endOfAuction?: string;
    auctionStepPercent?: number;
    currentBid?: number;
    ticketPrice?: number;
    totalTickets?: number;
    ticketsSold?: number;
    createdAt: string;
}

interface LotFormModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSave: (lot: Partial<Lot>) => void;
    lot?: Lot | null;
    categories: Array<{ id: string; name: string }>;
    hidePrice?: boolean;
}

export default function LotFormModal({ isOpen, onClose, onSave, lot, categories, hidePrice = false }: LotFormModalProps) {
    const [formData, setFormData] = useState({
        name: '',
        description: '',
        price: 0,
        compensation: 0,
        stockCount: 1,
        discountedPrice: '',
        type: 'Simple' as 'Simple' | 'Auction' | 'Draw',
        categoryId: '',
        isActive: true,
        endOfAuction: '',
        auctionStepPercent: 5,
        ticketPrice: 0,
        characteristics: [] as Characteristic[],
        images: [] as string[],
        goalId: ''
    });

    const [newChar, setNewChar] = useState({ key: '', value: '' });
    const [imageUrl, setImageUrl] = useState('');
    const [imageUrlError, setImageUrlError] = useState('');
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [showCompensationWarning, setShowCompensationWarning] = useState(false);

    useEffect(() => {
        if (lot) {
            setFormData({
                name: lot.name,
                description: lot.description,
                price: lot.price,
                compensation: lot.compensation,
                stockCount: lot.stockCount,
                discountedPrice: lot.discountedPrice?.toString() || '',
                type: lot.type,
                categoryId: lot.categoryId,
                isActive: lot.isActive,
                endOfAuction: lot.endOfAuction ? lot.endOfAuction.slice(0, 16) : '',
                auctionStepPercent: lot.auctionStepPercent || 5,
                ticketPrice: lot.ticketPrice || 0,
                characteristics: [...lot.characteristics],
                images: [...(lot.images || [])],
                goalId: (lot as any).goalId || ''
            });
        } else {
            setFormData({
                name: '',
                description: '',
                price: 0,
                compensation: 0,
                stockCount: 1,
                discountedPrice: '',
                type: 'Simple',
                categoryId: categories[0]?.id || '',
                isActive: true,
                endOfAuction: '',
                auctionStepPercent: 5,
                ticketPrice: 0,
                characteristics: [],
                images: [],
                goalId: ''
            });
        }
        setImageUrl('');
        setImageUrlError('');
    }, [lot, categories, isOpen]);

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        const lotData: Partial<Lot> = {
            name: formData.name,
            description: formData.description,
            price: formData.price,
            compensation: formData.compensation,
            stockCount: formData.stockCount,
            discountedPrice: formData.discountedPrice ? parseFloat(formData.discountedPrice) : null,
            type: formData.type,
            categoryId: formData.categoryId,
            isActive: formData.isActive,
            characteristics: formData.characteristics,
            images: formData.images,
            stage: lot?.stage || 'Created',
            goalId: formData.goalId || undefined
        };

        if (formData.type === 'Auction') {
            lotData.endOfAuction = formData.endOfAuction;
            lotData.auctionStepPercent = formData.auctionStepPercent;
        }

        if (formData.type === 'Draw') {
            lotData.ticketPrice = formData.ticketPrice;
            if (formData.ticketPrice > 0 && formData.price > 0) {
                lotData.totalTickets = Math.floor(formData.price / formData.ticketPrice);
            }
            lotData.ticketsSold = lot?.ticketsSold || 0;
        }

        onSave(lotData);
    };

    const addCharacteristic = () => {
        if (newChar.key && newChar.value) {
            setFormData({ ...formData, characteristics: [...formData.characteristics, { ...newChar }] });
            setNewChar({ key: '', value: '' });
        }
    };

    const removeCharacteristic = (index: number) => {
        setFormData({ ...formData, characteristics: formData.characteristics.filter((_, i) => i !== index) });
    };

    const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = Array.from(e.target.files || []);
        files.forEach((file) => {
            const reader = new FileReader();
            reader.onload = (ev) => {
                const result = ev.target?.result as string;
                setFormData((prev) => ({ ...prev, images: [...prev.images, result] }));
            };
            reader.readAsDataURL(file);
        });
        if (fileInputRef.current) fileInputRef.current.value = '';
    };

    const addImageUrl = () => {
        const trimmed = imageUrl.trim();
        if (!trimmed) return;
        try {
            new URL(trimmed);
            setFormData((prev) => ({ ...prev, images: [...prev.images, trimmed] }));
            setImageUrl('');
            setImageUrlError('');
        } catch {
            setImageUrlError('Please enter a valid URL.');
        }
    };

    const removeImage = (index: number) => {
        setFormData((prev) => ({ ...prev, images: prev.images.filter((_, i) => i !== index) }));
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-lg shadow-xl max-w-3xl w-full max-h-[90vh] overflow-y-auto">
                <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
                    <h2 className="text-xl font-semibold text-gray-900">
                        {lot ? 'Edit Lot' : 'Create New Lot'}
                    </h2>
                    <button onClick={onClose} className="w-8 h-8 flex items-center justify-center text-gray-400 hover:text-gray-600 cursor-pointer">
                        <i className="ri-close-line text-xl"></i>
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6">
                    <div className="space-y-4">
                        {/* Name */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
                            <input
                                type="text"
                                value={formData.name}
                                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                required
                            />
                        </div>

                        {/* Description */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                            <textarea
                                value={formData.description}
                                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                                rows={3}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                required
                            />
                        </div>

                        {/* Images Section */}
                        <div className="bg-gray-50 rounded-lg p-4">
                            <h3 className="text-sm font-semibold text-gray-900 mb-3">
                                <i className="ri-image-line mr-1"></i>
                                Images
                            </h3>

                            {/* Previews */}
                            {formData.images.length > 0 && (
                                <div className="grid grid-cols-4 gap-3 mb-4">
                                    {formData.images.map((img, index) => (
                                        <div key={index} className="relative group rounded-lg overflow-hidden border border-gray-200 bg-white" style={{ height: '90px' }}>
                                            <img
                                                src={img}
                                                alt={`Lot image ${index + 1}`}
                                                className="w-full h-full object-cover object-top"
                                                onError={(e) => {
                                                    (e.target as HTMLImageElement).src = 'https://readdy.ai/api/search-image?query=placeholder%20product%20image%20simple%20white%20background&width=200&height=200&seq=placeholder-err&orientation=squarish';
                                                }}
                                            />
                                            <button
                                                type="button"
                                                onClick={() => removeImage(index)}
                                                className="absolute top-1 right-1 w-6 h-6 flex items-center justify-center bg-red-600 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer"
                                            >
                                                <i className="ri-close-line text-xs"></i>
                                            </button>
                                            {index === 0 && (
                                                <span className="absolute bottom-1 left-1 text-xs bg-teal-600 text-white px-1.5 py-0.5 rounded font-medium">
                                                    Main
                                                </span>
                                            )}
                                        </div>
                                    ))}
                                </div>
                            )}

                            {/* Upload file */}
                            <div className="mb-3">
                                <input
                                    ref={fileInputRef}
                                    type="file"
                                    accept="image/*"
                                    multiple
                                    onChange={handleFileUpload}
                                    className="hidden"
                                    id="lot-image-upload"
                                />
                                <label
                                    htmlFor="lot-image-upload"
                                    className="flex items-center justify-center gap-2 w-full px-4 py-3 border-2 border-dashed border-gray-300 rounded-lg text-sm text-gray-600 hover:border-teal-400 hover:text-teal-600 transition-colors cursor-pointer"
                                >
                                    <i className="ri-upload-cloud-line text-lg"></i>
                                    Click to upload photos from your device
                                </label>
                            </div>

                            {/* Link URL */}
                            <div>
                                <p className="text-xs text-gray-500 mb-1.5">Or add an image by URL</p>
                                <div className="flex gap-2">
                                    <input
                                        type="text"
                                        placeholder="https://example.com/image.jpg"
                                        value={imageUrl}
                                        onChange={(e) => { setImageUrl(e.target.value); setImageUrlError(''); }}
                                        onKeyDown={(e) => { if (e.key === 'Enter') { e.preventDefault(); addImageUrl(); } }}
                                        className={`flex-1 px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent ${imageUrlError ? 'border-red-400' : 'border-gray-300'}`}
                                    />
                                    <button
                                        type="button"
                                        onClick={addImageUrl}
                                        className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                                    >
                                        <i className="ri-link mr-1"></i>
                                        Add
                                    </button>
                                </div>
                                {imageUrlError && <p className="text-xs text-red-500 mt-1">{imageUrlError}</p>}
                            </div>

                            {formData.images.length > 0 && (
                                <p className="text-xs text-gray-400 mt-2">
                                    <i className="ri-information-line mr-1"></i>
                                    The first image is used as the main display image. Hover over an image to remove it.
                                </p>
                            )}
                        </div>

                        {/* Type & Category */}
                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
                                <select
                                    value={formData.type}
                                    onChange={(e) => setFormData({ ...formData, type: e.target.value as 'Simple' | 'Auction' | 'Draw' })}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                                >
                                    <option value="Simple">Simple</option>
                                    <option value="Auction">Auction</option>
                                    <option value="Draw">Draw</option>
                                </select>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Category</label>
                                <select
                                    value={formData.categoryId}
                                    onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                                    required
                                >
                                    {categories.map((cat) => (
                                        <option key={cat.id} value={cat.id}>{cat.name}</option>
                                    ))}
                                </select>
                            </div>
                        </div>

                        {/* Goal */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                <i className="ri-flag-line mr-1"></i>Serves Goal (Charity Fund)
                            </label>
                            <select
                                value={formData.goalId}
                                onChange={(e) => setFormData({ ...formData, goalId: e.target.value })}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                            >
                                <option value="">Default Charity Fund</option>
                                {mockGoals.filter(g => g.status === 'Active').map(goal => (
                                    <option key={goal.id} value={goal.id}>
                                        {goal.organizationName} — {goal.title} (${goal.moneyRaised.toLocaleString()}/${goal.moneyBudget.toLocaleString()})
                                    </option>
                                ))}
                            </select>
                            <p className="text-xs text-gray-400 mt-1">Money from this lot will contribute to the selected goal</p>
                        </div>

                        {/* Price & Compensation */}
                        <div className={`grid ${(!lot || hidePrice) ? 'grid-cols-1' : 'grid-cols-2'} gap-4`}>
                            {lot && !hidePrice && (
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Price</label>
                                    <input
                                        type="number"
                                        value={formData.price}
                                        onChange={(e) => setFormData({ ...formData, price: parseFloat(e.target.value) || 0 })}
                                        className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                        required min="0" step="0.01"
                                    />
                                </div>
                            )}
                            {hidePrice && (
                                <>
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">Compensation</label>
                                        <input
                                            type="number"
                                            value={formData.compensation}
                                            onChange={(e) => {
                                                const newVal = parseFloat(e.target.value) || 0;
                                                setFormData({ ...formData, compensation: newVal });
                                                if (lot && newVal > lot.compensation) {
                                                    setShowCompensationWarning(true);
                                                } else {
                                                    setShowCompensationWarning(false);
                                                }
                                            }}
                                            className={`w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent ${showCompensationWarning ? 'border-amber-400 bg-amber-50' : 'border-gray-300'
                                                }`}
                                            required min="0" step="0.01"
                                        />
                                        {showCompensationWarning && (
                                            <div className="flex items-start gap-2 mt-2 p-3 bg-amber-50 border border-amber-200 rounded-lg">
                                                <i className="ri-error-warning-line text-amber-600 mt-0.5 flex-shrink-0"></i>
                                                <div>
                                                    <p className="text-sm font-medium text-amber-800">
                                                        Compensation Upscale Detected
                                                    </p>
                                                    <p className="text-xs text-amber-700 mt-0.5">
                                                        If you increase the compensation above its original value (${lot?.compensation?.toLocaleString()}), this lot will need to go through the approval process again before it can be listed.
                                                    </p>
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">Stock Count</label>
                                        <input
                                            type="number"
                                            value={formData.stockCount}
                                            onChange={(e) => setFormData({ ...formData, stockCount: parseInt(e.target.value) || 1 })}
                                            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                            required min="1"
                                        />
                                    </div>
                                </>
                            )}
                            {!hidePrice && (
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Compensation</label>
                                    <input
                                        type="number"
                                        value={formData.compensation}
                                        onChange={(e) => {
                                            const newVal = parseFloat(e.target.value) || 0;
                                            setFormData({ ...formData, compensation: newVal });
                                            if (lot && newVal > lot.compensation) {
                                                setShowCompensationWarning(true);
                                            } else {
                                                setShowCompensationWarning(false);
                                            }
                                        }}
                                        className={`w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent ${showCompensationWarning ? 'border-amber-400 bg-amber-50' : 'border-gray-300'
                                            }`}
                                        required min="0" step="0.01"
                                    />
                                    {showCompensationWarning && (
                                        <div className="flex items-start gap-2 mt-2 p-3 bg-amber-50 border border-amber-200 rounded-lg">
                                            <i className="ri-error-warning-line text-amber-600 mt-0.5 flex-shrink-0"></i>
                                            <div>
                                                <p className="text-sm font-medium text-amber-800">
                                                    Compensation Upscale Detected
                                                </p>
                                                <p className="text-xs text-amber-700 mt-0.5">
                                                    If you increase the compensation above its original value (${lot?.compensation?.toLocaleString()}), this lot will need to go through the approval process again before it can be listed.
                                                </p>
                                            </div>
                                        </div>
                                    )}
                                </div>
                            )}
                        </div>

                        {/* Stock & Discounted Price */}
                        {!hidePrice && (
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Stock Count</label>
                                    <input
                                        type="number"
                                        value={formData.stockCount}
                                        onChange={(e) => setFormData({ ...formData, stockCount: parseInt(e.target.value) || 1 })}
                                        className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                        required min="1"
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Discounted Price (Optional)</label>
                                    <input
                                        type="number"
                                        value={formData.discountedPrice}
                                        onChange={(e) => setFormData({ ...formData, discountedPrice: e.target.value })}
                                        className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                        min="0" step="0.01"
                                    />
                                </div>
                            </div>
                        )}

                        {/* Auction Settings */}
                        {formData.type === 'Auction' && (
                            <div className="bg-amber-50 rounded-lg p-4 space-y-4">
                                <h3 className="text-sm font-semibold text-amber-900">Auction Settings</h3>
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-medium text-amber-700 mb-1">End of Auction</label>
                                        <input
                                            type="datetime-local"
                                            value={formData.endOfAuction}
                                            onChange={(e) => setFormData({ ...formData, endOfAuction: e.target.value })}
                                            className="w-full px-3 py-2 border border-amber-300 rounded-lg text-sm focus:ring-2 focus:ring-amber-500 focus:border-transparent"
                                            required
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-amber-700 mb-1">Auction Step %</label>
                                        <input
                                            type="number"
                                            value={formData.auctionStepPercent}
                                            onChange={(e) => setFormData({ ...formData, auctionStepPercent: parseFloat(e.target.value) || 5 })}
                                            className="w-full px-3 py-2 border border-amber-300 rounded-lg text-sm focus:ring-2 focus:ring-amber-500 focus:border-transparent"
                                            required min="1" step="0.1"
                                        />
                                    </div>
                                </div>
                            </div>
                        )}

                        {/* Draw Settings */}
                        {formData.type === 'Draw' && (
                            <div className="bg-purple-50 rounded-lg p-4">
                                <h3 className="text-sm font-semibold text-purple-900 mb-3">Draw Settings</h3>
                                <div>
                                    <label className="block text-sm font-medium text-purple-700 mb-1">Ticket Price</label>
                                    <input
                                        type="number"
                                        value={formData.ticketPrice}
                                        onChange={(e) => setFormData({ ...formData, ticketPrice: parseFloat(e.target.value) || 0 })}
                                        className="w-full px-3 py-2 border border-purple-300 rounded-lg text-sm focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                                        required min="0.01" step="0.01"
                                    />
                                    {formData.ticketPrice > 0 && formData.price > 0 && (
                                        <p className="text-xs text-purple-600 mt-1">
                                            Total tickets: {Math.floor(formData.price / formData.ticketPrice)}
                                        </p>
                                    )}
                                </div>
                            </div>
                        )}

                        {/* Characteristics */}
                        <div className="bg-gray-50 rounded-lg p-4">
                            <h3 className="text-sm font-semibold text-gray-900 mb-3">Characteristics</h3>
                            <div className="space-y-2 mb-3">
                                {formData.characteristics.map((char, index) => (
                                    <div key={index} className="flex items-center gap-2 bg-white p-2 rounded border border-gray-200">
                                        <span className="text-sm text-gray-700 flex-1">
                                            <strong>{char.key}:</strong> {char.value}
                                        </span>
                                        <button
                                            type="button"
                                            onClick={() => removeCharacteristic(index)}
                                            className="w-6 h-6 flex items-center justify-center text-red-600 hover:bg-red-50 rounded cursor-pointer"
                                        >
                                            <i className="ri-close-line"></i>
                                        </button>
                                    </div>
                                ))}
                            </div>
                            <div className="flex gap-2">
                                <input
                                    type="text"
                                    placeholder="Key (e.g., Brand)"
                                    value={newChar.key}
                                    onChange={(e) => setNewChar({ ...newChar, key: e.target.value })}
                                    className="flex-1 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                />
                                <input
                                    type="text"
                                    placeholder="Value (e.g., Apple)"
                                    value={newChar.value}
                                    onChange={(e) => setNewChar({ ...newChar, value: e.target.value })}
                                    className="flex-1 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                />
                                <button
                                    type="button"
                                    onClick={addCharacteristic}
                                    className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                                >
                                    <i className="ri-add-line"></i>
                                </button>
                            </div>
                        </div>

                        {/* Active toggle */}
                        <div className="flex items-center gap-2">
                            <input
                                type="checkbox"
                                id="isActive"
                                checked={formData.isActive}
                                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                                className="w-4 h-4 text-teal-600 border-gray-300 rounded focus:ring-teal-500 cursor-pointer"
                            />
                            <label htmlFor="isActive" className="text-sm font-medium text-gray-700 cursor-pointer">Active</label>
                        </div>
                    </div>

                    <div className="flex items-center gap-3 mt-6 pt-6 border-t border-gray-200">
                        <button
                            type="submit"
                            className="px-6 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                        >
                            {lot ? 'Update Lot' : 'Create Lot'}
                        </button>
                        <button
                            type="button"
                            onClick={onClose}
                            className="px-6 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap"
                        >
                            Cancel
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
