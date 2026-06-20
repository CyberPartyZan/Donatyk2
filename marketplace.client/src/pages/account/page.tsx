import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { charityDirections } from '@/mocks/charity-directions';
import { mockReports } from '@/mocks/reports';

export default function AccountInfoPage() {
    const { account, logout, resendConfirmation, changeEmail } = useAuth();
    const navigate = useNavigate();

    const comingSoon = true; // Set to false when the feature is implemented

    const [showChangeEmail, setShowChangeEmail] = useState(false);
    const [newEmail, setNewEmail] = useState('');
    const [emailError, setEmailError] = useState('');
    const [emailSuccess, setEmailSuccess] = useState('');
    const [confirmLoading, setConfirmLoading] = useState(false);
    const [confirmMsg, setConfirmMsg] = useState('');

    const [selectedDirections, setSelectedDirections] = useState<string[]>(() => {
        const saved = localStorage.getItem('charity_directions');
        return saved ? JSON.parse(saved) : ['Education, Science and Youth Development'];
    });
    const [showDirectionsEditor, setShowDirectionsEditor] = useState(false);

    const totalMoneySpent = mockReports.reduce((sum, r) => sum + r.moneySpent, 0);

    const toggleDirection = (dir: string) => {
        setSelectedDirections(prev => {
            const next = prev.includes(dir) ? prev.filter(d => d !== dir) : [...prev, dir];
            localStorage.setItem('charity_directions', JSON.stringify(next));
            return next;
        });
    };

    if (!account) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <i className="ri-user-line text-6xl text-gray-300 mb-4 block"></i>
                    <p className="text-gray-600 text-lg mb-4">You are not logged in.</p>
                    <Link
                        to="/login"
                        className="px-6 py-2.5 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors whitespace-nowrap cursor-pointer inline-block"
                    >
                        Go to Login
                    </Link>
                </div>
            </div>
        );
    }

    const handleResendConfirmation = async () => {
        setConfirmLoading(true);
        setConfirmMsg('');
        const ok = await resendConfirmation();
        if (ok) {
            setConfirmMsg('Confirmation email has been sent. Please check your inbox.');
        } else {
            setConfirmMsg('Failed to send confirmation email. Please try again.');
        }
        setConfirmLoading(false);
    };

    const handleChangeEmail = async () => {
        setEmailError('');
        setEmailSuccess('');
        if (!newEmail.trim()) {
            setEmailError('Email is required.');
            return;
        }
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(newEmail.trim())) {
            setEmailError('Please enter a valid email address.');
            return;
        }
        const ok = await changeEmail(newEmail.trim());
        if (ok) {
            setEmailSuccess('Email updated successfully.');
            setNewEmail('');
            setShowChangeEmail(false);
        } else {
            setEmailError('Failed to update email.');
        }
    };

    const handleLogout = () => {
        logout();
        navigate('/');
    };

    return (
        <div>
            <div className="mb-6">
                <h2 className="text-2xl font-bold text-gray-900">Account Information</h2>
                <p className="text-gray-600 mt-1">Manage your personal account settings</p>
            </div>

            {/* Profile Header */}
            <div className="bg-white rounded-lg shadow-sm p-8 mb-6">
                <div className="flex items-center gap-6">
                    <div className="w-20 h-20 bg-gray-200 rounded-full overflow-hidden flex-shrink-0">
                        <img
                            src={account.avatarUrl}
                            alt={account.userName}
                            className="w-full h-full object-cover object-top"
                        />
                    </div>
                    <div>
                        <h3 className="text-xl font-bold text-gray-900">{account.userName}</h3>
                        <div className="flex items-center gap-3 mt-2">
                            {account.emailConfirmed ? (
                                <span className="inline-flex items-center gap-1 px-2.5 py-0.5 bg-green-100 text-green-700 text-xs font-medium rounded-full whitespace-nowrap">
                                    <i className="ri-checkbox-circle-fill text-xs"></i>
                                    Email Confirmed
                                </span>
                            ) : (
                                <span className="inline-flex items-center gap-1 px-2.5 py-0.5 bg-amber-100 text-amber-700 text-xs font-medium rounded-full whitespace-nowrap">
                                    <i className="ri-error-warning-fill text-xs"></i>
                                    Unconfirmed
                                </span>
                            )}
                        </div>
                    </div>
                </div>
            </div>

            {/* Money Spent Stat */}
            {!comingSoon && <div className="bg-white rounded-lg shadow-sm p-8 mb-6">
                <div className="flex items-center justify-between">
                    <div>
                        <p className="text-sm text-gray-500 mb-1">Total Money Spent on Charity</p>
                        <p className="text-3xl font-bold text-emerald-600">${totalMoneySpent.toLocaleString()}</p>
                    </div>
                    <Link
                        to="/admin/reports"
                        className="px-5 py-2.5 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                    >
                        <i className="ri-file-chart-line"></i>View Reports
                    </Link>
                </div>
            </div>}

            {/* Account Details */}
            <div className="bg-white rounded-lg shadow-sm p-8 mb-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-6">Account Details</h3>
                <div className="space-y-6">
                    {/* Email */}
                    <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-6 border-b border-gray-100">
                        <div className="flex items-start gap-3">
                            <div className="w-10 h-10 bg-teal-100 rounded-lg flex items-center justify-center flex-shrink-0">
                                <i className="ri-mail-line text-xl text-teal-600"></i>
                            </div>
                            <div>
                                <p className="text-sm text-gray-500 mb-0.5">Email Address</p>
                                <p className="text-base font-medium text-gray-900">{account.email}</p>
                            </div>
                        </div>
                        <div className="flex items-center gap-2 ml-13 sm:ml-0">
                            {!account.emailConfirmed && (
                                <button
                                    onClick={handleResendConfirmation}
                                    disabled={confirmLoading}
                                    className="px-4 py-2 text-sm border border-teal-500 text-teal-600 rounded-lg hover:bg-teal-50 transition-colors whitespace-nowrap cursor-pointer disabled:opacity-50"
                                >
                                    {confirmLoading ? 'Sending...' : 'Resend Confirmation'}
                                </button>
                            )}
                            <button
                                onClick={() => {
                                    setShowChangeEmail(!showChangeEmail);
                                    setNewEmail('');
                                    setEmailError('');
                                    setEmailSuccess('');
                                }}
                                className="px-4 py-2 text-sm border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors whitespace-nowrap cursor-pointer"
                            >
                                {showChangeEmail ? 'Cancel' : 'Change Email'}
                            </button>
                        </div>
                    </div>

                    {confirmMsg && (
                        <div className="px-4 py-3 bg-teal-50 border border-teal-200 rounded-lg text-sm text-teal-700">
                            {confirmMsg}
                        </div>
                    )}

                    {showChangeEmail && (
                        <div className="p-5 bg-gray-50 rounded-lg space-y-3">
                            <p className="text-sm font-medium text-gray-700">Enter new email address</p>
                            <div className="flex flex-col sm:flex-row items-start sm:items-center gap-3">
                                <input
                                    type="email"
                                    value={newEmail}
                                    onChange={(e) => {
                                        setNewEmail(e.target.value);
                                        setEmailError('');
                                        setEmailSuccess('');
                                    }}
                                    placeholder="newemail@example.com"
                                    className="flex-1 px-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                                />
                                <button
                                    onClick={handleChangeEmail}
                                    className="px-5 py-2.5 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors text-sm font-medium whitespace-nowrap cursor-pointer"
                                >
                                    Save Email
                                </button>
                            </div>
                            {emailError && <p className="text-sm text-red-600">{emailError}</p>}
                            {emailSuccess && <p className="text-sm text-green-600">{emailSuccess}</p>}
                        </div>
                    )}

                    {/* Phone Number */}
                    <div className="flex items-start gap-3 pb-6 border-b border-gray-100">
                        <div className="w-10 h-10 bg-teal-100 rounded-lg flex items-center justify-center flex-shrink-0">
                            <i className="ri-phone-line text-xl text-teal-600"></i>
                        </div>
                        <div>
                            <p className="text-sm text-gray-500 mb-0.5">Phone Number</p>
                            <p className="text-base font-medium text-gray-900">{account.phoneNumber}</p>
                        </div>
                    </div>

                    {/* Username */}
                    <div className="flex items-start gap-3">
                        <div className="w-10 h-10 bg-teal-100 rounded-lg flex items-center justify-center flex-shrink-0">
                            <i className="ri-user-3-line text-xl text-teal-600"></i>
                        </div>
                        <div>
                            <p className="text-sm text-gray-500 mb-0.5">Username</p>
                            <p className="text-base font-medium text-gray-900">{account.userName}</p>
                        </div>
                    </div>
                </div>
            </div>

            {/* Charity Directions */}
            {!comingSoon && <div className="bg-white rounded-lg shadow-sm p-8 mb-6">
                <div className="flex items-center justify-between mb-6">
                    <div>
                        <h3 className="text-lg font-semibold text-gray-900">Charity Directions</h3>
                        <p className="text-sm text-gray-500 mt-1">Select the charity causes you are interested in supporting</p>
                    </div>
                    <button
                        onClick={() => setShowDirectionsEditor(!showDirectionsEditor)}
                        className="px-4 py-2 text-sm border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors whitespace-nowrap cursor-pointer"
                    >
                        <i className="ri-edit-line mr-2"></i>
                        {showDirectionsEditor ? 'Done' : 'Edit'}
                    </button>
                </div>

                {showDirectionsEditor ? (
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                        {charityDirections.map(dir => (
                            <label key={dir} className="flex items-start gap-3 p-3 rounded-lg border border-gray-200 hover:border-teal-300 cursor-pointer transition-colors">
                                <input
                                    type="checkbox"
                                    checked={selectedDirections.includes(dir)}
                                    onChange={() => toggleDirection(dir)}
                                    className="w-4 h-4 text-teal-600 border-gray-300 rounded focus:ring-teal-500 mt-0.5 cursor-pointer"
                                />
                                <span className="text-sm text-gray-700">{dir}</span>
                            </label>
                        ))}
                    </div>
                ) : (
                    <>
                        {selectedDirections.length === 0 ? (
                            <p className="text-sm text-gray-400">No charity directions selected. Click Edit to choose your interests.</p>
                        ) : (
                            <div className="flex flex-wrap gap-2">
                                {selectedDirections.map(dir => (
                                    <span key={dir} className="px-3 py-1.5 bg-teal-50 text-teal-700 text-sm rounded-full flex items-center gap-1.5">
                                        <i className="ri-heart-line text-xs"></i>
                                        {dir}
                                    </span>
                                ))}
                            </div>
                        )}
                    </>
                )}
            </div>}

            {/* Logout */}
            <div className="bg-white rounded-lg shadow-sm p-8">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Session</h3>
                <p className="text-sm text-gray-600 mb-4">
                    Sign out from your account. You will be redirected to the homepage.
                </p>
                <button
                    onClick={handleLogout}
                    className="px-6 py-2.5 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors font-medium whitespace-nowrap cursor-pointer flex items-center gap-2"
                >
                    <i className="ri-logout-box-line text-lg"></i>
                    Logout
                </button>
            </div>
        </div>
    );
}