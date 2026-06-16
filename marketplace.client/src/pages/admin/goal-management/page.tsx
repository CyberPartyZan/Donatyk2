import { useState } from 'react';
import { mockGoals, mockOrganizations } from '@/mocks/goals';

interface Organization {
    id: string;
    name: string;
    contactEmail: string;
    contactPhone: string;
    description: string;
    avatarImage: string;
    createdAt: string;
    totalGoals: number;
    completedGoals: number;
}

interface GoalDocument {
    name: string;
    url: string;
    type: string;
    size: string;
}

interface ProofDocument {
    name: string;
    url: string;
    type: string;
    size: string;
    uploadedAt: string;
}

interface Goal {
    id: string;
    organizationId: string;
    organizationName: string;
    title: string;
    explanation: string;
    approvementDocuments: GoalDocument[];
    timeLimitStart: string | null;
    timeLimitEnd: string | null;
    moneyBudget: number;
    moneyRaised: number;
    status: 'Active' | 'Reached';
    createdAt: string;
    linkedLots: string[];
    proofDocuments?: ProofDocument[];
    processedAt?: string;
    youtubeUrl?: string;
}

export default function GoalManagementAdmin() {
    const [organizations] = useState<Organization[]>(mockOrganizations);
    const [goals, setGoals] = useState<Goal[]>(mockGoals.map(g => ({
        ...g,
        proofDocuments: g.status === 'Reached' ? (g as any).proofDocuments || [] : undefined,
        processedAt: g.status === 'Reached' ? (g as any).processedAt || undefined : undefined,
    })));
    const [activeTab, setActiveTab] = useState<'fulfilled' | 'organizations'>('fulfilled');
    const [selectedOrgId, setSelectedOrgId] = useState<string | null>(null);
    const [processingGoal, setProcessingGoal] = useState<Goal | null>(null);
    const [uploadedFiles, setUploadedFiles] = useState<ProofDocument[]>([]);
    const [youtubeUrl, setYoutubeUrl] = useState('');

    const [fulfilledSearch, setFulfilledSearch] = useState('');
    const [fulfilledActiveSearch, setFulfilledActiveSearch] = useState('');
    const [orgSearch, setOrgSearch] = useState('');
    const [orgActiveSearch, setOrgActiveSearch] = useState('');

    const reachedGoals = goals.filter(g => g.status === 'Reached');
    const processedGoals = goals.filter(g => g.status === 'Reached' && g.processedAt);
    const unprocessedGoals = goals.filter(g => g.status === 'Reached' && !g.processedAt);

    const filteredReachedGoals = reachedGoals.filter(g =>
        g.title.toLowerCase().includes(fulfilledActiveSearch.toLowerCase()) ||
        g.organizationName.toLowerCase().includes(fulfilledActiveSearch.toLowerCase())
    );

    const selectedOrg = selectedOrgId ? organizations.find(o => o.id === selectedOrgId) : null;
    const orgGoals = selectedOrgId ? goals.filter(g => g.organizationId === selectedOrgId) : [];

    const filteredOrganizations = organizations.filter(o =>
        o.name.toLowerCase().includes(orgActiveSearch.toLowerCase()) ||
        o.description.toLowerCase().includes(orgActiveSearch.toLowerCase())
    );

    const handleOpenProcess = (goal: Goal) => {
        setProcessingGoal(goal);
        setUploadedFiles(goal.proofDocuments ? [...goal.proofDocuments] : []);
        setYoutubeUrl('');
    };

    const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = e.target.files;
        if (!files) return;
        const newDocs: ProofDocument[] = Array.from(files).map((f) => ({
            name: f.name,
            url: '#',
            type: f.name.split('.').pop()?.toUpperCase() || 'FILE',
            size: `${(f.size / 1024 / 1024).toFixed(1)} MB`,
            uploadedAt: new Date().toISOString(),
        }));
        setUploadedFiles([...uploadedFiles, ...newDocs]);
    };

    const handleRemoveFile = (index: number) => {
        setUploadedFiles(uploadedFiles.filter((_, i) => i !== index));
    };

    const handleCompleteProcess = () => {
        if (!processingGoal) return;
        setGoals(goals.map(g =>
            g.id === processingGoal.id
                ? { ...g, proofDocuments: uploadedFiles, processedAt: new Date().toISOString(), youtubeUrl: youtubeUrl || undefined }
                : g
        ));
        setProcessingGoal(null);
        setUploadedFiles([]);
        setYoutubeUrl('');
    };

    const getOrgAvatar = (orgId: string) => {
        const org = organizations.find(o => o.id === orgId);
        return org?.avatarImage || '';
    };

    const getProgressPercent = (raised: number, budget: number) => Math.min(Math.round((raised / budget) * 100), 100);

    return (
        <div className="p-8">
            <div className="max-w-5xl mx-auto space-y-6">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">Goal Management</h1>
                    <p className="text-sm text-gray-600 mt-1">Review and process reached goals, browse all organizations</p>
                </div>

                {/* Tabs */}
                <div className="flex items-center gap-2 bg-gray-100 p-1 rounded-full w-fit">
                    <button
                        onClick={() => { setActiveTab('fulfilled'); setSelectedOrgId(null); }}
                        className={`px-5 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${activeTab === 'fulfilled' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'
                            }`}
                    >
                        <i className="ri-check-double-line mr-2"></i>Fulfilled Goals
                    </button>
                    <button
                        onClick={() => { setActiveTab('organizations'); }}
                        className={`px-5 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${activeTab === 'organizations' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'
                            }`}
                    >
                        <i className="ri-building-line mr-2"></i>Organizations
                    </button>
                </div>

                {/* Fulfilled Goals Tab */}
                {activeTab === 'fulfilled' && (
                    <>
                        {/* Stats */}
                        <div className="grid grid-cols-3 gap-4">
                            <div className="bg-white rounded-lg border border-gray-200 p-4">
                                <p className="text-sm text-gray-600 mb-1">Reached Goals</p>
                                <p className="text-2xl font-bold text-emerald-700">{reachedGoals.length}</p>
                            </div>
                            <div className="bg-amber-50 rounded-lg border border-amber-200 p-4">
                                <p className="text-sm text-amber-600 mb-1">Unprocessed</p>
                                <p className="text-2xl font-bold text-amber-900">{unprocessedGoals.length}</p>
                            </div>
                            <div className="bg-emerald-50 rounded-lg border border-emerald-200 p-4">
                                <p className="text-sm text-emerald-600 mb-1">Processed</p>
                                <p className="text-2xl font-bold text-emerald-900">{processedGoals.length}</p>
                            </div>
                        </div>

                        {/* Search bar for fulfilled goals */}
                        <div className="flex items-center gap-2 mb-4">
                            <div className="flex-1 relative">
                                <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                                <input
                                    type="text"
                                    placeholder="Search goals by title or organization..."
                                    value={fulfilledSearch}
                                    onChange={(e) => setFulfilledSearch(e.target.value)}
                                    onKeyDown={(e) => { if (e.key === 'Enter') setFulfilledActiveSearch(fulfilledSearch); }}
                                    className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                />
                            </div>
                            <button
                                onClick={() => setFulfilledActiveSearch(fulfilledSearch)}
                                className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                            >
                                <i className="ri-search-line"></i>
                                Search
                            </button>
                        </div>

                        {filteredReachedGoals.length === 0 ? (
                            <div className="text-center py-16 bg-white rounded-lg border border-gray-200">
                                <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                                    <i className="ri-flag-line text-5xl text-gray-300"></i>
                                </div>
                                <p className="text-gray-500 text-sm">No reached goals yet</p>
                            </div>
                        ) : (
                            <div className="space-y-4">
                                {filteredReachedGoals.map(goal => {
                                    const pct = getProgressPercent(goal.moneyRaised, goal.moneyBudget);
                                    const isProcessed = !!goal.processedAt;
                                    return (
                                        <div key={goal.id} className={`bg-white rounded-lg border p-5 ${isProcessed ? 'border-emerald-200 bg-emerald-50/20' : 'border-amber-200'}`}>
                                            <div className="flex items-start gap-4">
                                                <div className="w-14 h-14 rounded-lg bg-gray-100 overflow-hidden flex-shrink-0 border border-gray-200">
                                                    <img src={getOrgAvatar(goal.organizationId)} alt={goal.organizationName} className="w-full h-full object-cover object-top" />
                                                </div>
                                                <div className="flex-1 min-w-0">
                                                    <div className="flex items-start justify-between mb-2">
                                                        <div>
                                                            <div className="flex items-center gap-2 mb-1">
                                                                <span className="text-xs text-gray-400">{goal.organizationName}</span>
                                                                <span className={`px-2 py-0.5 text-xs font-medium rounded-full ${isProcessed ? 'bg-emerald-100 text-emerald-700' : 'bg-amber-100 text-amber-700'}`}>
                                                                    {isProcessed ? 'Processed' : 'Ready to Process'}
                                                                </span>
                                                            </div>
                                                            <h3 className="text-lg font-semibold text-gray-900">{goal.title}</h3>
                                                        </div>
                                                        {!isProcessed && (
                                                            <button
                                                                onClick={() => handleOpenProcess(goal)}
                                                                className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                                                            >
                                                                <i className="ri-check-double-line"></i>Process Goal
                                                            </button>
                                                        )}
                                                    </div>
                                                    <p className="text-sm text-gray-600 line-clamp-2 mb-3">{goal.explanation}</p>

                                                    <div className="mb-3">
                                                        <div className="flex items-center justify-between mb-1.5">
                                                            <span className="text-sm font-semibold text-gray-900">${goal.moneyRaised.toLocaleString()} raised</span>
                                                            <span className="text-sm text-gray-500">of ${goal.moneyBudget.toLocaleString()} budget</span>
                                                        </div>
                                                        <div className="w-full h-2 bg-gray-200 rounded-full overflow-hidden">
                                                            <div className="h-full bg-emerald-500 rounded-full" style={{ width: `${pct}%` }}></div>
                                                        </div>
                                                    </div>

                                                    {goal.approvementDocuments.length > 0 && (
                                                        <div className="flex flex-wrap gap-2 mb-3">
                                                            {goal.approvementDocuments.map((doc, idx) => (
                                                                <a key={idx} href={doc.url} className="flex items-center gap-1 px-2 py-1 bg-gray-50 rounded text-xs text-gray-600 hover:bg-gray-100 transition-colors cursor-pointer">
                                                                    <i className="ri-file-pdf-line text-red-500"></i>
                                                                    {doc.name} ({doc.size})
                                                                </a>
                                                            ))}
                                                        </div>
                                                    )}

                                                    <div className="flex items-center gap-2 text-xs text-gray-500 mb-2">
                                                        <i className="ri-link-m"></i>
                                                        {goal.linkedLots.length} linked lot{goal.linkedLots.length !== 1 ? 's' : ''}
                                                    </div>

                                                    {isProcessed && goal.proofDocuments && goal.proofDocuments.length > 0 && (
                                                        <div className="mt-3 pt-3 border-t border-gray-100">
                                                            <p className="text-xs font-medium text-gray-700 mb-2 flex items-center gap-1">
                                                                <i className="ri-camera-line text-emerald-600"></i>Proof Documents
                                                            </p>
                                                            <div className="flex flex-wrap gap-2">
                                                                {goal.proofDocuments.map((doc, idx) => (
                                                                    <a key={idx} href={doc.url} className="flex items-center gap-1.5 px-2.5 py-1.5 bg-emerald-50 rounded-lg text-xs text-emerald-700 hover:bg-emerald-100 transition-colors cursor-pointer">
                                                                        <i className="ri-image-line"></i>
                                                                        {doc.name}
                                                                        <span className="text-emerald-400">({doc.size})</span>
                                                                    </a>
                                                                ))}
                                                            </div>
                                                            <p className="text-xs text-gray-400 mt-2">
                                                                Processed on {new Date(goal.processedAt!).toLocaleString()}
                                                            </p>
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </>
                )}

                {/* Organizations Tab */}
                {activeTab === 'organizations' && (
                    <>
                        {selectedOrg ? (
                            <>
                                {/* Back button + Org header */}
                                <button
                                    onClick={() => setSelectedOrgId(null)}
                                    className="flex items-center gap-1 text-sm text-gray-600 hover:text-teal-600 transition-colors cursor-pointer"
                                >
                                    <i className="ri-arrow-left-s-line"></i> All Organizations
                                </button>

                                <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
                                    <div className="p-6">
                                        <div className="flex items-start gap-6">
                                            <div className="w-24 h-24 rounded-lg bg-gray-100 overflow-hidden flex-shrink-0 border border-gray-200">
                                                <img src={selectedOrg.avatarImage} alt={selectedOrg.name} className="w-full h-full object-cover object-top" />
                                            </div>
                                            <div className="flex-1">
                                                <h2 className="text-xl font-bold text-gray-900 mb-1">{selectedOrg.name}</h2>
                                                <p className="text-sm text-gray-600 leading-relaxed mb-3">{selectedOrg.description}</p>
                                                <div className="flex items-center gap-4 text-sm text-gray-600">
                                                    <span className="flex items-center gap-1.5">
                                                        <i className="ri-mail-line text-gray-400"></i>{selectedOrg.contactEmail}
                                                    </span>
                                                    <span className="flex items-center gap-1.5">
                                                        <i className="ri-phone-line text-gray-400"></i>{selectedOrg.contactPhone}
                                                    </span>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="grid grid-cols-3 border-t border-gray-200">
                                        <div className="px-6 py-4 border-r border-gray-200">
                                            <div className="text-2xl font-bold text-gray-900">{selectedOrg.totalGoals}</div>
                                            <div className="text-xs text-gray-600 mt-1">Total Goals</div>
                                        </div>
                                        <div className="px-6 py-4 border-r border-gray-200">
                                            <div className="text-2xl font-bold text-emerald-600">{selectedOrg.completedGoals}</div>
                                            <div className="text-xs text-gray-600 mt-1">Completed</div>
                                        </div>
                                        <div className="px-6 py-4">
                                            <div className="text-2xl font-bold text-teal-600">${orgGoals.reduce((s, g) => s + g.moneyRaised, 0).toLocaleString()}</div>
                                            <div className="text-xs text-gray-600 mt-1">Total Raised</div>
                                        </div>
                                    </div>
                                </div>

                                {/* Organization Goals */}
                                <div className="bg-white rounded-lg border border-gray-200">
                                    <div className="px-6 py-4 border-b border-gray-200">
                                        <h3 className="text-base font-semibold text-gray-900">{orgGoals.length} Goal{orgGoals.length !== 1 ? 's' : ''}</h3>
                                    </div>
                                    <div className="p-6">
                                        {orgGoals.length === 0 ? (
                                            <div className="text-center py-12">
                                                <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                                                    <i className="ri-flag-line text-5xl text-gray-300"></i>
                                                </div>
                                                <p className="text-gray-500 text-sm">No goals yet</p>
                                            </div>
                                        ) : (
                                            <div className="grid gap-4">
                                                {orgGoals.map(goal => {
                                                    const pct = getProgressPercent(goal.moneyRaised, goal.moneyBudget);
                                                    return (
                                                        <div key={goal.id} className={`rounded-lg border p-5 ${goal.status === 'Reached' ? 'border-emerald-200 bg-emerald-50/30' : 'border-gray-200'}`}>
                                                            <div className="flex items-start justify-between mb-3">
                                                                <div className="flex-1 min-w-0">
                                                                    <span className={`px-2 py-0.5 text-xs font-medium rounded-full ${goal.status === 'Reached' ? 'bg-emerald-100 text-emerald-700' : 'bg-amber-100 text-amber-700'}`}>
                                                                        {goal.status}
                                                                    </span>
                                                                    <h3 className="text-lg font-semibold text-gray-900 mt-1">{goal.title}</h3>
                                                                    <p className="text-sm text-gray-600 mt-1 line-clamp-2">{goal.explanation}</p>
                                                                </div>
                                                            </div>
                                                            <div className="mb-3">
                                                                <div className="flex items-center justify-between mb-1.5">
                                                                    <span className="text-sm font-semibold text-gray-900">${goal.moneyRaised.toLocaleString()} raised</span>
                                                                    <span className="text-sm text-gray-500">of ${goal.moneyBudget.toLocaleString()}</span>
                                                                </div>
                                                                <div className="w-full h-2.5 bg-gray-200 rounded-full overflow-hidden">
                                                                    <div className={`h-full rounded-full ${goal.status === 'Reached' ? 'bg-emerald-500' : 'bg-teal-500'}`} style={{ width: `${pct}%` }}></div>
                                                                </div>
                                                                <span className="text-xs text-gray-400 mt-1 block">{pct}% funded</span>
                                                            </div>
                                                        </div>
                                                    );
                                                })}
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </>
                        ) : (
                            <>
                                <p className="text-sm text-gray-500">{organizations.length} organization{organizations.length !== 1 ? 's' : ''}</p>

                                {/* Search bar for organizations */}
                                <div className="flex items-center gap-2 mb-4">
                                    <div className="flex-1 relative">
                                        <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                                        <input
                                            type="text"
                                            placeholder="Search organizations by name or description..."
                                            value={orgSearch}
                                            onChange={(e) => setOrgSearch(e.target.value)}
                                            onKeyDown={(e) => { if (e.key === 'Enter') setOrgActiveSearch(orgSearch); }}
                                            className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                        />
                                    </div>
                                    <button
                                        onClick={() => setOrgActiveSearch(orgSearch)}
                                        className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                                    >
                                        <i className="ri-search-line"></i>
                                        Search
                                    </button>
                                </div>

                                <div className="grid gap-4">
                                    {filteredOrganizations.map(org => (
                                        <button
                                            key={org.id}
                                            onClick={() => setSelectedOrgId(org.id)}
                                            className="bg-white rounded-lg border border-gray-200 p-5 text-left hover:border-teal-300 hover:bg-teal-50/30 transition-colors cursor-pointer w-full"
                                        >
                                            <div className="flex items-start gap-4">
                                                <div className="w-16 h-16 rounded-lg bg-gray-100 overflow-hidden flex-shrink-0 border border-gray-200">
                                                    <img src={org.avatarImage} alt={org.name} className="w-full h-full object-cover object-top" />
                                                </div>
                                                <div className="flex-1 min-w-0">
                                                    <h3 className="text-lg font-semibold text-gray-900">{org.name}</h3>
                                                    <p className="text-sm text-gray-600 mt-1 line-clamp-2">{org.description}</p>
                                                    <div className="flex items-center gap-4 mt-3">
                                                        <span className="flex items-center gap-1.5 text-xs text-gray-500">
                                                            <i className="ri-mail-line"></i>{org.contactEmail}
                                                        </span>
                                                        <span className="flex items-center gap-1.5 text-xs text-gray-500">
                                                            <i className="ri-phone-line"></i>{org.contactPhone}
                                                        </span>
                                                    </div>
                                                    <div className="flex items-center gap-4 mt-3">
                                                        <span className="text-xs text-teal-600 bg-teal-50 px-2 py-1 rounded-full">{org.totalGoals} goals</span>
                                                        <span className="text-xs text-emerald-600 bg-emerald-50 px-2 py-1 rounded-full">{org.completedGoals} completed</span>
                                                    </div>
                                                </div>
                                                <div className="flex items-center text-gray-400">
                                                    <i className="ri-arrow-right-s-line text-xl"></i>
                                                </div>
                                            </div>
                                        </button>
                                    ))}
                                </div>
                            </>
                        )}
                    </>
                )}
            </div>

            {/* Process Goal Modal */}
            {processingGoal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-lg w-full p-6 max-h-[90vh] overflow-y-auto">
                        <h3 className="text-lg font-semibold text-gray-900 mb-1">Process Reached Goal</h3>
                        <p className="text-sm text-gray-600 mb-4">
                            Upload photo proofs and documents confirming the goal items were purchased and shipped to the destination.
                        </p>

                        <div className="bg-gray-50 rounded-lg p-4 mb-4">
                            <div className="flex items-center gap-3 mb-2">
                                <div className="w-10 h-10 rounded-lg bg-gray-100 overflow-hidden">
                                    <img src={getOrgAvatar(processingGoal.organizationId)} alt="" className="w-full h-full object-cover object-top" />
                                </div>
                                <div>
                                    <p className="text-sm font-medium text-gray-900">{processingGoal.title}</p>
                                    <p className="text-xs text-gray-500">{processingGoal.organizationName}</p>
                                </div>
                            </div>
                            <div className="flex items-center justify-between text-sm">
                                <span className="text-gray-600">Budget fulfilled</span>
                                <span className="font-semibold text-emerald-600">${processingGoal.moneyRaised.toLocaleString()} / ${processingGoal.moneyBudget.toLocaleString()}</span>
                            </div>
                        </div>

                        <div className="mb-4">
                            <label className="block text-sm font-medium text-gray-700 mb-2">Proof Documents & Photos</label>
                            <div className="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center hover:border-teal-400 transition-colors cursor-pointer">
                                <label className="cursor-pointer">
                                    <div className="w-12 h-12 flex items-center justify-center mx-auto mb-3">
                                        <i className="ri-upload-cloud-2-line text-4xl text-gray-400"></i>
                                    </div>
                                    <p className="text-sm text-gray-600 mb-1">Click to upload documents</p>
                                    <p className="text-xs text-gray-400">JPG, PNG, PDF up to 10MB each</p>
                                    <input type="file" multiple accept="image/*,.pdf" onChange={handleFileUpload} className="hidden" />
                                </label>
                            </div>
                        </div>

                        {uploadedFiles.length > 0 && (
                            <div className="space-y-2 mb-4">
                                <p className="text-sm font-medium text-gray-700">{uploadedFiles.length} file{uploadedFiles.length !== 1 ? 's' : ''} uploaded</p>
                                {uploadedFiles.map((file, idx) => (
                                    <div key={idx} className="flex items-center justify-between bg-gray-50 rounded-lg px-3 py-2">
                                        <div className="flex items-center gap-2 min-w-0">
                                            <i className={`${file.type === 'PDF' ? 'ri-file-pdf-line text-red-500' : 'ri-image-line text-emerald-500'} text-lg`}></i>
                                            <span className="text-sm text-gray-700 truncate">{file.name}</span>
                                            <span className="text-xs text-gray-400 whitespace-nowrap">{file.size}</span>
                                        </div>
                                        <button onClick={() => handleRemoveFile(idx)} className="w-6 h-6 flex items-center justify-center rounded hover:bg-red-50 cursor-pointer text-gray-400 hover:text-red-500">
                                            <i className="ri-close-line"></i>
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}

                        <div className="mb-4">
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                YouTube Approvals & Thanks Video <span className="text-gray-400 font-normal">(optional)</span>
                            </label>
                            <div className="relative">
                                <i className="ri-youtube-line absolute left-3 top-1/2 -translate-y-1/2 text-red-500 text-lg"></i>
                                <input
                                    type="url"
                                    value={youtubeUrl}
                                    onChange={(e) => setYoutubeUrl(e.target.value)}
                                    placeholder="https://www.youtube.com/watch?v=..."
                                    className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                />
                            </div>
                            <p className="text-xs text-gray-400 mt-1">Link a YouTube video with approvals, thanks, or documentation of the completed goal</p>
                        </div>

                        <div className="flex items-center gap-3 pt-2">
                            <button
                                onClick={() => { setProcessingGoal(null); setUploadedFiles([]); setYoutubeUrl(''); }}
                                className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleCompleteProcess}
                                disabled={uploadedFiles.length === 0}
                                className={`flex-1 px-4 py-2 text-white text-sm font-medium rounded-lg transition-colors cursor-pointer whitespace-nowrap ${uploadedFiles.length === 0 ? 'bg-gray-300 cursor-not-allowed' : 'bg-teal-600 hover:bg-teal-700'
                                    }`}
                            >
                                <i className="ri-check-line mr-1.5"></i>Complete Processing
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}