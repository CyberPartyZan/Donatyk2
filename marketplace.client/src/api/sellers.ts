const ACCESS_TOKEN_KEY = 'auth_access_token';

export interface SellerApiDto {
    id: string;
    name: string;
    description: string;
    email: string;
    phoneNumber: string;
    avatarImageUrl: string;
}

interface GetSellersParams {
    search?: string;
    page?: number;
    pageSize?: number;
}

interface ApiErrorPayload {
    message?: string;
    title?: string;
    detail?: string;
}

function getAuthHeaders(): Record<string, string> {
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    return token ? { Authorization: `Bearer ${token}` } : {};
}

function decodeSubFromToken(): string | null {
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    if (!token) return null;

    try {
        const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/'))) as { sub?: string };
        return payload.sub ?? null;
    } catch {
        return null;
    }
}

async function parseError(response: Response): Promise<string> {
    try {
        const payload = (await response.json()) as ApiErrorPayload;
        return payload.message ?? payload.detail ?? payload.title ?? `Request failed (${response.status}).`;
    } catch {
        return `Request failed (${response.status}).`;
    }
}

export async function getCurrentSeller(): Promise<SellerApiDto | null> {
    const sub = decodeSubFromToken();
    if (!sub) return null;

    const response = await fetch(`/api/sellers/by-user/${encodeURIComponent(sub)}`, {
        method: 'GET',
        credentials: 'include',
        headers: { ...getAuthHeaders() },
    });

    if (response.status === 404) return null;
    if (!response.ok) throw new Error(await parseError(response));

    return (await response.json()) as SellerApiDto;
}

export async function getSellers(params?: GetSellersParams): Promise<SellerApiDto[]> {
    const query = new URLSearchParams();

    if (params?.search?.trim()) {
        query.set('search', params.search.trim());
    }

    query.set('page', String(params?.page ?? 1));
    query.set('pageSize', String(params?.pageSize ?? 500));

    const response = await fetch(`/api/sellers?${query.toString()}`, {
        method: 'GET',
        credentials: 'include',
    });

    if (!response.ok) {
        throw new Error(await parseError(response));
    }

    return (await response.json()) as SellerApiDto[];
}

export async function updateSeller(id: string, payload: SellerApiDto): Promise<void> {
    const response = await fetch(`/api/sellers/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            ...getAuthHeaders(),
        },
        credentials: 'include',
        body: JSON.stringify(payload),
    });

    if (!response.ok) {
        throw new Error(await parseError(response));
    }
}

export async function deleteSeller(id: string): Promise<void> {
    const response = await fetch(`/api/sellers/${id}`, {
        method: 'DELETE',
        headers: {
            ...getAuthHeaders(),
        },
        credentials: 'include',
    });

    if (!response.ok) {
        throw new Error(await parseError(response));
    }
}