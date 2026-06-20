import { useParams, Link } from 'react-router-dom';
import { mockGoals, mockOrganizations } from '@/mocks/goals';
import { products } from '@/mocks/products';
import { mockLots } from '@/mocks/lots';

const lotImageMap: Record<string, string> = {
    '1': 'https://readdy.ai/api/search-image?query=vintage%20rolex%20submariner%20luxury%20watch%20product%20photography%20premium%20timepiece%20on%20clean%20white%20background%20charity%20auction&width=400&height=400&seq=goal-lot-img-1&orientation=squarish',
    '2': 'https://readdy.ai/api/search-image?query=apple%20macbook%20pro%2016%20inch%20laptop%20product%20photography%20premium%20electronics%20on%20clean%20white%20background&width=400&height=400&seq=goal-lot-img-2&orientation=squarish',
    '3': 'https://readdy.ai/api/search-image?query=tesla%20model%203%20performance%20electric%20car%20product%20photography%20modern%20vehicle%20on%20clean%20white%20background&width=400&height=400&seq=goal-lot-img-3&orientation=squarish',
    '4': 'https://readdy.ai/api/search-image?query=hermes%20birkin%20bag%20luxury%20handbag%20product%20photography%20on%20clean%20white%20background&width=400&height=400&seq=goal-lot-img-4&orientation=squarish',
    '5': 'https://readdy.ai/api/search-image?query=playstation%205%20gaming%20console%20bundle%20product%20photography%20electronics%20on%20clean%20white%20background&width=400&height=400&seq=goal-lot-img-5&orientation=squarish',
    '6': 'https://readdy.ai/api/search-image?query=luxury%20yacht%20experience%20mediterranean%20product%20photography%20boat%20on%20clean%20white%20background&width=400&height=400&seq=goal-lot-img-6&orientation=squarish',
    '7': 'https://readdy.ai/api/search-image?query=patek%20philippe%20nautilus%20luxury%20watch%20product%20photography%20on%20clean%20white%20background&width=400&height=400&seq=goal-lot-img-7&orientation=squarish',
};

const fallbackLotImage = 'https://readdy.ai/api/search-image?query=charity%20auction%20generic%20item%20placeholder%20product%20clean%20white%20background%20minimal&width=400&height=400&seq=goal-lot-fallback&orientation=squarish';

export default function GoalDetailPage() {
    const { id } = useParams();
    const goal = mockGoals.find(g => g.id === id);

    if (!goal) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <i className="ri-flag-line text-6xl text-gray-300 mb-4 block"></i>
                    <p className="text-gray-600 text-lg mb-4">Goal not found</p>
                    <Link to="/marketplace" className="px-6 py-2.5 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors whitespace-nowrap cursor-pointer inline-block">
                        Back to Marketplace
                    </Link>
                </div>
            </div>
        );
    }

    const organization = mockOrganizations.find(o => o.id === goal.organizationId);
    const progressPercent = goal.moneyBudget > 0 ? Math.round((goal.moneyRaised / goal.moneyBudget) * 100) : 0;

    const servingLots = mockLots.filter(lot =>
        (lot as any).goalId === goal.id && lot.stage === 'Approved' && lot.isActive
    );

    const servingProducts = products.filter(prod =>
        (prod as any).goalId === goal.id
    );

    const allServingItems = [
        ...servingLots.map(lot => ({
            id: lot.id,
            name: lot.name,
            price: lot.type === 'Auction' ? (lot.currentBid || lot.price) : lot.price,
            image: lotImageMap[lot.id] || fallbackLotImage,
            type: lot.type,
            isLot: true
        })),
        ...servingProducts.map(prod => ({
            id: prod.id,
            name: prod.name,
            price: prod.price,
            image: prod.image,
            type: (prod as any).lotType || 'Simple',
            isLot: false
        }))
    ];

    const totalContributed = allServingItems.reduce((sum, item) => sum + item.price, 0);

    return (
        <div className="min-h-screen bg-gray-50">
            <div className="max-w-[1600px] mx-auto px-6 py-8">
                <div className="flex items-center gap-2 text-sm text-gray-600 mb-8">
                    <Link to="/" className="hover:text-teal-500 cursor-pointer">Home</Link>
                    <i className="ri-arrow-right-s-line"></i>
                    <Link to="/marketplace" className="hover:text-teal-500 cursor-pointer">Marketplace</Link>
                    <i className="ri-arrow-right-s-line"></i>
                    <span className="text-gray-900">{goal.title}</span>
                </div>

                <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden mb-8">
                    <div className="bg-gradient-to-r from-teal-500 to-emerald-500 px-8 py-10">
                        <div className="flex items-start justify-between flex-col lg:flex-row gap-6">
                            <div className="flex-1">
                                <div className="flex items-center gap-3 mb-3">
                                    <span className={`px-3 py-1 text-xs font-semibold rounded-full whitespace-nowrap ${goal.status === 'Reached'
                                            ? 'bg-white/90 text-emerald-700'
                                            : 'bg-white/90 text-teal-700'
                                        }`}>
                                        {goal.status}
                                    </span>
                                    {goal.timeLimitEnd && (
                                        <span className="text-white/80 text-sm flex items-center gap-1">
                                            <i className="ri-calendar-line"></i>
                                            Ends {new Date(goal.timeLimitEnd).toLocaleDateString()}
                                        </span>
                                    )}
                                </div>
                                <h1 className="text-3xl font-bold text-white mb-2">{goal.title}</h1>
                                {organization && (
                                    <Link
                                        to={`/organization/${organization.id}`}
                                        className="flex items-center gap-3 hover:bg-white/10 rounded-lg p-1 -m-1 transition-colors cursor-pointer group/org"
                                    >
                                        <div className="w-10 h-10 rounded-full bg-white/20 overflow-hidden flex-shrink-0">
                                            <img src={organization.avatarImage} alt={organization.name} className="w-full h-full object-cover object-top" />
                                        </div>
                                        <span className="text-white/90 font-medium group-hover/org:text-white transition-colors">{organization.name}</span>
                                        <i className="ri-arrow-right-s-line text-white/60 group-hover/org:text-white transition-colors"></i>
                                    </Link>
                                )}
                            </div>

                            <div className="flex-shrink-0 flex flex-col items-center">
                                <div className="relative w-28 h-28">
                                    <svg className="w-28 h-28 -rotate-90" viewBox="0 0 120 120">
                                        <circle cx="60" cy="60" r="52" stroke="rgba(255,255,255,0.25)" strokeWidth="10" fill="none" />
                                        <circle
                                            cx="60" cy="60" r="52"
                                            stroke="white"
                                            strokeWidth="10"
                                            fill="none"
                                            strokeLinecap="round"
                                            strokeDasharray={`${progressPercent * 3.267} 326.7`}
                                        />
                                    </svg>
                                    <div className="absolute inset-0 flex flex-col items-center justify-center">
                                        <span className="text-2xl font-bold text-white">{progressPercent}%</span>
                                        <span className="text-xs text-white/70">raised</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="grid grid-cols-4 border-t border-gray-100">
                        <div className="px-8 py-5 border-r border-gray-100">
                            <p className="text-xs text-gray-500 mb-1">Budget Goal</p>
                            <p className="text-xl font-bold text-gray-900">${goal.moneyBudget.toLocaleString()}</p>
                        </div>
                        <div className="px-8 py-5 border-r border-gray-100">
                            <p className="text-xs text-gray-500 mb-1">Money Raised</p>
                            <p className="text-xl font-bold text-teal-600">${goal.moneyRaised.toLocaleString()}</p>
                        </div>
                        <div className="px-8 py-5 border-r border-gray-100">
                            <p className="text-xs text-gray-500 mb-1">Items Serving</p>
                            <p className="text-xl font-bold text-gray-900">{allServingItems.length}</p>
                        </div>
                        <div className="px-8 py-5">
                            <p className="text-xs text-gray-500 mb-1">Est. Contribution</p>
                            <p className="text-xl font-bold text-emerald-600">${totalContributed.toLocaleString()}</p>
                        </div>
                    </div>
                </div>

                <div className="grid lg:grid-cols-3 gap-8 mb-8">
                    <div className="lg:col-span-2">
                        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                            <h2 className="text-lg font-bold text-gray-900 mb-4">About This Goal</h2>
                            <p className="text-gray-600 leading-relaxed text-sm">{goal.explanation}</p>

                            {organization && (
                                <div className="mt-6 p-4 bg-gray-50 rounded-lg">
                                    <h3 className="text-sm font-semibold text-gray-900 mb-3">Organization Details</h3>
                                    <div className="grid grid-cols-2 gap-3 text-sm">
                                        <div>
                                            <p className="text-gray-500 text-xs mb-0.5">Contact Email</p>
                                            <p className="text-gray-900">{organization.contactEmail}</p>
                                        </div>
                                        <div>
                                            <p className="text-gray-500 text-xs mb-0.5">Contact Phone</p>
                                            <p className="text-gray-900">{organization.contactPhone}</p>
                                        </div>
                                    </div>
                                    <p className="text-sm text-gray-600 mt-3">{organization.description}</p>
                                </div>
                            )}

                            {goal.timeLimitStart && goal.timeLimitEnd && (
                                <div className="mt-4 flex items-center gap-2 text-sm text-gray-600">
                                    <i className="ri-calendar-line"></i>
                                    <span>
                                        {new Date(goal.timeLimitStart).toLocaleDateString()} - {new Date(goal.timeLimitEnd).toLocaleDateString()}
                                    </span>
                                </div>
                            )}
                        </div>
                    </div>

                    <div>
                        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                            <h2 className="text-lg font-bold text-gray-900 mb-4">
                                <i className="ri-file-list-3-line mr-2"></i>
                                Approvement Documents
                            </h2>
                            {goal.approvementDocuments.length === 0 ? (
                                <p className="text-sm text-gray-500">No documents available</p>
                            ) : (
                                <div className="space-y-3">
                                    {goal.approvementDocuments.map((doc, idx) => (
                                        <a
                                            key={idx}
                                            href={doc.url}
                                            className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg hover:bg-teal-50 transition-colors cursor-pointer group"
                                        >
                                            <div className="w-10 h-10 bg-teal-100 rounded-lg flex items-center justify-center flex-shrink-0">
                                                <i className={`${doc.type === 'PDF' ? 'ri-file-pdf-line' : 'ri-file-line'} text-teal-600 text-lg`}></i>
                                            </div>
                                            <div className="flex-1 min-w-0">
                                                <p className="text-sm font-medium text-gray-900 truncate group-hover:text-teal-600">{doc.name}</p>
                                                <p className="text-xs text-gray-500">{doc.type} &bull; {doc.size}</p>
                                            </div>
                                            <i className="ri-download-line text-gray-400 group-hover:text-teal-600"></i>
                                        </a>
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                <div>
                    <div className="flex items-center justify-between mb-6">
                        <div>
                            <h2 className="text-2xl font-bold text-gray-900">Items Serving This Goal</h2>
                            <p className="text-sm text-gray-600 mt-1">
                                {allServingItems.length} item{allServingItems.length !== 1 ? 's' : ''} contributing to funding
                            </p>
                        </div>
                    </div>

                    {allServingItems.length === 0 ? (
                        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-12 text-center">
                            <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                                <i className="ri-inbox-line text-gray-400 text-2xl"></i>
                            </div>
                            <p className="text-gray-500 text-sm">No items are currently serving this goal</p>
                        </div>
                    ) : (
                        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                            {allServingItems.map(item => (
                                <Link
                                    key={item.id}
                                    to={item.isLot ? `/admin/lots` : `/marketplace/item/${item.id}`}
                                    className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-all cursor-pointer group"
                                >
                                    <div className="h-48 bg-gray-100 overflow-hidden">
                                        <img
                                            src={item.image}
                                            alt={item.name}
                                            className="w-full h-full object-cover object-top group-hover:scale-105 transition-transform duration-300"
                                            onError={(e) => {
                                                (e.target as HTMLImageElement).src = fallbackLotImage;
                                            }}
                                        />
                                    </div>
                                    <div className="p-4">
                                        <div className="flex items-center gap-2 mb-2">
                                            <span className={`px-2 py-0.5 text-xs font-medium rounded-full whitespace-nowrap ${item.type === 'Auction' ? 'bg-amber-100 text-amber-700' :
                                                    item.type === 'Draw' ? 'bg-purple-100 text-purple-700' :
                                                        'bg-teal-100 text-teal-700'
                                                }`}>
                                                {item.type}
                                            </span>
                                        </div>
                                        <h3 className="text-sm font-semibold text-gray-900 line-clamp-2 group-hover:text-teal-600 transition-colors mb-2">
                                            {item.name}
                                        </h3>
                                        <p className="text-lg font-bold text-teal-600">
                                            ${item.price.toLocaleString()}
                                        </p>
                                    </div>
                                </Link>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}