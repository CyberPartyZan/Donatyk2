const ACCESS_TOKEN_KEY = 'auth_access_token';

export interface ShipmentApiDto {
    id: string;
    orderId: string;
    orderNumber: string;
    lotName: string;
    lotImage: string;
    buyerName: string;
    carrier: string;
    trackingNumber: string;
    address: string;
    recipientName: string;
    recipientPhone: string;
    status: 'Created' | 'Processing' | 'Shipped' | 'InTransit' | 'OutForDelivery' | 'Delivered';
    processedAt: string | null;
    deliveredAt: string | null;
    notes: string;
}

export interface ShipmentStatisticsDto {
    total: number;
    pending: number;
    inTransit: number;
    delivered: number;
}

const getAuthHeaders = (): Record<string, string> => {
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    return token ? { Authorization: `Bearer ${token}` } : {};
};

async function parseError(response: Response): Promise<string> {
    try {
        const payload = (await response.json()) as { message?: string; detail?: string; title?: string };
        return payload.message ?? payload.detail ?? payload.title ?? `Request failed (${response.status}).`;
    } catch {
        return `Request failed (${response.status}).`;
    }
}

export async function getShipments(params: {
    search?: string;
    page?: number;
    pageSize?: number;
    onlyPending?: boolean;
    sellerId?: string;
}): Promise<{ items: ShipmentApiDto[]; totalCount: number }> {
    const query = new URLSearchParams();
    query.set('page', String(params.page ?? 1));
    query.set('pageSize', String(params.pageSize ?? 20));
    query.set('onlyPending', String(params.onlyPending ?? false));

    if (params.search?.trim()) query.set('search', params.search.trim());
    if (params.sellerId) query.set('sellerId', params.sellerId);

    const response = await fetch(`/api/shipments?${query.toString()}`, {
        method: 'GET',
        credentials: 'include',
        headers: { ...getAuthHeaders() },
    });

    if (!response.ok) throw new Error(await parseError(response));

    const items = (await response.json()) as ShipmentApiDto[];
    const totalCountRaw = Number(response.headers.get('X-Total-Count'));
    const totalCount = Number.isFinite(totalCountRaw) ? totalCountRaw : items.length;

    return { items, totalCount };
}

export async function getShipmentStatistics(params?: { search?: string; sellerId?: string }): Promise<ShipmentStatisticsDto> {
    const query = new URLSearchParams();
    if (params?.search?.trim()) query.set('search', params.search.trim());
    if (params?.sellerId) query.set('sellerId', params.sellerId);

    const response = await fetch(`/api/shipments/statistics?${query.toString()}`, {
        method: 'GET',
        credentials: 'include',
        headers: { ...getAuthHeaders() },
    });

    if (!response.ok) throw new Error(await parseError(response));
    return (await response.json()) as ShipmentStatisticsDto;
}

export async function takeShipmentIntoProcessing(id: string, trackingNumber: string): Promise<void> {
    const response = await fetch(
        `/api/shipments/${id}/take-into-processing?trackingNumber=${encodeURIComponent(trackingNumber)}`,
        { method: 'PUT', credentials: 'include', headers: { ...getAuthHeaders() } }
    );

    if (!response.ok) throw new Error(await parseError(response));
}