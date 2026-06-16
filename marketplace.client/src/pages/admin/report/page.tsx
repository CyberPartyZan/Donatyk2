import { useState } from 'react';
import { mockReports } from '@/mocks/reports';
import { mockGoals } from '@/mocks/goals';

interface ReceiptDocument {
    name: string;
    url: string;
    type: string;
    size: string;
}

interface Report {
    id: string;
    goalId: string;
    goalTitle: string;
    organizationName: string;
    moneyBudget: number;
    moneyRaised: number;
    moneySpent: number;
    spentOn: string;
    spentAt: string;
    direction: string;
    status: string;
    youtubeUrl: string;
    receiptDocuments: ReceiptDocument[];
}

export default function ReportsAdmin() {
    const [reports] = useState<Report[]>(mockReports.sort((a, b) => new Date(b.spentAt).getTime() - new Date(a.spentAt).getTime()));
    const [searchQuery, setSearchQuery] = useState('');
    const [activeSearchQuery, setActiveSearchQuery] = useState('');
    const [directionFilter, setDirectionFilter] = useState('All');
    const [statusFilter, setStatusFilter] = useState('All');
    const [dateFrom, setDateFrom] = useState('');
    const [dateTo, setDateTo] = useState('');
    const [selectedReport, setSelectedReport] = useState<Report | null>(null);

    const allDirections = Array.from(new Set(reports.map(r => r.direction)));
    const totalSpent = reports.reduce((sum, r) => sum + r.moneySpent, 0);
    const totalBudget = reports.reduce((sum, r) => sum + r.moneyBudget, 0);

    const handleSearch = () => {
        setActiveSearchQuery(searchQuery);
    };

    const filtered = reports.filter(r => {
        const matchesSearch = r.goalTitle.toLowerCase().includes(activeSearchQuery.toLowerCase()) ||
            r.organizationName.toLowerCase().includes(activeSearchQuery.toLowerCase()) ||
            r.spentOn.toLowerCase().includes(activeSearchQuery.toLowerCase());
        const matchesDir = directionFilter === 'All' || r.direction === directionFilter;
        const matchesStatus = statusFilter === 'All' || r.status === statusFilter;
        const matchesDateFrom = !dateFrom || new Date(r.spentAt) >= new Date(dateFrom);
        const matchesDateTo = !dateTo || new Date(r.spentAt) <= new Date(dateTo);
        return matchesSearch && matchesDir && matchesStatus && matchesDateFrom && matchesDateTo;
    });

    const getYoutubeEmbedUrl = (url: string) => {
        const match = url.match(/(?:youtube\.com\/watch\?v=|youtu\.be\/)([a-zA-Z0-9_-]{11})/);
        if (match) return 'https://www.youtube.com/embed/' + match[1];
        return url;
    };

    return (
        <div className="p-8">
            <div className="max-w-5xl mx-auto space-y-6">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">Reports</h1>
                    <p className="text-sm text-gray-600 mt-1">Track money spent across all charitable goals</p>
                </div>

                {/* Stats */}
                <div className="grid grid-cols-3 gap-4">
                    <div className="bg-white rounded-lg border border-gray-200 p-5">
                        <p className="text-sm text-gray-600 mb-1">Total Spent</p>
                        <p className="text-3xl font-bold text-emerald-600">${totalSpent.toLocaleString()}</p>
                    </div>
                    <div className="bg-white rounded-lg border border-gray-200 p-5">
                        <p className="text-sm text-gray-600 mb-1">Total Budget Allocated</p>
                        <p className="text-3xl font-bold text-gray-900">${totalBudget.toLocaleString()}</p>
                    </div>
                    <div className="bg-white rounded-lg border border-gray-200 p-5">
                        <p className="text-sm text-gray-600 mb-1">Total Expenditures</p>
                        <p className="text-3xl font-bold text-teal-600">{reports.length}</p>
                    </div>
                </div>

                {/* Filters */}
                <div className="bg-white rounded-lg border border-gray-200">
                    <div className="px-6 py-4 border-b border-gray-200">
                        <div className="flex items-center gap-3 flex-wrap">
                            <div className="flex-1 min-w-[200px]">
                                <div className="relative flex items-center gap-2">
                                    <div className="relative flex-1">
                                        <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                                        <input
                                            type="text"
                                            placeholder="Search by goal, organization, or description..."
                                            value={searchQuery}
                                            onChange={(e) => setSearchQuery(e.target.value)}
                                            onKeyDown={(e) => { if (e.key === 'Enter') handleSearch(); }}
                                            className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                        />
                                    </div>
                                    <button
                                        onClick={handleSearch}
                                        className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                                    >
                                        <i className="ri-search-line"></i>
                                        Search
                                    </button>
                                </div>
                            </div>
                            <select
                                value={directionFilter}
                                onChange={(e) => setDirectionFilter(e.target.value)}
                                className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                            >
                                <option value="All">All Directions</option>
                                {allDirections.map(d => (
                                    <option key={d} value={d}>{d}</option>
                                ))}
                            </select>
                            <select
                                value={statusFilter}
                                onChange={(e) => setStatusFilter(e.target.value)}
                                className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                            >
                                <option value="All">All Status</option>
                                <option value="Completed">Completed</option>
                                <option value="In Progress">In Progress</option>
                            </select>
                            <div className="flex items-center gap-2">
                                <input
                                    type="date"
                                    value={dateFrom}
                                    onChange={(e) => setDateFrom(e.target.value)}
                                    className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                    placeholder="From"
                                />
                                <span className="text-sm text-gray-400">to</span>
                                <input
                                    type="date"
                                    value={dateTo}
                                    onChange={(e) => setDateTo(e.target.value)}
                                    className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                    placeholder="To"
                                />
                            </div>
                        </div>
                    </div>
                    <div className="p-6">
                        {filtered.length === 0 ? (
                            <div className="text-center py-12">
                                <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                                    <i className="ri-file-chart-line text-5xl text-gray-300"></i>
                                </div>
                                <p className="text-gray-500 text-sm">No reports found</p>
                            </div>
                        ) : (
                            <div className="space-y-4">
                                {filtered.map(report => {
                                    const goal = mockGoals.find(g => g.id === report.goalId);
                                    const pctSpent = Math.round((report.moneySpent / report.moneyBudget) * 100);
                                    return (
                                        <div key={report.id} className="bg-gray-50 rounded-lg border border-gray-200 p-5 hover:border-teal-200 transition-colors">
                                            <div className="flex items-start justify-between mb-3">
                                                <div>
                                                    <div className="flex items-center gap-2 mb-1">
                                                        <span className="text-xs text-gray-400">{report.organizationName}</span>
                                                        <span className={`px-2 py-0.5 text-xs font-medium rounded-full ${report.status === 'Completed' ? 'bg-emerald-100 text-emerald-700' : 'bg-amber-100 text-amber-700'
                                                            }`}>
                                                            {report.status}
                                                        </span>
                                                    </div>
                                                    <h3 className="text-lg font-semibold text-gray-900">{report.goalTitle}</h3>
                                                    <div className="flex items-center gap-2 mt-1">
                                                        <span className="text-xs text-gray-500 px-2 py-0.5 bg-gray-100 rounded-full">{report.direction}</span>
                                                    </div>
                                                </div>
                                                <div className="text-right">
                                                    <p className="text-sm text-gray-500">Spent</p>
                                                    <p className="text-xl font-bold text-emerald-600">${report.moneySpent.toLocaleString()}</p>
                                                </div>
                                            </div>

                                            <p className="text-sm text-gray-600 mb-3 line-clamp-2">{report.spentOn}</p>

                                            <div className="mb-3">
                                                <div className="flex items-center justify-between mb-1.5">
                                                    <span className="text-xs text-gray-500">Budget utilization</span>
                                                    <span className="text-xs font-medium text-gray-700">{pctSpent}%</span>
                                                </div>
                                                <div className="w-full h-2 bg-gray-200 rounded-full overflow-hidden">
                                                    <div className={`h-full rounded-full ${report.status === 'Completed' ? 'bg-emerald-500' : 'bg-teal-500'}`} style={{ width: `${Math.min(pctSpent, 100)}%` }}></div>
                                                </div>
                                            </div>

                                            <div className="flex items-center justify-between">
                                                <p className="text-xs text-gray-400 flex items-center gap-1">
                                                    <i className="ri-calendar-line"></i>
                                                    {new Date(report.spentAt).toLocaleDateString()}
                                                </p>
                                                <div className="flex items-center gap-3">
                                                    {report.receiptDocuments.length > 0 && (
                                                        <span className="text-xs text-gray-500 flex items-center gap-1">
                                                            <i className="ri-file-pdf-line text-red-500"></i>
                                                            {report.receiptDocuments.length} document{report.receiptDocuments.length !== 1 ? 's' : ''}
                                                        </span>
                                                    )}
                                                    {report.youtubeUrl && (
                                                        <span className="text-xs text-red-500 flex items-center gap-1">
                                                            <i className="ri-youtube-line"></i>Video
                                                        </span>
                                                    )}
                                                    <button
                                                        onClick={() => setSelectedReport(report)}
                                                        className="text-xs text-teal-600 hover:text-teal-700 cursor-pointer flex items-center gap-1"
                                                    >
                                                        <i className="ri-eye-line"></i>View Details
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </div>
                </div>

                {/* Report Detail Modal */}
                {selectedReport && (
                    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                        <div className="bg-white rounded-lg shadow-xl max-w-lg w-full p-6 max-h-[90vh] overflow-y-auto">
                            <div className="flex items-center justify-between mb-4">
                                <h3 className="text-lg font-semibold text-gray-900">Expenditure Report</h3>
                                <button
                                    onClick={() => setSelectedReport(null)}
                                    className="w-8 h-8 flex items-center justify-center text-gray-400 hover:text-gray-600 cursor-pointer"
                                >
                                    <i className="ri-close-line text-xl"></i>
                                </button>
                            </div>

                            <div className="space-y-4">
                                <div className="bg-gray-50 rounded-lg p-4">
                                    <p className="text-xs text-gray-500 mb-1">Goal</p>
                                    <p className="text-base font-semibold text-gray-900">{selectedReport.goalTitle}</p>
                                    <p className="text-sm text-gray-600 mt-1">{selectedReport.organizationName}</p>
                                    <span className="inline-block mt-2 text-xs text-gray-500 px-2 py-0.5 bg-gray-100 rounded-full">{selectedReport.direction}</span>
                                </div>

                                <div className="grid grid-cols-3 gap-3">
                                    <div className="bg-gray-50 rounded-lg p-3 text-center">
                                        <p className="text-xs text-gray-500 mb-1">Budget</p>
                                        <p className="text-lg font-bold text-gray-900">${selectedReport.moneyBudget.toLocaleString()}</p>
                                    </div>
                                    <div className="bg-gray-50 rounded-lg p-3 text-center">
                                        <p className="text-xs text-gray-500 mb-1">Raised</p>
                                        <p className="text-lg font-bold text-teal-600">${selectedReport.moneyRaised.toLocaleString()}</p>
                                    </div>
                                    <div className="bg-emerald-50 rounded-lg p-3 text-center">
                                        <p className="text-xs text-emerald-600 mb-1">Spent</p>
                                        <p className="text-lg font-bold text-emerald-600">${selectedReport.moneySpent.toLocaleString()}</p>
                                    </div>
                                </div>

                                <div className="bg-gray-50 rounded-lg p-4">
                                    <p className="text-xs text-gray-500 mb-2">Description of Spending</p>
                                    <p className="text-sm text-gray-700 leading-relaxed">{selectedReport.spentOn}</p>
                                </div>

                                <div className="flex items-center gap-3 text-sm">
                                    <span className="flex items-center gap-1 text-gray-500">
                                        <i className="ri-calendar-line"></i>
                                        {new Date(selectedReport.spentAt).toLocaleDateString()}
                                    </span>
                                    <span className={`px-2 py-0.5 text-xs font-medium rounded-full ${selectedReport.status === 'Completed' ? 'bg-emerald-100 text-emerald-700' : 'bg-amber-100 text-amber-700'
                                        }`}>
                                        {selectedReport.status}
                                    </span>
                                </div>

                                {selectedReport.youtubeUrl && (
                                    <div>
                                        <p className="text-sm font-medium text-gray-700 mb-2 flex items-center gap-1.5">
                                            <i className="ri-youtube-line text-red-500"></i>
                                            Approvals & Thanks Video
                                        </p>
                                        <div className="aspect-video rounded-lg overflow-hidden border border-gray-200">
                                            <iframe
                                                src={getYoutubeEmbedUrl(selectedReport.youtubeUrl)}
                                                className="w-full h-full"
                                                title="Approvals and Thanks Video"
                                                frameBorder="0"
                                                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                                                allowFullScreen
                                            ></iframe>
                                        </div>
                                    </div>
                                )}

                                {selectedReport.receiptDocuments.length > 0 && (
                                    <div>
                                        <p className="text-sm font-medium text-gray-700 mb-2">Receipt Documents</p>
                                        <div className="space-y-2">
                                            {selectedReport.receiptDocuments.map((doc, idx) => (
                                                <a key={idx} href={doc.url} className="flex items-center gap-2 px-3 py-2 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors cursor-pointer">
                                                    <i className="ri-file-pdf-line text-red-500"></i>
                                                    <span className="text-sm text-gray-700 flex-1">{doc.name}</span>
                                                    <span className="text-xs text-gray-400">{doc.size}</span>
                                                </a>
                                            ))}
                                        </div>
                                    </div>
                                )}
                            </div>

                            <div className="mt-6">
                                <button
                                    onClick={() => setSelectedReport(null)}
                                    className="w-full px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer"
                                >
                                    Close
                                </button>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}