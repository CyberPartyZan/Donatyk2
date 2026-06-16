import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';

export default function ForgotPasswordPage() {
    const { sendResetEmail } = useAuth();
    const [email, setEmail] = useState('');
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setMessage('');

        if (!email.trim()) {
            setError('Please enter your email address.');
            return;
        }

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email.trim())) {
            setError('Please enter a valid email address.');
            return;
        }

        setLoading(true);
        const ok = await sendResetEmail(email.trim());
        setLoading(false);

        if (ok) {
            setMessage('A password reset link has been sent to your email. Please check your inbox.');
            setEmail('');
        } else {
            setError('Failed to send reset email. Please try again.');
        }
    };

    return (
        <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4 py-12">
            <div className="w-full max-w-md">
                <div className="text-center mb-8">
                    <Link to="/">
                        <img
                            src="https://public.readdy.ai/ai/img_res/fd4376ec-da1c-49df-8529-c6f422339bdf.png"
                            alt="Logo"
                            className="h-12 w-auto mx-auto mb-6"
                        />
                    </Link>
                    <h1 className="text-3xl font-bold text-gray-900">Forgot Password</h1>
                    <p className="text-gray-600 mt-2">
                        Enter your email and we&apos;ll send you a password reset link
                    </p>
                </div>

                <div className="bg-white rounded-lg shadow-sm p-8">
                    <form onSubmit={handleSubmit} noValidate>
                        {error && (
                            <div className="mb-6 px-4 py-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700 flex items-start gap-2">
                                <i className="ri-error-warning-line text-red-500 mt-0.5"></i>
                                <span>{error}</span>
                            </div>
                        )}

                        {message && (
                            <div className="mb-6 px-4 py-3 bg-green-50 border border-green-200 rounded-lg text-sm text-green-700 flex items-start gap-2">
                                <i className="ri-checkbox-circle-line text-green-500 mt-0.5"></i>
                                <span>{message}</span>
                            </div>
                        )}

                        <div className="space-y-5">
                            <div>
                                <label htmlFor="forgot-email" className="block text-sm font-medium text-gray-700 mb-1.5">
                                    Email Address
                                </label>
                                <input
                                    id="forgot-email"
                                    type="email"
                                    value={email}
                                    onChange={(e) => {
                                        setEmail(e.target.value);
                                        setError('');
                                        setMessage('');
                                    }}
                                    placeholder="you@example.com"
                                    required
                                    autoComplete="email"
                                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                />
                            </div>

                            <button
                                type="submit"
                                disabled={loading}
                                className="w-full py-3 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors text-base font-semibold whitespace-nowrap cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                                {loading ? 'Sending...' : 'Send Password Reset Email'}
                            </button>
                        </div>
                    </form>

                    <div className="mt-6 pt-6 border-t border-gray-200 text-center">
                        <Link
                            to="/login"
                            className="inline-flex items-center gap-2 text-sm text-teal-600 hover:text-teal-700 font-medium transition-colors cursor-pointer"
                        >
                            <i className="ri-arrow-left-line"></i>
                            Back to Login
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
}