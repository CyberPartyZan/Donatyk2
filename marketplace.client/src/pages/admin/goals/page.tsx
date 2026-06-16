import { useState } from 'react';
import { mockOrganizations, mockGoals } from '@/mocks/goals';
import { charityDirections } from '@/mocks/charity-directions';

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
    charityDirection?: string;
    parentGoalId: string | null;
}

export default function GoalsAdmin() {
    const [organization, setOrganization] = useState<Organization | null>(mockOrganizations[0]);
    const [goals, setGoals] = useState<Goal[]>(mockGoals.filter(g => g.organizationId === 'org-1'));
    const [isEditing, setIsEditing] = useState(false);
    const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
    const [showGoalModal, setShowGoalModal] = useState(false);
    const [editingGoal, setEditingGoal] = useState<Goal | null>(null);
    const [showSplitModal, setShowSplitModal] = useState(false);
    const [splittingGoal, setSplittingGoal] = useState<Goal | null>(null);
    const [imagePreview, setImagePreview] = useState(organization?.avatarImage || '');

    const [orgForm, setOrgForm] = useState({
        name: organization?.name || '',
        contactEmail: organization?.contactEmail || '',
        contactPhone: organization?.contactPhone || '',
        description: organization?.description || '',
        avatarImage: organization?.avatarImage || ''
    });

    const [goalForm, setGoalForm] = useState({
        title: '', explanation: '', moneyBudget: '', charityDirection: 'Education, Science and Youth Development',
        timeLimitStart: new Date().toISOString().split('T')[0], timeLimitEnd: '',
    });

    const [splitForm, setSplitForm] = useState({
        title: '', explanation: '', moneyBudget: '',
        charityDirection: 'Education, Science and Youth Development',
    });

    const [approvementUploads, setApprovementUploads] = useState<GoalDocument[]>([]);
    const [editApprovements, setEditApprovements] = useState<GoalDocument[]>([]);

    const resetGoalForm = () => { setGoalForm({ title: '', explanation: '', moneyBudget: '', charityDirection: 'Education, Science and Youth Development', timeLimitStart: new Date().toISOString().split('T')[0], timeLimitEnd: '' }); setApprovementUploads([]); };
    const resetSplitForm = () => setSplitForm({ title: '', explanation: '', moneyBudget: '', charityDirection: 'Education, Science and Youth Development' });

    const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            const reader = new FileReader();
            reader.onloadend = () => {
                const result = reader.result as string;
                setImagePreview(result);
                setOrgForm({ ...orgForm, avatarImage: result });
            };
            reader.readAsDataURL(file);
        }
    };

    const handleApprovementUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = e.target.files;
        if (!files) return;
        const newDocs: GoalDocument[] = Array.from(files).map(f => ({
            name: f.name,
            url: '#',
            type: f.name.split('.').pop()?.toUpperCase() || 'FILE',
            size: `${(f.size / 1024 / 1024).toFixed(1)} MB`,
        }));
        if (editingGoal) {
            setEditApprovements([...editApprovements, ...newDocs]);
        } else {
            setApprovementUploads([...approvementUploads, ...newDocs]);
        }
    };

    const removeApprovementDoc = (index: number, isEdit: boolean) => {
        if (isEdit) {
            setEditApprovements(editApprovements.filter((_, i) => i !== index));
        } else {
            setApprovementUploads(approvementUploads.filter((_, i) => i !== index));
        }
    };

    const handleCreateOrg = (e: React.FormEvent) => {
        e.preventDefault();
        const newOrg: Organization = {
            id: 'org-new-01',
            ...orgForm,
            createdAt: new Date().toISOString(),
            totalGoals: 0,
            completedGoals: 0,
        };
        setOrganization(newOrg);
        setIsEditing(false);
        setImagePreview(orgForm.avatarImage);
    };

    const handleUpdateOrg = (e: React.FormEvent) => {
        e.preventDefault();
        if (!organization) return;
        setOrganization({ ...organization, ...orgForm });
        setIsEditing(false);
        setImagePreview(orgForm.avatarImage);
    };

    const handleEditClick = () => {
        if (organization) {
            setOrgForm({
                name: organization.name,
                contactEmail: organization.contactEmail,
                contactPhone: organization.contactPhone,
                description: organization.description,
                avatarImage: organization.avatarImage
            });
            setImagePreview(organization.avatarImage);
        }
        setIsEditing(true);
    };

    const handleCancelEdit = () => {
        setIsEditing(false);
        if (organization) {
            setOrgForm({
                name: organization.name,
                contactEmail: organization.contactEmail,
                contactPhone: organization.contactPhone,
                description: organization.description,
                avatarImage: organization.avatarImage
            });
            setImagePreview(organization.avatarImage);
        }
    };

    const handleDeleteOrg = () => {
        setOrganization(null);
        setGoals([]);
        setShowDeleteConfirm(false);
        setOrgForm({ name: '', contactEmail: '', contactPhone: '', description: '', avatarImage: '' });
        setImagePreview('');
    };

    const handleCreateGoal = (e: React.FormEvent) => {
        e.preventDefault();
        if (!organization) return;
        const newGoal: Goal = {
            id: 'goal-' + Date.now(),
            organizationId: organization.id,
            organizationName: organization.name,
            title: goalForm.title,
            explanation: goalForm.explanation,
            moneyBudget: parseFloat(goalForm.moneyBudget) || 0,
            timeLimitStart: goalForm.timeLimitStart || null,
            timeLimitEnd: goalForm.timeLimitEnd || null,
            moneyRaised: 0,
            status: 'Active',
            approvementDocuments: approvementUploads,
            linkedLots: [],
            createdAt: new Date().toISOString(),
            charityDirection: goalForm.charityDirection,
            parentGoalId: null,
        };
        setGoals([newGoal, ...goals]);
        setOrganization({ ...organization, totalGoals: organization.totalGoals + 1 });
        setShowGoalModal(false);
        resetGoalForm();
    };

    const handleUpdateGoal = (e: React.FormEvent) => {
        e.preventDefault();
        if (!editingGoal) return;
        setGoals(goals.map(g => g.id === editingGoal.id ? {
            ...g,
            title: goalForm.title,
            explanation: goalForm.explanation,
            moneyBudget: parseFloat(goalForm.moneyBudget) || 0,
            timeLimitStart: goalForm.timeLimitStart || null,
            timeLimitEnd: goalForm.timeLimitEnd || null,
            charityDirection: goalForm.charityDirection,
            approvementDocuments: editApprovements,
        } : g));
        setShowGoalModal(false);
        setEditingGoal(null);
        resetGoalForm();
    };

    const handleEditGoal = (goal: Goal) => {
        setEditingGoal(goal);
        setGoalForm({
            title: goal.title,
            explanation: goal.explanation,
            moneyBudget: goal.moneyBudget.toString(),
            timeLimitStart: goal.timeLimitStart?.split('T')[0] || '',
            timeLimitEnd: goal.timeLimitEnd?.split('T')[0] || '',
            charityDirection: goal.charityDirection || 'Education, Science and Youth Development',
        });
        setEditApprovements([...goal.approvementDocuments]);
        setApprovementUploads([]);
        setShowGoalModal(true);
    };

    const handleDeleteGoal = (id: string) => {
        const goal = goals.find(g => g.id === id);
        if (window.confirm(`Delete goal "${goal?.title}"?`)) {
            setGoals(goals.filter(g => g.id !== id));
            if (organization && goal) {
                setOrganization({ ...organization, totalGoals: organization.totalGoals - 1 });
            }
        }
    };

    const openSplitModal = (goal: Goal) => {
        setSplittingGoal(goal);
        setSplitForm({
            title: '',
            explanation: '',
            moneyBudget: '',
            charityDirection: goal.charityDirection || 'Education, Science and Youth Development',
        });
        setShowSplitModal(true);
    };

    const handleSplitGoal = (e: React.FormEvent) => {
        e.preventDefault();
        if (!splittingGoal || !organization) return;
        const splitBudget = parseFloat(splitForm.moneyBudget) || 0;
        if (splitBudget <= 0 || splitBudget >= splittingGoal.moneyBudget) {
            alert('Split budget must be greater than 0 and less than the parent goal budget.');
            return;
        }
        const newGoal: Goal = {
            id: 'goal-' + Date.now(),
            organizationId: organization.id,
            organizationName: organization.name,
            title: splitForm.title,
            explanation: splitForm.explanation,
            moneyBudget: splitBudget,
            timeLimitStart: splittingGoal.timeLimitStart,
            timeLimitEnd: splittingGoal.timeLimitEnd,
            moneyRaised: 0,
            status: 'Active',
            approvementDocuments: [],
            linkedLots: [],
            createdAt: new Date().toISOString(),
            charityDirection: splitForm.charityDirection,
            parentGoalId: splittingGoal.id,
        };
        setGoals([newGoal, ...goals]);
        setOrganization({ ...organization, totalGoals: organization.totalGoals + 1 });
        setShowSplitModal(false);
        setSplittingGoal(null);
        resetSplitForm();
    };

    const getProgressPercent = (raised: number, budget: number) => Math.min(Math.round((raised / budget) * 100), 100);

    const completedGoals = goals.filter(g => g.status === 'Reached').length;
    const activeGoals = goals.filter(g => g.status === 'Active').length;
    const totalRaised = goals.reduce((sum, g) => sum + g.moneyRaised, 0);

    const getParentGoalTitle = (parentGoalId: string | null) => {
        if (!parentGoalId) return null;
        const parent = goals.find(g => g.id === parentGoalId);
        return parent?.title || 'Unknown Parent Goal';
    };

    if (!organization) {
        return (
            <div className="p-8">
                <div className="max-w-3xl mx-auto">
                    <div className="mb-6">
                        <h1 className="text-2xl font-bold text-gray-900">My Organization</h1>
                        <p className="text-sm text-gray-600 mt-1">Create your charity organization to start fundraising goals</p>
                    </div>
                    <form onSubmit={handleCreateOrg} className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                        <div className="space-y-5">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">Organization Avatar</label>
                                <div className="flex items-center gap-4">
                                    <div className="w-24 h-24 rounded-lg bg-gray-100 flex items-center justify-center overflow-hidden border-2 border-gray-200">
                                        {imagePreview ? (
                                            <img src={imagePreview} alt="Preview" className="w-full h-full object-cover object-top" />
                                        ) : (
                                            <i className="ri-image-line text-3xl text-gray-400"></i>
                                        )}
                                    </div>
                                    <div>
                                        <label className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap inline-block">
                                            <i className="ri-upload-2-line mr-2"></i>Upload Avatar
                                            <input type="file" accept="image/*" onChange={handleImageUpload} className="hidden" />
                                        </label>
                                        <p className="text-xs text-gray-500 mt-1">JPG, PNG or GIF. Max 5MB.</p>
                                    </div>
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Organization Name</label>
                                <input type="text" value={orgForm.name} onChange={e => setOrgForm({ ...orgForm, name: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" placeholder="Enter your organization name" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                                <textarea value={orgForm.description} onChange={e => setOrgForm({ ...orgForm, description: e.target.value })} rows={4} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" placeholder="Describe your organization's mission" required />
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Contact Email</label>
                                    <input type="email" value={orgForm.contactEmail} onChange={e => setOrgForm({ ...orgForm, contactEmail: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" placeholder="contact@example.org" required />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Contact Phone</label>
                                    <input type="tel" value={orgForm.contactPhone} onChange={e => setOrgForm({ ...orgForm, contactPhone: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" placeholder="+1 (555) 000-0000" required />
                                </div>
                            </div>
                        </div>
                        <div className="flex items-center gap-3 mt-6 pt-6 border-t border-gray-200">
                            <button type="submit" className="px-6 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap">
                                <i className="ri-check-line mr-2"></i>Create Organization
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        );
    }

    return (
        <div className="p-8">
            <div className="max-w-5xl mx-auto space-y-6">
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">My Organization</h1>
                        <p className="text-sm text-gray-600 mt-1">Manage your charity organization and fundraising goals</p>
                    </div>
                    {!isEditing && (
                        <div className="flex items-center gap-3">
                            <button onClick={handleEditClick} className="px-4 py-2 bg-gray-100 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-200 transition-colors cursor-pointer whitespace-nowrap">
                                <i className="ri-edit-line mr-2"></i>Edit Organization
                            </button>
                            <button onClick={() => setShowDeleteConfirm(true)} className="px-4 py-2 bg-red-50 text-red-600 text-sm font-medium rounded-lg hover:bg-red-100 transition-colors cursor-pointer whitespace-nowrap">
                                <i className="ri-delete-bin-line mr-2"></i>Delete
                            </button>
                        </div>
                    )}
                </div>

                {/* Organization Profile */}
                {isEditing ? (
                    <form onSubmit={handleUpdateOrg} className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                        <div className="space-y-5">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">Organization Avatar</label>
                                <div className="flex items-center gap-4">
                                    <div className="w-24 h-24 rounded-lg bg-gray-100 flex items-center justify-center overflow-hidden border-2 border-gray-200">
                                        {imagePreview ? (
                                            <img src={imagePreview} alt="Preview" className="w-full h-full object-cover object-top" />
                                        ) : (
                                            <i className="ri-image-line text-3xl text-gray-400"></i>
                                        )}
                                    </div>
                                    <div>
                                        <label className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap inline-block">
                                            <i className="ri-upload-2-line mr-2"></i>Change Avatar
                                            <input type="file" accept="image/*" onChange={handleImageUpload} className="hidden" />
                                        </label>
                                        <p className="text-xs text-gray-500 mt-1">JPG, PNG or GIF. Max 5MB.</p>
                                    </div>
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Organization Name</label>
                                <input type="text" value={orgForm.name} onChange={e => setOrgForm({ ...orgForm, name: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                                <textarea value={orgForm.description} onChange={e => setOrgForm({ ...orgForm, description: e.target.value })} rows={4} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Contact Email</label>
                                    <input type="email" value={orgForm.contactEmail} onChange={e => setOrgForm({ ...orgForm, contactEmail: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Contact Phone</label>
                                    <input type="tel" value={orgForm.contactPhone} onChange={e => setOrgForm({ ...orgForm, contactPhone: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                                </div>
                            </div>
                        </div>
                        <div className="flex items-center gap-3 mt-6 pt-6 border-t border-gray-200">
                            <button type="submit" className="px-6 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap">
                                <i className="ri-check-line mr-2"></i>Save Changes
                            </button>
                            <button type="button" onClick={handleCancelEdit} className="px-6 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">
                                Cancel
                            </button>
                        </div>
                    </form>
                ) : (
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
                        <div className="p-6">
                            <div className="flex items-start gap-6">
                                <div className="w-32 h-32 rounded-lg bg-gray-100 flex items-center justify-center overflow-hidden border-2 border-gray-200 flex-shrink-0">
                                    <img src={organization.avatarImage} alt={organization.name} className="w-full h-full object-cover object-top" />
                                </div>
                                <div className="flex-1">
                                    <h2 className="text-xl font-bold text-gray-900 mb-1">{organization.name}</h2>
                                    <p className="text-sm text-gray-600 leading-relaxed mb-4">{organization.description}</p>
                                    <div className="grid grid-cols-2 gap-4">
                                        <div className="flex items-center gap-2 text-sm">
                                            <i className="ri-mail-line text-gray-400"></i>
                                            <span className="text-gray-700">{organization.contactEmail}</span>
                                        </div>
                                        <div className="flex items-center gap-2 text-sm">
                                            <i className="ri-phone-line text-gray-400"></i>
                                            <span className="text-gray-700">{organization.contactPhone}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div className="grid grid-cols-4 border-t border-gray-200">
                            <div className="px-6 py-4 border-r border-gray-200">
                                <div className="text-2xl font-bold text-gray-900">{organization.totalGoals}</div>
                                <div className="text-xs text-gray-600 mt-1">Total Goals</div>
                            </div>
                            <div className="px-6 py-4 border-r border-gray-200">
                                <div className="text-2xl font-bold text-emerald-600">{completedGoals}</div>
                                <div className="text-xs text-gray-600 mt-1">Reached</div>
                            </div>
                            <div className="px-6 py-4 border-r border-gray-200">
                                <div className="text-2xl font-bold text-amber-500">{activeGoals}</div>
                                <div className="text-xs text-gray-600 mt-1">Active</div>
                            </div>
                            <div className="px-6 py-4">
                                <div className="text-2xl font-bold text-teal-600">${totalRaised.toLocaleString()}</div>
                                <div className="text-xs text-gray-600 mt-1">Total Raised</div>
                            </div>
                        </div>
                    </div>
                )}

                {/* Goals Section */}
                {!isEditing && (
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200">
                        <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
                            <div>
                                <h3 className="text-base font-semibold text-gray-900">Fundraising Goals</h3>
                                <p className="text-xs text-gray-500 mt-0.5">{goals.length} goal{goals.length !== 1 ? 's' : ''} created</p>
                            </div>
                            <button
                                onClick={() => { setEditingGoal(null); resetGoalForm(); setShowGoalModal(true); }}
                                className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                            >
                                <i className="ri-add-line"></i>Create Goal
                            </button>
                        </div>

                        <div className="p-6">
                            {goals.length === 0 ? (
                                <div className="text-center py-12">
                                    <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                                        <i className="ri-flag-line text-5xl text-gray-300"></i>
                                    </div>
                                    <p className="text-gray-500 text-sm mb-4">No goals created yet</p>
                                    <button
                                        onClick={() => { setEditingGoal(null); resetGoalForm(); setShowGoalModal(true); }}
                                        className="px-5 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                                    >
                                        <i className="ri-add-line mr-2"></i>Create Your First Goal
                                    </button>
                                </div>
                            ) : (
                                <div className="grid gap-4">
                                    {goals.map(goal => {
                                        const pct = getProgressPercent(goal.moneyRaised, goal.moneyBudget);
                                        const parentTitle = getParentGoalTitle(goal.parentGoalId);
                                        return (
                                            <div key={goal.id} className={`bg-white rounded-lg border p-5 ${goal.status === 'Reached' ? 'border-emerald-200 bg-emerald-50/30' : 'border-gray-200'}`}>
                                                <div className="flex items-start justify-between mb-3">
                                                    <div className="flex-1 min-w-0">
                                                        <div className="flex items-center gap-2 mb-1 flex-wrap">
                                                            <span className={`px-2 py-0.5 text-xs font-medium rounded-full ${goal.status === 'Reached' ? 'bg-emerald-100 text-emerald-700' : 'bg-amber-100 text-amber-700'}`}>
                                                                {goal.status}
                                                            </span>
                                                            {goal.charityDirection && (
                                                                <span className="px-2 py-0.5 text-xs bg-teal-50 text-teal-600 rounded-full">{goal.charityDirection}</span>
                                                            )}
                                                            {goal.parentGoalId && parentTitle && (
                                                                <span className="px-2 py-0.5 text-xs bg-orange-50 text-orange-600 rounded-full flex items-center gap-1">
                                                                    <i className="ri-git-branch-line"></i>From: {parentTitle}
                                                                </span>
                                                            )}
                                                        </div>
                                                        <h3 className="text-lg font-semibold text-gray-900">{goal.title}</h3>
                                                        <p className="text-sm text-gray-600 mt-1 line-clamp-2">{goal.explanation}</p>
                                                    </div>
                                                    <div className="flex items-center gap-2 ml-4">
                                                        <button onClick={() => handleEditGoal(goal)} className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-gray-100 transition-colors cursor-pointer text-gray-500">
                                                            <i className="ri-edit-line"></i>
                                                        </button>
                                                        {!goal.parentGoalId && (
                                                            <button onClick={() => openSplitModal(goal)} className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-orange-50 transition-colors cursor-pointer text-orange-500" title="Split Goal">
                                                                <i className="ri-git-branch-line"></i>
                                                            </button>
                                                        )}
                                                        <button onClick={() => handleDeleteGoal(goal.id)} className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-red-50 transition-colors cursor-pointer text-gray-500 hover:text-red-500">
                                                            <i className="ri-delete-bin-line"></i>
                                                        </button>
                                                    </div>
                                                </div>

                                                <div className="mb-3">
                                                    <div className="flex items-center justify-between mb-1.5">
                                                        <span className="text-sm font-semibold text-gray-900">${goal.moneyRaised.toLocaleString()} raised</span>
                                                        <span className="text-sm text-gray-500">of ${goal.moneyBudget.toLocaleString()}</span>
                                                    </div>
                                                    <div className="w-full h-2.5 bg-gray-200 rounded-full overflow-hidden">
                                                        <div
                                                            className={`h-full rounded-full transition-all duration-500 ${goal.status === 'Reached' ? 'bg-emerald-500' : 'bg-teal-500'}`}
                                                            style={{ width: `${pct}%` }}
                                                        ></div>
                                                    </div>
                                                    <span className="text-xs text-gray-400 mt-1 block">{pct}% funded</span>
                                                </div>

                                                <div className="flex items-center gap-4 flex-wrap text-xs text-gray-500">
                                                    {goal.timeLimitStart && (
                                                        <span className="flex items-center gap-1">
                                                            <i className="ri-calendar-line"></i>
                                                            {new Date(goal.timeLimitStart).toLocaleDateString()} – {goal.timeLimitEnd ? new Date(goal.timeLimitEnd).toLocaleDateString() : 'Ongoing'}
                                                        </span>
                                                    )}
                                                    <span className="flex items-center gap-1">
                                                        <i className="ri-file-list-3-line"></i>
                                                        {goal.approvementDocuments.length} document{goal.approvementDocuments.length !== 1 ? 's' : ''}
                                                    </span>
                                                    <span className="flex items-center gap-1">
                                                        <i className="ri-link-m"></i>
                                                        {goal.linkedLots.length} linked lot{goal.linkedLots.length !== 1 ? 's' : ''}
                                                    </span>
                                                </div>

                                                {goal.approvementDocuments.length > 0 && (
                                                    <div className="mt-3 pt-3 border-t border-gray-100">
                                                        <p className="text-xs font-medium text-gray-700 mb-2">Approvement Documents</p>
                                                        <div className="flex flex-wrap gap-2">
                                                            {goal.approvementDocuments.map((doc, idx) => (
                                                                <a key={idx} href={doc.url} className="flex items-center gap-1.5 px-2.5 py-1.5 bg-gray-50 rounded-lg text-xs text-gray-600 hover:bg-gray-100 transition-colors cursor-pointer">
                                                                    <i className="ri-file-pdf-line text-red-500"></i>
                                                                    {doc.name}
                                                                    <span className="text-gray-400">({doc.size})</span>
                                                                </a>
                                                            ))}
                                                        </div>
                                                    </div>
                                                )}
                                            </div>
                                        );
                                    })}
                                </div>
                            )}
                        </div>
                    </div>
                )}
            </div>

            {/* Delete Org Confirm */}
            {showDeleteConfirm && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
                        <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center mx-auto mb-4">
                            <i className="ri-alert-line text-red-600 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900 text-center mb-2">Delete Organization?</h3>
                        <p className="text-sm text-gray-600 text-center mb-6">
                            This will permanently delete your organization and all associated goals. This action cannot be undone.
                        </p>
                        <div className="flex items-center gap-3">
                            <button onClick={() => setShowDeleteConfirm(false)} className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">Cancel</button>
                            <button onClick={handleDeleteOrg} className="flex-1 px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors cursor-pointer whitespace-nowrap">Delete</button>
                        </div>
                    </div>
                </div>
            )}

            {/* Goal Form Modal */}
            {showGoalModal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-lg w-full p-6 max-h-[90vh] overflow-y-auto">
                        <h3 className="text-lg font-semibold text-gray-900 mb-4">{editingGoal ? 'Edit Goal' : 'Create Goal'}</h3>
                        <form onSubmit={editingGoal ? handleUpdateGoal : handleCreateGoal} className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Goal Title</label>
                                <input type="text" value={goalForm.title} onChange={e => setGoalForm({ ...goalForm, title: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Explanation of Purpose</label>
                                <textarea value={goalForm.explanation} onChange={e => setGoalForm({ ...goalForm, explanation: e.target.value })} rows={4} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Charity Direction</label>
                                <select value={goalForm.charityDirection} onChange={e => setGoalForm({ ...goalForm, charityDirection: e.target.value })} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer">
                                    {charityDirections.map(d => (
                                        <option key={d} value={d}>{d}</option>
                                    ))}
                                </select>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Money Budget ($)</label>
                                <input type="number" value={goalForm.moneyBudget} onChange={e => setGoalForm({ ...goalForm, moneyBudget: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" min="0" required />
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Time Limit Start (optional)</label>
                                    <input type="date" value={goalForm.timeLimitStart} onChange={e => setGoalForm({ ...goalForm, timeLimitStart: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Time Limit End (optional)</label>
                                    <input type="date" value={goalForm.timeLimitEnd} onChange={e => setGoalForm({ ...goalForm, timeLimitEnd: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" />
                                </div>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">
                                    <i className="ri-file-pdf-line mr-1 text-red-500"></i>
                                    Approvement Documents
                                </label>

                                {/* Existing docs in edit mode */}
                                {editingGoal && editApprovements.length > 0 && (
                                    <div className="space-y-2 mb-3">
                                        <p className="text-xs text-gray-500">Existing documents:</p>
                                        {editApprovements.map((doc, idx) => (
                                            <div key={idx} className="flex items-center justify-between bg-gray-50 rounded-lg px-3 py-2">
                                                <div className="flex items-center gap-2">
                                                    <i className="ri-file-pdf-line text-red-500"></i>
                                                    <span className="text-sm text-gray-700 truncate">{doc.name}</span>
                                                    <span className="text-xs text-gray-400 whitespace-nowrap">{doc.size}</span>
                                                </div>
                                                <button type="button" onClick={() => removeApprovementDoc(idx, true)} className="w-6 h-6 flex items-center justify-center rounded hover:bg-red-50 cursor-pointer text-gray-400 hover:text-red-500">
                                                    <i className="ri-close-line"></i>
                                                </button>
                                            </div>
                                        ))}
                                    </div>
                                )}

                                {/* New uploads (create mode) */}
                                {!editingGoal && approvementUploads.length > 0 && (
                                    <div className="space-y-2 mb-3">
                                        {approvementUploads.map((doc, idx) => (
                                            <div key={idx} className="flex items-center justify-between bg-teal-50 rounded-lg px-3 py-2">
                                                <div className="flex items-center gap-2">
                                                    <i className="ri-file-pdf-line text-red-500"></i>
                                                    <span className="text-sm text-gray-700 truncate">{doc.name}</span>
                                                    <span className="text-xs text-gray-400 whitespace-nowrap">{doc.size}</span>
                                                </div>
                                                <button type="button" onClick={() => removeApprovementDoc(idx, false)} className="w-6 h-6 flex items-center justify-center rounded hover:bg-red-50 cursor-pointer text-gray-400 hover:text-red-500">
                                                    <i className="ri-close-line"></i>
                                                </button>
                                            </div>
                                        ))}
                                    </div>
                                )}

                                <div className="border-2 border-dashed border-gray-300 rounded-lg p-4 text-center hover:border-teal-400 transition-colors cursor-pointer">
                                    <input
                                        type="file"
                                        accept=".pdf,image/*"
                                        multiple
                                        onChange={handleApprovementUpload}
                                        className="hidden"
                                        id="approvement-upload"
                                    />
                                    <label htmlFor="approvement-upload" className="cursor-pointer">
                                        <i className="ri-upload-cloud-2-line text-2xl text-gray-400 mb-1 block"></i>
                                        <p className="text-sm text-gray-600">Click to upload approvement documents</p>
                                        <p className="text-xs text-gray-400">PDF, JPG, PNG up to 10MB each</p>
                                    </label>
                                </div>
                            </div>
                            <div className="flex items-center gap-3 pt-2">
                                <button type="submit" className="flex-1 px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap">
                                    {editingGoal ? 'Save Changes' : 'Create Goal'}
                                </button>
                                <button type="button" onClick={() => { setShowGoalModal(false); setEditingGoal(null); resetGoalForm(); }} className="px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">Cancel</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Split Goal Modal */}
            {showSplitModal && splittingGoal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-lg w-full p-6 max-h-[90vh] overflow-y-auto">
                        <h3 className="text-lg font-semibold text-gray-900 mb-2">Split Goal</h3>
                        <div className="bg-orange-50 rounded-lg p-3 mb-4">
                            <p className="text-sm text-orange-700">
                                Splitting from: <strong>{splittingGoal.title}</strong>
                            </p>
                            <p className="text-xs text-orange-600 mt-1">Parent Budget: ${splittingGoal.moneyBudget.toLocaleString()}</p>
                        </div>
                        <form onSubmit={handleSplitGoal} className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">New Goal Title</label>
                                <input type="text" value={splitForm.title} onChange={e => setSplitForm({ ...splitForm, title: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Explanation</label>
                                <textarea value={splitForm.explanation} onChange={e => setSplitForm({ ...splitForm, explanation: e.target.value })} rows={3} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Charity Direction</label>
                                <select value={splitForm.charityDirection} onChange={e => setSplitForm({ ...splitForm, charityDirection: e.target.value })} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer">
                                    {charityDirections.map(d => (
                                        <option key={d} value={d}>{d}</option>
                                    ))}
                                </select>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Budget from Parent Goal ($)</label>
                                <input type="number" value={splitForm.moneyBudget} onChange={e => setSplitForm({ ...splitForm, moneyBudget: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" min="1" max={splittingGoal.moneyBudget - 1} required />
                                <p className="text-xs text-gray-400 mt-1">Must be less than ${splittingGoal.moneyBudget.toLocaleString()}</p>
                            </div>
                            <div className="flex items-center gap-3 pt-2">
                                <button type="submit" className="flex-1 px-4 py-2 bg-orange-600 text-white text-sm font-medium rounded-lg hover:bg-orange-700 transition-colors cursor-pointer whitespace-nowrap">
                                    <i className="ri-git-branch-line mr-2"></i>Split Goal
                                </button>
                                <button type="button" onClick={() => { setShowSplitModal(false); setSplittingGoal(null); resetSplitForm(); }} className="px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">Cancel</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}