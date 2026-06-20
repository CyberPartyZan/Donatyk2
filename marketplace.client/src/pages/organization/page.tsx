import { useParams, Link } from 'react-router-dom';
import { useState } from 'react';
import { mockGoals, mockOrganizations } from '@/mocks/goals';
import Pagination from '@/components/base/Pagination';

const ITEMS_PER_PAGE = 6;

export default function OrganizationPage() {
    const { orgId } = useParams();
    const [currentPage, setCurrentPage] = useState(1);

    const organization = mockOrganizations.find(o => o.id === orgId);

    if (!organization) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <i className="ri-building-line text-6xl text-gray-300 mb-4 block"></i>
                    <p className="text-gray-600 text-lg mb-4">Organization not found</p>
                    <Link to="/marketplace" className="px-6 py-2.5 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors whitespace-nowrap cursor-pointer inline-block">
                        Back to Marketplace
                    </Link>
                </div>
            </div>
        );
    }

    const orgGoals = mockGoals.filter(g => g.organizationId === organization.id);
    const totalRaisedAll = orgGoals.reduce((sum, g) => sum + g.moneyRaised, 0);
    const totalBudgetAll = orgGoals.reduce((sum, g) => sum + g.moneyBudget, 0);
    const reachedCount = orgGoals.filter(g => g.status === 'Reached').length;
    const activeCount = orgGoals.filter(g => g.status === 'Active').length;

    const totalGoalPages = Math.ceil(orgGoals.length / ITEMS_PER_PAGE);
    const pagedGoals = orgGoals.slice((currentPage - 1) * ITEMS_PER_PAGE, currentPage * ITEMS_PER_PAGE);

    const getProgressPercent = (raised: number, budget: number) =>
        budget > 0 ? Math.min(Math.round((raised / budget) * 100), 100) : 0;

    return (
        <div className="min-h-screen bg-gray-50">
            <div className="max-w-[1600px] mx-auto px-6 py-8">
                {/* Breadcrumbs */}
                <div className="flex items-center gap-2 text-sm text-gray-600 mb-8">
                    <Link to="/" className="hover:text-teal-500 cursor-pointer">Home</Link>
                    <i className="ri-arrow-right-s-line"></i>
                    <span className="text-gray-900">{organization.name}</span>
                </div>

                {/* Organization Header */}
                <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden mb-8">
                    <div className="bg-gradient-to-r from-teal-500 to-emerald-500 px-8 py-10">
                        <div className="flex items-start gap-6 flex-col lg:flex-row">
                            <div className="w-24 h-24 rounded-xl bg-white/20 overflow-hidden flex-shrink-0 border-2 border-white/30">
                                <img
                                    src={organization.avatarImage}
                                    alt={organization.name}
                                    className="w-full h-full object-cover object-top"
                                />
                            </div>
                            <div className="flex-1">
                                <h1 className="text-3xl font-bold text-white mb-2">{organization.name}</h1>
                                <p className="text-white/80 text-sm leading-relaxed max-w-3xl">{organization.description}</p>
                            </div>
                        </div>
                    </div>

                    {/* Stats Bar */}
                    <div className="grid grid-cols-4 border-t border-gray-100">
                        <div className="px-8 py-5 border-r border-gray-100">
                            <p className="text-xs text-gray-500 mb-1">Total Goals</p>
                            <p className="text-xl font-bold text-gray-900">{orgGoals.length}</p>
                        </div>
                        <div className="px-8 py-5 border-r border-gray-100">
                            <p className="text-xs text-gray-500 mb-1">Active Goals</p>
                            <p className="text-xl font-bold text-amber-600">{activeCount}</p>
                        </div>
                        <div className="px-8 py-5 border-r border-gray-100">
                            <p className="text-xs text-gray-500 mb-1">Reached Goals</p>
                            <p className="text-xl font-bold text-emerald-600">{reachedCount}</p>
                        </div>
                        <div className="px-8 py-5">
                            <p className="text-xs text-gray-500 mb-1">Total Raised</p>
                            <p className="text-xl font-bold text-teal-600">
                                ${totalRaisedAll.toLocaleString()}
                                <span className="text-sm text-gray-400 font-normal ml-1">/ ${totalBudgetAll.toLocaleString()}</span>
                            </p>
                        </div>
                    </div>

                    {/* Contact Info */}
                    <div className="px-8 py-5 bg-gray-50 border-t border-gray-100">
                        <div className="flex items-center gap-6 flex-wrap">
                            <div className="flex items-center gap-2 text-sm text-gray-600">
                                <i className="ri-mail-line text-gray-400"></i>
                                <a href={`mailto:${organization.contactEmail}`} className="hover:text-teal-600 transition-colors">{organization.contactEmail}</a>
                            </div>
                            <div className="flex items-center gap-2 text-sm text-gray-600">
                                <i className="ri-phone-line text-gray-400"></i>
                                <a href={`tel:${organization.contactPhone}`} className="hover:text-teal-600 transition-colors">{organization.contactPhone}</a>
                            </div>
                            <div className="flex items-center gap-2 text-sm text-gray-500">
                                <i className="ri-calendar-line text-gray-400"></i>
                                <span>Joined {new Date(organization.createdAt).toLocaleDateString('en-US', { year: 'numeric', month: 'long' })}</span>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Goals Section */}
                <div>
                    <div className="flex items-center justify-between mb-6">
                        <div>
                            <h2 className="text-2xl font-bold text-gray-900">All Goals</h2>
                            <p className="text-sm text-gray-600 mt-1">
                                {orgGoals.length} goal{orgGoals.length !== 1 ? 's' : ''} by {organization.name}
                            </p>
                        </div>
                    </div>

                    {orgGoals.length === 0 ? (
                        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-12 text-center">
                            <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                                <i className="ri-flag-line text-gray-400 text-2xl"></i>
                            </div>
                            <p className="text-gray-500 text-sm">This organization has no goals yet</p>
                        </div>
                    ) : (
                        <>
                            <div className="grid gap-6">
                                {pagedGoals.map(goal => {
                                    const pct = getProgressPercent(goal.moneyRaised, goal.moneyBudget);
                                    return (
                                        <Link
                                            key={goal.id}
                                            to={`/goals/${goal.id}`}
                                            className="bg-white rounded-lg border border-gray-200 p-6 hover:border-teal-300 hover:shadow-sm transition-all cursor-pointer group block"
                                        >
                                            <div className="flex items-start justify-between flex-col lg:flex-row gap-4">
                                                <div className="flex-1 min-w-0">
                                                    <div className="flex items-center gap-3 mb-2 flex-wrap">
                                                        <span className={`px-2.5 py-0.5 text-xs font-semibold rounded-full whitespace-nowrap ${goal.status === 'Reached'
                                                                ? 'bg-emerald-100 text-emerald-700'
                                                                : 'bg-amber-100 text-amber-700'
                                                            }`}>
                                                            {goal.status}
                                                        </span>
                                                        {goal.charityDirection && (
                                                            <span className="px-2 py-0.5 text-xs bg-teal-50 text-teal-600 rounded-full whitespace-nowrap">
                                                                {goal.charityDirection}
                                                            </span>
                                                        )}
                                                        {goal.parentGoalId && (
                                                            <span className="px-2 py-0.5 text-xs bg-orange-50 text-orange-600 rounded-full flex items-center gap-1 whitespace-nowrap">
                                                                <i className="ri-git-branch-line"></i>Sub-goal
                                                            </span>
                                                        )}
                                                    </div>
                                                    <h3 className="text-lg font-semibold text-gray-900 group-hover:text-teal-600 transition-colors mb-2">
                                                        {goal.title}
                                                    </h3>
                                                    <p className="text-sm text-gray-600 line-clamp-2 mb-4">{goal.explanation}</p>

                                                    {/* Progress */}
                                                    <div className="mb-3">
                                                        <div className="flex items-center justify-between mb-1.5">
                                                            <span className="text-sm font-semibold text-gray-900">
                                                                ${goal.moneyRaised.toLocaleString()} raised
                                                            </span>
                                                            <span className="text-sm text-gray-500">
                                                                of ${goal.moneyBudget.toLocaleString()}
                                                            </span>
                                                        </div>
                                                        <div className="w-full h-2.5 bg-gray-200 rounded-full overflow-hidden">
                                                            <div
                                                                className={`h-full rounded-full transition-all duration-500 ${goal.status === 'Reached' ? 'bg-emerald-500' : 'bg-teal-500'
                                                                    }`}
                                                                style={{ width: `${pct}%` }}
                                                            ></div>
                                                        </div>
                                                    </div>

                                                    <div className="flex items-center gap-4 flex-wrap text-xs text-gray-500">
                                                        {goal.timeLimitStart && (
                                                            <span className="flex items-center gap-1">
                                                                <i className="ri-calendar-line"></i>
                                                                {new Date(goal.timeLimitStart).toLocaleDateString()}
                                                                {goal.timeLimitEnd ? ` - ${new Date(goal.timeLimitEnd).toLocaleDateString()}` : ''}
                                                            </span>
                                                        )}
                                                        <span className="flex items-center gap-1">
                                                            <i className="ri-link-m"></i>
                                                            {goal.linkedLots.length} linked lot{goal.linkedLots.length !== 1 ? 's' : ''}
                                                        </span>
                                                        <span className="flex items-center gap-1">
                                                            <i className="ri-file-list-3-line"></i>
                                                            {goal.approvementDocuments.length} document{goal.approvementDocuments.length !== 1 ? 's' : ''}
                                                        </span>
                                                    </div>
                                                </div>

                                                {/* Progress Circle */}
                                                <div className="flex-shrink-0 flex items-center">
                                                    <div className="relative w-20 h-20">
                                                        <svg className="w-20 h-20 -rotate-90" viewBox="0 0 100 100">
                                                            <circle cx="50" cy="50" r="40" stroke="#e5e7eb" strokeWidth="8" fill="none" />
                                                            <circle
                                                                cx="50" cy="50" r="40"
                                                                stroke={goal.status === 'Reached' ? '#059669' : '#0d9488'}
                                                                strokeWidth="8"
                                                                fill="none"
                                                                strokeLinecap="round"
                                                                strokeDasharray={`${pct * 2.513} 251.3`}
                                                            />
                                                        </svg>
                                                        <div className="absolute inset-0 flex items-center justify-center">
                                                            <span className="text-sm font-bold text-teal-600">{pct}%</span>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </Link>
                                    );
                                })}
                            </div>
                            <Pagination currentPage={currentPage} totalPages={totalGoalPages} onPageChange={(p) => setCurrentPage(p)} />
                        </>
                    )}
                </div>
            </div>
        </div>
    );
}