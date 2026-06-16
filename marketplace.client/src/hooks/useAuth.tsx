import { createContext, useContext, useState, useCallback, type ReactNode } from 'react';
import { mockAccount } from '@/mocks/account';

interface AccountInfo {
    email: string;
    phoneNumber: string;
    userName: string;
    emailConfirmed: boolean;
    isAdmin: boolean;
    avatarUrl: string;
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
    confirmEmail: () => Promise<boolean>;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [isLoggedIn, setIsLoggedIn] = useState(() => {
        return localStorage.getItem('auth_logged_in') === 'true';
    });
    const [account, setAccount] = useState<AccountInfo | null>(() => {
        const stored = localStorage.getItem('auth_account');
        if (stored) {
            try {
                return JSON.parse(stored);
            } catch {
                return null;
            }
        }
        return null;
    });

    const login = useCallback(async (email: string, password: string) => {
        if (!email || !password) {
            return { success: false, error: 'Email and password are required.' };
        }
        if (password.length < 6) {
            return { success: false, error: 'Password must be at least 6 characters.' };
        }
        const accountData: AccountInfo = {
            ...mockAccount,
            email,
        };
        localStorage.setItem('auth_logged_in', 'true');
        localStorage.setItem('auth_account', JSON.stringify(accountData));
        setIsLoggedIn(true);
        setAccount(accountData);
        return { success: true };
    }, []);

    const logout = useCallback(() => {
        localStorage.removeItem('auth_logged_in');
        localStorage.removeItem('auth_account');
        setIsLoggedIn(false);
        setAccount(null);
    }, []);

    const resendConfirmation = useCallback(async () => {
        return true;
    }, []);

    const changeEmail = useCallback(async (newEmail: string) => {
        if (!newEmail) return false;
        const updated = { ...account!, email: newEmail };
        localStorage.setItem('auth_account', JSON.stringify(updated));
        setAccount(updated);
        return true;
    }, [account]);

    const sendResetEmail = useCallback(async (_email: string) => {
        return true;
    }, []);

    const resetPassword = useCallback(async (_password: string) => {
        return true;
    }, []);

    const confirmEmail = useCallback(async () => {
        if (!account) return false;
        const updated = { ...account, emailConfirmed: true };
        localStorage.setItem('auth_account', JSON.stringify(updated));
        setAccount(updated);
        return true;
    }, [account]);

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
    if (!ctx) {
        throw new Error('useAuth must be used within AuthProvider');
    }
    return ctx;
}