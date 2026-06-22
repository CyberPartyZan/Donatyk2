import { createContext, useContext, useState, useCallback, useEffect, useRef, type ReactNode } from 'react';
import { mockAccount } from '@/mocks/account';

interface AccountInfo {
    email: string;
    phoneNumber: string;
    userName: string;
    emailConfirmed: boolean;
    isAdmin: boolean;
    avatarUrl: string;
}

interface AuthApiResponse {
    accessToken: string;
    refreshToken: string;
    isLockedOut?: boolean;
    requiresEmailConfirmation?: boolean;
}

interface UserDto {
    id: string;
    email: string;
    emailConfirmed: boolean;
    lockoutEnabled: boolean;
    lockoutEnd?: string | null;
}

interface AuthContextType {
    isLoggedIn: boolean;
    account: AccountInfo | null;
    login: (email: string, password: string) => Promise<{ success: boolean; error?: string }>;
    logout: () => void;
    resendConfirmation: () => Promise<boolean>;
    changeEmail: (newEmail: string) => Promise<boolean>;
    sendResetEmail: (email: string) => Promise<boolean>;
    resetPassword: (password: string) => Promise<boolean>;
    confirmEmail: (userId: string, token: string) => Promise<boolean>;
}

const AuthContext = createContext<AuthContextType | null>(null);
const ACCESS_TOKEN_KEY = 'auth_access_token';
const ACCOUNT_KEY = 'auth_account';

function decodeJwtPayload(token: string): Record<string, unknown> | null {
    try {
        const parts = token.split('.');
        if (parts.length !== 3) return null;
        const normalized = parts[1].replace(/-/g, '+').replace(/_/g, '/');
        const padded = normalized + '='.repeat((4 - (normalized.length % 4)) % 4);
        return JSON.parse(atob(padded)) as Record<string, unknown>;
    } catch {
        return null;
    }
}

function getRoles(payload: Record<string, unknown> | null): string[] {
    if (!payload) return [];
    const role = payload.role ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (Array.isArray(role)) return role.map(String);
    if (typeof role === 'string') return [role];
    return [];
}

export function AuthProvider({ children }: { children: ReactNode }) {
    const [accessToken, setAccessToken] = useState<string | null>(() => localStorage.getItem(ACCESS_TOKEN_KEY));
    const [isLoggedIn, setIsLoggedIn] = useState<boolean>(() => !!localStorage.getItem(ACCESS_TOKEN_KEY));
    const [account, setAccount] = useState<AccountInfo | null>(() => {
        const stored = localStorage.getItem(ACCOUNT_KEY);
        if (!stored) return null;
        try { return JSON.parse(stored) as AccountInfo; } catch { return null; }
    });

    const refreshPromiseRef = useRef<Promise<string | null> | null>(null);

    const clearAuth = useCallback(() => {
        localStorage.removeItem(ACCESS_TOKEN_KEY);
        localStorage.removeItem(ACCOUNT_KEY);
        localStorage.removeItem('auth_logged_in');
        setAccessToken(null);
        setIsLoggedIn(false);
        setAccount(null);
    }, []);

    const setAuthToken = useCallback((token: string | null) => {
        if (!token) {
            localStorage.removeItem(ACCESS_TOKEN_KEY);
            setAccessToken(null);
            setIsLoggedIn(false);
            return;
        }
        localStorage.setItem(ACCESS_TOKEN_KEY, token);
        localStorage.setItem('auth_logged_in', 'true');
        setAccessToken(token);
        setIsLoggedIn(true);
    }, []);

    const refreshAccessToken = useCallback(async (): Promise<string | null> => {
        if (refreshPromiseRef.current) return refreshPromiseRef.current;

        refreshPromiseRef.current = (async () => {
            const response = await fetch('/api/auth/refresh', {
                method: 'POST',
                credentials: 'include',
            });

            if (!response.ok) {
                clearAuth();
                return null;
            }

            const payload = (await response.json()) as AuthApiResponse;
            if (!payload.accessToken) {
                clearAuth();
                return null;
            }

            setAuthToken(payload.accessToken);
            return payload.accessToken;
        })();

        try {
            return await refreshPromiseRef.current;
        } finally {
            refreshPromiseRef.current = null;
        }
    }, [clearAuth, setAuthToken]);

    const authFetch = useCallback(async (url: string, init?: RequestInit, allowRefresh = true): Promise<Response> => {
        const headers = new Headers(init?.headers ?? {});
        if (accessToken) {
            headers.set('Authorization', `Bearer ${accessToken}`);
        }

        const response = await fetch(url, {
            ...init,
            headers,
            credentials: 'include',
        });

        if (response.status !== 401 || !allowRefresh) {
            return response;
        }

        const refreshed = await refreshAccessToken();
        if (!refreshed) return response;

        const retryHeaders = new Headers(init?.headers ?? {});
        retryHeaders.set('Authorization', `Bearer ${refreshed}`);

        return fetch(url, {
            ...init,
            headers: retryHeaders,
            credentials: 'include',
        });
    }, [accessToken, refreshAccessToken]);

    const loadAccount = useCallback(async (token: string): Promise<AccountInfo | null> => {
        const payload = decodeJwtPayload(token);
        const sub = payload?.sub;
        if (typeof sub !== 'string' || !sub) return null;

        const roles = getRoles(payload);
        const isAdmin = roles.includes('Admin');

        const response = await authFetch(`/api/users/${sub}`, { method: 'GET' });
        if (!response.ok) return null;

        const user = (await response.json()) as UserDto;
        const accountInfo: AccountInfo = {
            email: user.email,
            userName: user.email.split('@')[0],
            phoneNumber: mockAccount.phoneNumber,
            emailConfirmed: user.emailConfirmed,
            isAdmin,
            avatarUrl: mockAccount.avatarUrl,
        };

        localStorage.setItem(ACCOUNT_KEY, JSON.stringify(accountInfo));
        setAccount(accountInfo);
        return accountInfo;
    }, [authFetch]);

    useEffect(() => {
        let active = true;

        const bootstrap = async () => {
            const currentToken = accessToken;
            const tokenToUse = currentToken ?? (await refreshAccessToken());
            if (!active || !tokenToUse) return;

            const loaded = await loadAccount(tokenToUse);
            if (!loaded && active) {
                clearAuth();
            }
        };

        void bootstrap();
        return () => { active = false; };
    }, [accessToken, clearAuth, loadAccount, refreshAccessToken]);

    const login = useCallback(async (email: string, password: string) => {
        if (!email || !password) {
            return { success: false, error: 'Email and password are required.' };
        }

        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({ email, password }),
        });

        if (!response.ok) {
            if (response.status === 401) {
                return { success: false, error: 'Invalid email or password.' };
            }
            return { success: false, error: 'Login failed. Please try again.' };
        }

        const payload = (await response.json()) as AuthApiResponse;

        if (payload.requiresEmailConfirmation) {
            return { success: false, error: 'Please confirm your email before logging in.' };
        }

        if (payload.isLockedOut) {
            return { success: false, error: 'Account is locked. Please try again later.' };
        }

        if (!payload.accessToken) {
            return { success: false, error: 'Login failed. Access token missing.' };
        }

        setAuthToken(payload.accessToken);
        const loaded = await loadAccount(payload.accessToken);
        if (!loaded) {
            clearAuth();
            return { success: false, error: 'Unable to load account details.' };
        }

        return { success: true };
    }, [clearAuth, loadAccount, setAuthToken]);

    const logout = useCallback(() => {
        void authFetch('/api/auth/logout', { method: 'POST' }, false);
        clearAuth();
    }, [authFetch, clearAuth]);

    const resendConfirmation = useCallback(async () => {
        if (!account?.email) return false;
        const response = await fetch('/api/auth/resend-confirmation', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({
                email: account.email,
                redirectUrl: `${window.location.origin}/confirm-email`,
            }),
        });
        return response.ok;
    }, [account?.email]);

    const changeEmail = useCallback(async (_newEmail: string) => {
        return false;
    }, []);

    const sendResetEmail = useCallback(async (email: string) => {
        if (!email) return false;
        const response = await fetch('/api/auth/forgot-password', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({ email }),
        });
        return response.ok;
    }, []);

    const resetPassword = useCallback(async (_password: string) => {
        return false;
    }, []);

    const confirmEmail = useCallback(async (userId: string, token: string) => {
        if (!userId || !token) return false;

        const response = await fetch('/api/auth/confirm-email', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({ userId, token }),
        });

        if (!response.ok) return false;

        if (accessToken) {
            await loadAccount(accessToken);
        }

        return true;
    }, [accessToken, loadAccount]);

    return (
        <AuthContext.Provider
            value={{
                isLoggedIn,
                account,
                login,
                logout,
                resendConfirmation,
                changeEmail,
                sendResetEmail,
                resetPassword,
                confirmEmail,
            }}
        >
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error('useAuth must be used within AuthProvider');
    return ctx;
}