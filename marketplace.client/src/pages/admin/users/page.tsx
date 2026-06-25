import { useCallback, useEffect, useMemo, useState } from 'react';
import Pagination from '@/components/base/Pagination';

const ITEMS_PER_PAGE = 10;
const ACCESS_TOKEN_KEY = 'auth_access_token';

interface ApiUserDto {
    id: string;
    email: string;
    emailConfirmed: boolean;
    lockoutEnabled: boolean;
    lockoutEnd?: string | null;
}

interface User {
    id: string;
    email: string;
    emailConfirmed: boolean;
    lockoutEnabled: boolean;
    lockoutEnd: string | null;
}

const getAuthHeader = (): Record<string, string> => {
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    return token ? { Authorization: `Bearer ${token}` } : {};
};

const getResponseMessage = async (response: Response): Promise<string> => {
    try {
        const payload = await response.json() as { message?: string; title?: string; detail?: string };
        return payload.message ?? payload.detail ?? payload.title ?? `Request failed: ${response.status}`;
    } catch {
        return `Request failed: ${response.status}`;
    }
};

const mapApiUser = (dto: ApiUserDto): User => ({
    id: dto.id,
    email: dto.email,
    emailConfirmed: dto.emailConfirmed,
    lockoutEnabled: dto.lockoutEnabled,
    lockoutEnd: dto.lockoutEnd ?? null,
});

export default function UsersAdmin() {
    const [users, setUsers] = useState<User[]>([]);
    const [searchQuery, setSearchQuery] = useState('');
    const [activeSearchQuery, setActiveSearchQuery] = useState('');
    const [currentPage, setCurrentPage] = useState(1);
    const [totalUsers, setTotalUsers] = useState(0);

    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [mutationError, setMutationError] = useState<string | null>(null);

    const loadUsers = useCallback(async (search: string, page: number) => {
        setIsLoading(true);
        setError(null);

        const params = new URLSearchParams({
            page: String(page),
            pageSize: String(ITEMS_PER_PAGE),
        });

        if (search.trim()) {
            params.set('search', search.trim());
        }

        try {
            const response = await fetch(`/api/users?${params.toString()}`, {
                method: 'GET',
                headers: {
                    ...getAuthHeader(),
                },
                credentials: 'include',
            });

            if (!response.ok) {
                throw new Error(await getResponseMessage(response));
            }

            const data = (await response.json()) as ApiUserDto[];
            setUsers((Array.isArray(data) ? data : []).map(mapApiUser));

            const totalCountHeader = response.headers.get('X-Total-Count');
            const totalCount = totalCountHeader ? Number(totalCountHeader) : NaN;
            setTotalUsers(Number.isFinite(totalCount) && totalCount >= 0 ? totalCount : data.length);
        } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to load users');
            setUsers([]);
            setTotalUsers(0);
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        void loadUsers(activeSearchQuery, currentPage);
    }, [activeSearchQuery, currentPage, loadUsers]);

    const handleSearch = () => {
        setCurrentPage(1);
        setActiveSearchQuery(searchQuery.trim());
    };

    const handleUnlock = async (user: User) => {
        setMutationError(null);

        try {
            const response = await fetch(`/api/users/${user.id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    ...getAuthHeader(),
                },
                credentials: 'include',
                body: JSON.stringify({
                    id: user.id,
                    email: user.email,
                    emailConfirmed: user.emailConfirmed,
                    lockoutEnabled: false,
                    lockoutEnd: user.lockoutEnd,
                }),
            });

            if (!response.ok) {
                setMutationError(await getResponseMessage(response));
                return;
            }

            setUsers(prev =>
                prev.map(u => (u.id === user.id ? { ...u, lockoutEnabled: false } : u))
            );
        } catch (e) {
            setMutationError(e instanceof Error ? e.message : 'Failed to unlock user');
        }
    };

    const formatDate = (dateString: string | null) => {
        if (!dateString) return '—';
        return new Date(dateString).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });
    };

    const isLocked = (user: User) => {
        if (!user.lockoutEnabled || !user.lockoutEnd) return false;
        return new Date(user.lockoutEnd) > new Date();
    };

    const lockedCount = useMemo(() => users.filter(isLocked).length, [users]);
    const hasNextPage = users.length === ITEMS_PER_PAGE;
    const totalPages = useMemo(
        () => Math.max(1, Math.ceil(totalUsers / ITEMS_PER_PAGE)),
        [totalUsers]
    );

    return (
        <div className="bg-white rounded-lg shadow-sm">
            <div className="p-6 border-b border-gray-200">
                <div className="flex items-center justify-between mb-4">
                    <div>
                        <h2 className="text-2xl font-semibold text-gray-900">Users Management</h2>
                        <p className="text-sm text-gray-600 mt-1">Manage user accounts and access control</p>
                    </div>
                    <div className="flex items-center gap-4">
                        <div className="flex items-center gap-2 px-4 py-2 bg-gray-50 rounded-lg">
                            <i className="ri-user-line text-gray-600"></i>
                            <span className="text-sm font-medium text-gray-900">{totalUsers} Total Users</span>
                        </div>
                        <div className="flex items-center gap-2 px-4 py-2 bg-red-50 rounded-lg">
                            <i className="ri-lock-line text-red-600"></i>
                            <span className="text-sm font-medium text-red-900">{lockedCount} Locked</span>
                        </div>
                    </div>
                </div>

                <div className="flex items-center gap-3">
                    <div className="relative flex-1">
                        <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                        <input
                            type="text"
                            placeholder="Search users by email..."
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                            className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                        />
                    </div>
                    <button
                        onClick={handleSearch}
                        className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                    >
                        <i className="ri-search-line mr-1.5"></i>Search
                    </button>
                </div>

                {error && <p className="mt-3 text-sm text-red-600">{error}</p>}
                {mutationError && <p className="mt-2 text-sm text-red-600">{mutationError}</p>}
            </div>

            <div className="overflow-x-auto">
                {isLoading ? (
                    <div className="text-center py-12 text-gray-600">Loading users...</div>
                ) : users.length === 0 ? (
                    <div className="text-center py-12">
                        <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                            <i className="ri-user-line text-gray-400 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-medium text-gray-900 mb-2">No users found</h3>
                        <p className="text-gray-600">
                            {activeSearchQuery ? 'Try adjusting your search terms' : 'No users have been registered yet'}
                        </p>
                    </div>
                ) : (
                    <>
                        <table className="w-full">
                            <thead className="bg-gray-50 border-b border-gray-200">
                                <tr>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email Confirmed</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Lockout Status</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Lockout Date</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                                </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                                {users.map(user => (
                                    <tr key={user.id} className="hover:bg-gray-50 transition-colors">
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <div className="flex items-center gap-2">
                                                <div className="w-8 h-8 bg-teal-100 rounded-full flex items-center justify-center flex-shrink-0">
                                                    <i className="ri-user-line text-teal-600 text-sm"></i>
                                                </div>
                                                <span className="text-sm font-medium text-gray-900">{user.email}</span>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            {user.emailConfirmed ? (
                                                <span className="inline-flex items-center gap-1 px-2 py-1 bg-green-100 text-green-800 text-xs font-medium rounded-full whitespace-nowrap">
                                                    <i className="ri-checkbox-circle-fill"></i>
                                                    Confirmed
                                                </span>
                                            ) : (
                                                <span className="inline-flex items-center gap-1 px-2 py-1 bg-gray-100 text-gray-600 text-xs font-medium rounded-full whitespace-nowrap">
                                                    <i className="ri-close-circle-fill"></i>
                                                    Not Confirmed
                                                </span>
                                            )}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            {isLocked(user) ? (
                                                <span className="inline-flex items-center gap-1 px-2 py-1 bg-red-100 text-red-800 text-xs font-medium rounded-full whitespace-nowrap">
                                                    <i className="ri-lock-fill"></i>
                                                    Locked
                                                </span>
                                            ) : (
                                                <span className="inline-flex items-center gap-1 px-2 py-1 bg-green-100 text-green-800 text-xs font-medium rounded-full whitespace-nowrap">
                                                    <i className="ri-lock-unlock-fill"></i>
                                                    Active
                                                </span>
                                            )}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                                            {formatDate(user.lockoutEnd)}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            {isLocked(user) ? (
                                                <button
                                                    onClick={() => void handleUnlock(user)}
                                                    className="inline-flex items-center gap-1 px-3 py-1 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors whitespace-nowrap cursor-pointer"
                                                >
                                                    <i className="ri-lock-unlock-line"></i>
                                                    Unlock Account
                                                </button>
                                            ) : (
                                                <span className="text-sm text-gray-400">No action needed</span>
                                            )}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                        <div className="p-4">
                            <Pagination
                                currentPage={currentPage}
                                totalPages={totalPages}
                                onPageChange={(p) => setCurrentPage(Math.max(1, p))}
                            />
                        </div>
                    </>
                )}
            </div>
        </div>
    );
}