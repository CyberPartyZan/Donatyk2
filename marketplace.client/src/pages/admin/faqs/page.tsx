import { useState } from 'react';
import { mockFaqs } from '@/mocks/faqs';

interface Faq {
    id: string;
    question: string;
    answer: string;
    category: string;
    order: number;
}

export default function FaqsAdmin() {
    const [faqs, setFaqs] = useState<Faq[]>(mockFaqs);
    const [searchQuery, setSearchQuery] = useState('');
    const [activeSearchQuery, setActiveSearchQuery] = useState('');
    const [categoryFilter, setCategoryFilter] = useState('All');
    const [isAddModalOpen, setIsAddModalOpen] = useState(false);
    const [editingFaq, setEditingFaq] = useState<Faq | null>(null);
    const [formQuestion, setFormQuestion] = useState('');
    const [formAnswer, setFormAnswer] = useState('');
    const [formCategory, setFormCategory] = useState('General');
    const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null);

    const categories = ['All', ...Array.from(new Set(faqs.map(f => f.category)))];

    const handleSearch = () => {
        setActiveSearchQuery(searchQuery.trim());
    };

    const filteredFaqs = faqs.filter(faq => {
        const matchesSearch = !activeSearchQuery
            || faq.question.toLowerCase().includes(activeSearchQuery.toLowerCase())
            || faq.answer.toLowerCase().includes(activeSearchQuery.toLowerCase());
        const matchesCategory = categoryFilter === 'All' || faq.category === categoryFilter;
        return matchesSearch && matchesCategory;
    });

    const openAddModal = () => {
        setEditingFaq(null);
        setFormQuestion('');
        setFormAnswer('');
        setFormCategory('General');
        setIsAddModalOpen(true);
    };

    const openEditModal = (faq: Faq) => {
        setEditingFaq(faq);
        setFormQuestion(faq.question);
        setFormAnswer(faq.answer);
        setFormCategory(faq.category);
        setIsAddModalOpen(true);
    };

    const handleSave = () => {
        if (!formQuestion.trim() || !formAnswer.trim()) return;

        if (editingFaq) {
            setFaqs(faqs.map(f => f.id === editingFaq.id
                ? { ...f, question: formQuestion.trim(), answer: formAnswer.trim(), category: formCategory }
                : f
            ));
        } else {
            const newFaq: Faq = {
                id: 'faq-' + Date.now(),
                question: formQuestion.trim(),
                answer: formAnswer.trim(),
                category: formCategory,
                order: faqs.length + 1,
            };
            setFaqs([newFaq, ...faqs]);
        }
        setIsAddModalOpen(false);
    };

    const handleDelete = (id: string) => {
        setFaqs(faqs.filter(f => f.id !== id));
        setDeleteConfirmId(null);
    };

    return (
        <div>
            <div className="mb-8">
                <h1 className="text-3xl font-bold text-gray-900 mb-2">FAQs Management</h1>
                <p className="text-sm text-gray-600">Manage frequently asked questions displayed on the homepage</p>
            </div>

            {/* Stats */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
                <div className="bg-white rounded-lg border border-gray-200 p-4">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-teal-100 rounded-lg flex items-center justify-center">
                            <i className="ri-question-answer-line text-teal-600 text-lg"></i>
                        </div>
                        <div>
                            <p className="text-sm text-gray-600">Total FAQs</p>
                            <p className="text-2xl font-bold text-gray-900">{faqs.length}</p>
                        </div>
                    </div>
                </div>
                <div className="bg-white rounded-lg border border-gray-200 p-4">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-amber-100 rounded-lg flex items-center justify-center">
                            <i className="ri-folders-line text-amber-600 text-lg"></i>
                        </div>
                        <div>
                            <p className="text-sm text-gray-600">Categories</p>
                            <p className="text-2xl font-bold text-gray-900">{categories.length - 1}</p>
                        </div>
                    </div>
                </div>
                <div className="bg-white rounded-lg border border-gray-200 p-4">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-emerald-100 rounded-lg flex items-center justify-center">
                            <i className="ri-eye-line text-emerald-600 text-lg"></i>
                        </div>
                        <div>
                            <p className="text-sm text-gray-600">Visible on Homepage</p>
                            <p className="text-2xl font-bold text-gray-900">{faqs.length}</p>
                        </div>
                    </div>
                </div>
            </div>

            {/* Actions Bar */}
            <div className="bg-white rounded-lg border border-gray-200 p-4 mb-6">
                <div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
                    <div className="flex items-center gap-3 flex-wrap w-full sm:w-auto">
                        <div className="relative flex-1 sm:flex-none sm:w-64">
                            <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                            <input
                                type="text"
                                value={searchQuery}
                                onChange={(e) => setSearchQuery(e.target.value)}
                                onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                                placeholder="Search FAQs..."
                                className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                            />
                        </div>
                        <button
                            onClick={handleSearch}
                            className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                        >
                            <i className="ri-search-line mr-1.5"></i>Search
                        </button>
                        <select
                            value={categoryFilter}
                            onChange={(e) => setCategoryFilter(e.target.value)}
                            className="px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 bg-white cursor-pointer"
                        >
                            {categories.map(cat => (
                                <option key={cat} value={cat}>{cat}</option>
                            ))}
                        </select>
                    </div>
                    <button
                        onClick={openAddModal}
                        className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                    >
                        <i className="ri-add-line mr-2"></i>
                        Add FAQ
                    </button>
                </div>
            </div>

            {/* FAQ List */}
            {filteredFaqs.length > 0 ? (
                <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
                    <table className="w-full">
                        <thead>
                            <tr className="bg-gray-50 border-b border-gray-200">
                                <th className="text-left px-6 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wider">#</th>
                                <th className="text-left px-6 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wider">Question</th>
                                <th className="text-left px-6 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wider">Category</th>
                                <th className="text-left px-6 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wider">Answer Preview</th>
                                <th className="text-right px-6 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                            {filteredFaqs.map((faq, idx) => (
                                <tr key={faq.id} className="hover:bg-gray-50 transition-colors">
                                    <td className="px-6 py-4 text-sm text-gray-400">{idx + 1}</td>
                                    <td className="px-6 py-4 text-sm font-medium text-gray-900 max-w-xs truncate">
                                        {faq.question}
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className="inline-block px-2.5 py-0.5 bg-teal-100 text-teal-700 text-xs font-medium rounded-full whitespace-nowrap">
                                            {faq.category}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 text-sm text-gray-500 max-w-sm truncate">
                                        {faq.answer}
                                    </td>
                                    <td className="px-6 py-4 text-right">
                                        <div className="flex items-center justify-end gap-2">
                                            <button
                                                onClick={() => openEditModal(faq)}
                                                className="w-8 h-8 flex items-center justify-center rounded-md hover:bg-teal-50 text-gray-400 hover:text-teal-600 transition-colors cursor-pointer"
                                                title="Edit"
                                            >
                                                <i className="ri-edit-line"></i>
                                            </button>
                                            <button
                                                onClick={() => setDeleteConfirmId(faq.id)}
                                                className="w-8 h-8 flex items-center justify-center rounded-md hover:bg-red-50 text-gray-400 hover:text-red-500 transition-colors cursor-pointer"
                                                title="Delete"
                                            >
                                                <i className="ri-delete-bin-line"></i>
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            ) : (
                <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
                    <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                        <i className="ri-question-answer-line text-gray-400 text-2xl"></i>
                    </div>
                    <h3 className="text-lg font-semibold text-gray-900 mb-2">No FAQs found</h3>
                    <p className="text-sm text-gray-600 mb-4">
                        {activeSearchQuery || categoryFilter !== 'All' ? 'Try adjusting your search or filter' : 'Get started by adding your first FAQ'}
                    </p>
                    {!activeSearchQuery && categoryFilter === 'All' && (
                        <button
                            onClick={openAddModal}
                            className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                        >
                            <i className="ri-add-line mr-2"></i>
                            Add FAQ
                        </button>
                    )}
                </div>
            )}

            {/* Add/Edit Modal */}
            {isAddModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
                    <div className="bg-white rounded-2xl w-full max-w-lg mx-4 shadow-xl">
                        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200">
                            <h2 className="text-lg font-semibold text-gray-900">
                                {editingFaq ? 'Edit FAQ' : 'Add New FAQ'}
                            </h2>
                            <button
                                onClick={() => setIsAddModalOpen(false)}
                                className="w-8 h-8 rounded-full hover:bg-gray-100 flex items-center justify-center cursor-pointer"
                            >
                                <i className="ri-close-line text-gray-500"></i>
                            </button>
                        </div>

                        <div className="px-6 py-5 space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1.5">
                                    Question <span className="text-red-500">*</span>
                                </label>
                                <input
                                    type="text"
                                    value={formQuestion}
                                    onChange={(e) => setFormQuestion(e.target.value)}
                                    placeholder="Enter the question..."
                                    className="w-full px-3.5 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1.5">
                                    Answer <span className="text-red-500">*</span>
                                </label>
                                <textarea
                                    value={formAnswer}
                                    onChange={(e) => setFormAnswer(e.target.value)}
                                    maxLength={500}
                                    rows={4}
                                    placeholder="Enter the answer..."
                                    className="w-full px-3.5 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 resize-none"
                                />
                                <p className="text-xs text-gray-400 mt-1 text-right">{formAnswer.length}/500</p>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1.5">Category</label>
                                <select
                                    value={formCategory}
                                    onChange={(e) => setFormCategory(e.target.value)}
                                    className="w-full px-3.5 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 bg-white cursor-pointer"
                                >
                                    {categories.filter(c => c !== 'All').map(cat => (
                                        <option key={cat} value={cat}>{cat}</option>
                                    ))}
                                    <option value="General">General</option>
                                </select>
                            </div>
                        </div>

                        <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-gray-200">
                            <button
                                onClick={() => setIsAddModalOpen(false)}
                                className="px-4 py-2 text-sm font-medium text-gray-600 hover:text-gray-900 transition-colors cursor-pointer whitespace-nowrap"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleSave}
                                disabled={!formQuestion.trim() || !formAnswer.trim()}
                                className="px-5 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors cursor-pointer whitespace-nowrap"
                            >
                                {editingFaq ? 'Save Changes' : 'Add FAQ'}
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Delete Confirmation */}
            {deleteConfirmId && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
                    <div className="bg-white rounded-2xl w-full max-w-sm mx-4 shadow-xl p-6">
                        <div className="text-center mb-5">
                            <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center mx-auto mb-3">
                                <i className="ri-delete-bin-line text-red-500 text-xl"></i>
                            </div>
                            <h3 className="text-lg font-semibold text-gray-900 mb-1">Delete FAQ</h3>
                            <p className="text-sm text-gray-500">
                                Are you sure you want to delete this FAQ? This action cannot be undone.
                            </p>
                        </div>
                        <div className="flex items-center gap-3">
                            <button
                                onClick={() => setDeleteConfirmId(null)}
                                className="flex-1 px-4 py-2.5 text-sm font-medium text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors cursor-pointer whitespace-nowrap"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={() => handleDelete(deleteConfirmId)}
                                className="flex-1 px-4 py-2.5 text-sm font-medium text-white bg-red-500 rounded-lg hover:bg-red-600 transition-colors cursor-pointer whitespace-nowrap"
                            >
                                Delete
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}