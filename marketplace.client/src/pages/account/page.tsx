import { Outlet, useLocation, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';

export default function AdminPanel() {
    const location = useLocation();
    const navigate = useNavigate();
    const { logout, account } = useAuth();

    const isAdmin = account?.isAdmin ?? false;

    const allNavItems = [
        { path: '/admin/categories', label: 'Categories', icon: 'ri-folder-line', adminOnly: true, section: 'admin' },
        { path: '/admin/lots', label: 'Lots', icon: 'ri-auction-line', adminOnly: true, section: 'admin' },
        { path: '/admin/sellers', label: 'Sellers', icon: 'ri-team-line', adminOnly: true, section: 'admin' },
        { path: '/admin/users', label: 'Users', icon: 'ri-user-line', adminOnly: true, section: 'admin' },
        { path: '/admin/goal-management', label: 'Goal Management', icon: 'ri-check-double-line', adminOnly: true, section: 'admin' },
        { path: '/admin/compensations', label: 'Compensations', icon: 'ri-money-dollar-circle-line', adminOnly: true, section: 'admin' },
        { path: '/admin/reports', label: 'Reports', icon: 'ri-file-chart-line', section: 'general' },
        { path: '/admin/media', label: 'Media', icon: 'ri-film-line', adminOnly: true, section: 'admin' },
        { path: '/admin/faqs', label: 'FAQs', icon: 'ri-question-answer-line', adminOnly: true, section: 'admin' },
        { path: '/admin/seller', label: 'My Seller Info', icon: 'ri-store-2-line', section: 'general' },
        { path: '/admin/goals', label: 'Goals', icon: 'ri-flag-line', section: 'general' },
        { path: '/admin/shipments', label: 'Shipments', icon: 'ri-truck-line', section: 'general' },
        { path: '/admin/account', label: 'Account Info', icon: 'ri-user-settings-line', section: 'general' },
    ];

    const navItems = allNavItems.filter(item => !item.adminOnly || isAdmin);

    const handleLogout = () => {
        logout();
        navigate('/');
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <header className="bg-white shadow-sm sticky top-0 z-40">
                <div className="max-w-[1600px] mx-auto px-6 py-4">
                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-4">
                            <Link to="/">
                                <img
                                    src="https://public.readdy.ai/ai/img_res/fd4376ec-da1c-49df-8529-c6f422339bdf.png"
                                    alt="Logo"
                                    className="h-10 w-auto"
                                />
                            </Link>
                            <div className="h-8 w-px bg-gray-300"></div>
                            <h1 className="text-xl font-semibold text-gray-800">Admin Panel</h1>
                        </div>
                        <div className="flex items-center gap-3">
                            <Link
                                to="/marketplace"
                                className="px-4 py-2 text-sm text-gray-600 hover:text-gray-900 transition-colors cursor-pointer flex items-center gap-2 whitespace-nowrap"
                            >
                                <i className="ri-arrow-left-line"></i>
                                Back to Marketplace
                            </Link>
                            <button
                                onClick={handleLogout}
                                className="w-9 h-9 flex items-center justify-center hover:bg-red-50 rounded-lg transition-colors cursor-pointer text-gray-500 hover:text-red-500"
                                title="Logout"
                            >
                                <i className="ri-logout-box-line text-xl"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </header>

            <div className="max-w-[1600px] mx-auto px-6 py-8">
                <div className="flex gap-8">
                    <aside className="w-64 flex-shrink-0">
                        <nav className="bg-white rounded-lg shadow-sm p-4 sticky top-24">
                            <ul className="space-y-1">
                                {(() => {
                                    const adminItems = navItems.filter(item => item.section === 'admin');
                                    const generalItems = navItems.filter(item => item.section === 'general');

                                    return (
                                        <>
                                            {adminItems.length > 0 && (
                                                <>
                                                    <li className="px-3 py-1.5 text-xs font-semibold text-gray-400 uppercase tracking-wider">
                                                        Admin
                                                    </li>
                                                    {adminItems.map((item) => {
                                                        const isActive = location.pathname === item.path;
                                                        return (
                                                            <li key={item.path}>
                                                                <Link
                                                                    to={item.path}
                                                                    className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-colors cursor-pointer whitespace-nowrap ${isActive
                                                                            ? 'bg-teal-500 text-white'
                                                                            : 'text-gray-700 hover:bg-gray-100'
                                                                        }`}
                                                                >
                                                                    <i className={`${item.icon} text-lg`}></i>
                                                                    <span className="font-medium text-sm">{item.label}</span>
                                                                </Link>
                                                            </li>
                                                        );
                                                    })}
                                                    <li className="my-2 border-b border-gray-100"></li>
                                                </>
                                            )}
                                            {generalItems.length > 0 && (
                                                <>
                                                    <li className="px-3 py-1.5 text-xs font-semibold text-gray-400 uppercase tracking-wider">
                                                        General
                                                    </li>
                                                    {generalItems.map((item) => {
                                                        const isActive = location.pathname === item.path;
                                                        return (
                                                            <li key={item.path}>
                                                                <Link
                                                                    to={item.path}
                                                                    className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-colors cursor-pointer whitespace-nowrap ${isActive
                                                                            ? 'bg-teal-500 text-white'
                                                                            : 'text-gray-700 hover:bg-gray-100'
                                                                        }`}
                                                                >
                                                                    <i className={`${item.icon} text-lg`}></i>
                                                                    <span className="font-medium text-sm">{item.label}</span>
                                                                </Link>
                                                            </li>
                                                        );
                                                    })}
                                                </>
                                            )}
                                        </>
                                    );
                                })()}
                            </ul>
                        </nav>
                    </aside>

                    <main className="flex-1 min-w-0">
                        <Outlet />
                    </main>
                </div>
            </div>
        </div>
    );
}