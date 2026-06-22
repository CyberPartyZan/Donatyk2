import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';

export default function ConfirmEmailPage() {
    const { account, confirmEmail } = useAuth();
    const [searchParams] = useSearchParams();
    const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');

    const userId = searchParams.get('userId');
    const token = searchParams.get('token');

    useEffect(() => {
        const doConfirm = async () => {
            if (!userId || !token) {
                setStatus('error');
                return;
            }

            try {
                const ok = await confirmEmail(userId, token);
                setStatus(ok ? 'success' : 'error');
            } catch {
                setStatus('error');
            }
        };

        void doConfirm();
    }, [userId, token, confirmEmail]);

    return (
        <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4">
            <div className="max-w-md w-full bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center">
                {status === 'loading' && (
                    <>
                        <div className="w-16 h-16 flex items-center justify-center mx-auto mb-6">
                            <i className="ri-loader-4-line text-5xl text-teal-500 animate-spin"></i>
                        </div>
                        <h1 className="text-2xl font-bold text-gray-900 mb-3">Confirming Your Email</h1>
                        <p className="text-gray-600 text-sm">Please wait while we verify your email address...</p>
                    </>
                )}

                {status === 'success' && (
                    <>
                        <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-6">
                            <i className="ri-check-line text-3xl text-green-600"></i>
                        </div>
                        <h1 className="text-2xl font-bold text-gray-900 mb-3">Email Confirmed!</h1>
                        <p className="text-gray-600 text-sm mb-4">
                            Your email address <strong>{account?.email}</strong> has been successfully verified.
                        </p>
                        <p className="text-gray-500 text-xs mb-6">You can now enjoy all features of the platform.</p>
                        <Link
                            to="/marketplace"
                            className="inline-block px-8 py-3 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                        >
                            Go to Marketplace
                        </Link>
                    </>
                )}

                {status === 'error' && (
                    <>
                        <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-6">
                            <i className="ri-close-line text-3xl text-red-600"></i>
                        </div>
                        <h1 className="text-2xl font-bold text-gray-900 mb-3">Confirmation Failed</h1>
                        <p className="text-gray-600 text-sm mb-6">
                            The confirmation link is invalid or has expired. Please request a new confirmation email.
                        </p>
                        <div className="flex flex-col gap-3">
                            <Link
                                to="/admin/account"
                                className="px-6 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                            >
                                Go to Account
                            </Link>
                            <Link
                                to="/marketplace"
                                className="px-6 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap"
                            >
                                Back to Marketplace
                            </Link>
                        </div>
                    </>
                )}
            </div>
        </div>
    );
}