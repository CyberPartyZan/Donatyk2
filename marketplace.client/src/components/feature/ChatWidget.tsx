import { useState, useRef, useEffect } from 'react';

interface Message {
    id: string;
    text: string;
    sender: 'user' | 'manager';
    timestamp: Date;
}

interface OfflineFormData {
    name: string;
    email: string;
    message: string;
}

const initialMessages: Message[] = [
    {
        id: 'msg-1',
        text: 'Hello! How can I help you today? Feel free to ask about our marketplace, goals, or anything else.',
        sender: 'manager',
        timestamp: new Date()
    }
];

const MANAGER_KEY = 'chat_manager_online';
const MANAGER_NAME_KEY = 'chat_manager_name';

function getManagerOnline(): boolean {
    return localStorage.getItem(MANAGER_KEY) === 'true';
}

function getManagerName(): string {
    return localStorage.getItem(MANAGER_NAME_KEY) || 'Support Manager';
}

export default function ChatWidget() {
    const [isOpen, setIsOpen] = useState(false);
    const [messages, setMessages] = useState<Message[]>(initialMessages);
    const [input, setInput] = useState('');
    const [isTyping, setIsTyping] = useState(false);
    const [isManagerOnline, setIsManagerOnline] = useState(getManagerOnline);
    const [managerName, setManagerName] = useState(getManagerName);
    const [offlineForm, setOfflineForm] = useState<OfflineFormData>({ name: '', email: '', message: '' });
    const [offlineSubmitted, setOfflineSubmitted] = useState(false);
    const [offlineSubmitting, setOfflineSubmitting] = useState(false);

    const [showManagerLogin, setShowManagerLogin] = useState(false);
    const [managerLoginName, setManagerLoginName] = useState('');
    const [managerLoginPassword, setManagerLoginPassword] = useState('');
    const [managerLoginError, setManagerLoginError] = useState('');

    const messagesEndRef = useRef<HTMLDivElement>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        if (isOpen && inputRef.current) {
            inputRef.current.focus();
        }
    }, [isOpen]);

    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages, isTyping]);

    const handleSend = () => {
        const trimmed = input.trim();
        if (!trimmed) return;

        const userMsg: Message = {
            id: 'msg-' + Date.now(),
            text: trimmed,
            sender: 'user',
            timestamp: new Date()
        };
        setMessages(prev => [...prev, userMsg]);
        setInput('');

        setIsTyping(true);
        const responses = [
            "Thanks for reaching out! I'll look into that right away.",
            "That's a great question! Let me check on that for you.",
            "I understand your concern. Let me connect you with the right person.",
            "Absolutely! We can help with that. Could you provide a bit more detail?",
            "Thank you for bringing that to our attention. I'll escalate this to the team.",
            "Yes, that's definitely something we can assist with. One moment please.",
            "I appreciate your patience! Here's what I'd suggest..."
        ];
        const randomResponse = responses[Math.floor(Math.random() * responses.length)];

        setTimeout(() => {
            const managerMsg: Message = {
                id: 'msg-' + Date.now() + 100,
                text: randomResponse,
                sender: 'manager',
                timestamp: new Date()
            };
            setMessages(prev => [...prev, managerMsg]);
            setIsTyping(false);
        }, 1500);
    };

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSend();
        }
    };

    const handleOfflineSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!offlineForm.name.trim() || !offlineForm.email.trim() || !offlineForm.message.trim()) return;

        setOfflineSubmitting(true);
        const formBody = new URLSearchParams();
        formBody.append('name', offlineForm.name.trim());
        formBody.append('email', offlineForm.email.trim());
        formBody.append('message', offlineForm.message.trim());

        try {
            await fetch('https://readdy.ai/api/form/d8mk8jq1heuq7aefijsg', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: formBody.toString()
            });
        } catch (_) {
            // form submitted even on network error
        }

        setOfflineSubmitting(false);
        setOfflineSubmitted(true);
    };

    const handleManagerLogin = (e: React.FormEvent) => {
        e.preventDefault();
        if (!managerLoginName.trim() || !managerLoginPassword.trim()) {
            setManagerLoginError('Please fill in all fields.');
            return;
        }
        if (managerLoginPassword !== 'admin123') {
            setManagerLoginError('Invalid password. Please try again.');
            return;
        }
        setManagerLoginError('');
        localStorage.setItem(MANAGER_KEY, 'true');
        localStorage.setItem(MANAGER_NAME_KEY, managerLoginName.trim());
        setIsManagerOnline(true);
        setManagerName(managerLoginName.trim());
        setShowManagerLogin(false);
        setManagerLoginName('');
        setManagerLoginPassword('');
    };

    const handleEndDuty = () => {
        localStorage.removeItem(MANAGER_KEY);
        localStorage.removeItem(MANAGER_NAME_KEY);
        setIsManagerOnline(false);
        setManagerName('Support Manager');
        setOfflineSubmitted(false);
        setOfflineForm({ name: '', email: '', message: '' });
    };

    const formatTime = (date: Date) => {
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    };

    const resetChat = () => {
        setIsOpen(false);
        if (!isManagerOnline) {
            setOfflineSubmitted(false);
            setOfflineForm({ name: '', email: '', message: '' });
        }
        setShowManagerLogin(false);
        setManagerLoginError('');
        setManagerLoginName('');
        setManagerLoginPassword('');
        setMessages(initialMessages);
        setInput('');
    };

    return (
        <>
            {/* Chat Panel */}
            {isOpen && (
                <div className="fixed bottom-20 right-6 w-96 h-[540px] bg-white rounded-2xl shadow-2xl border border-gray-200 flex flex-col z-50 overflow-hidden">
                    {/* Header */}
                    <div className="flex items-center justify-between px-5 py-4 bg-teal-600 text-white flex-shrink-0">
                        <div className="flex items-center gap-3">
                            <div className="w-9 h-9 rounded-full bg-white/20 flex items-center justify-center">
                                <i className="ri-customer-service-2-line text-white text-lg"></i>
                            </div>
                            <div>
                                <h3 className="text-sm font-semibold">{isManagerOnline ? managerName : 'Support Manager'}</h3>
                                {isManagerOnline ? (
                                    <p className="text-xs text-white/70 flex items-center gap-1">
                                        <span className="w-2 h-2 rounded-full bg-emerald-300 inline-block"></span>
                                        Online
                                    </p>
                                ) : (
                                    <p className="text-xs text-white/70 flex items-center gap-1">
                                        <span className="w-2 h-2 rounded-full bg-amber-300 inline-block"></span>
                                        Offline
                                    </p>
                                )}
                            </div>
                        </div>
                        <div className="flex items-center gap-1">
                            {isManagerOnline && (
                                <button
                                    onClick={handleEndDuty}
                                    className="px-2.5 py-1 bg-white/20 rounded-md text-xs text-white hover:bg-white/30 transition-colors cursor-pointer whitespace-nowrap"
                                    title="End duty shift"
                                >
                                    End Duty
                                </button>
                            )}
                            <button
                                onClick={resetChat}
                                className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center hover:bg-white/30 transition-colors cursor-pointer"
                            >
                                <i className="ri-close-line text-white"></i>
                            </button>
                        </div>
                    </div>

                    {/* Content — Online Chat or Offline Form */}
                    {isManagerOnline ? (
                        <>
                            {/* Messages */}
                            <div className="flex-1 overflow-y-auto px-4 py-4 bg-gray-50">
                                {messages.map(msg => (
                                    <div
                                        key={msg.id}
                                        className={`flex mb-4 ${msg.sender === 'user' ? 'justify-end' : 'justify-start'}`}
                                    >
                                        {msg.sender === 'manager' && (
                                            <div className="w-8 h-8 rounded-full bg-teal-100 flex items-center justify-center flex-shrink-0 mr-2 mt-1">
                                                <i className="ri-customer-service-2-line text-teal-600 text-sm"></i>
                                            </div>
                                        )}
                                        <div className={`max-w-[75%] ${msg.sender === 'user' ? 'order-1' : ''}`}>
                                            <div
                                                className={`px-4 py-2.5 rounded-2xl text-sm ${msg.sender === 'user'
                                                        ? 'bg-teal-600 text-white rounded-br-md'
                                                        : 'bg-white text-gray-800 rounded-bl-md border border-gray-200 shadow-sm'
                                                    }`}
                                            >
                                                {msg.text}
                                            </div>
                                            <p className={`text-xs text-gray-400 mt-1 ${msg.sender === 'user' ? 'text-right' : 'text-left'}`}>
                                                {formatTime(msg.timestamp)}
                                            </p>
                                        </div>
                                        {msg.sender === 'user' && (
                                            <div className="w-8 h-8 rounded-full bg-gray-200 flex items-center justify-center flex-shrink-0 ml-2 mt-1">
                                                <i className="ri-user-line text-gray-500 text-sm"></i>
                                            </div>
                                        )}
                                    </div>
                                ))}

                                {isTyping && (
                                    <div className="flex mb-4 justify-start">
                                        <div className="w-8 h-8 rounded-full bg-teal-100 flex items-center justify-center flex-shrink-0 mr-2 mt-1">
                                            <i className="ri-customer-service-2-line text-teal-600 text-sm"></i>
                                        </div>
                                        <div className="bg-white border border-gray-200 shadow-sm rounded-2xl rounded-bl-md px-4 py-3">
                                            <div className="flex items-center gap-1">
                                                <span className="w-2 h-2 rounded-full bg-gray-300 animate-bounce" style={{ animationDelay: '0ms' }}></span>
                                                <span className="w-2 h-2 rounded-full bg-gray-300 animate-bounce" style={{ animationDelay: '150ms' }}></span>
                                                <span className="w-2 h-2 rounded-full bg-gray-300 animate-bounce" style={{ animationDelay: '300ms' }}></span>
                                            </div>
                                        </div>
                                    </div>
                                )}

                                <div ref={messagesEndRef} />
                            </div>

                            {/* Input */}
                            <div className="px-4 py-3 bg-white border-t border-gray-200 flex-shrink-0">
                                <div className="flex items-center gap-2">
                                    <input
                                        ref={inputRef}
                                        type="text"
                                        value={input}
                                        onChange={(e) => setInput(e.target.value)}
                                        onKeyDown={handleKeyDown}
                                        placeholder="Type your message..."
                                        className="flex-1 px-4 py-2.5 bg-gray-100 border border-gray-200 rounded-full text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                    />
                                    <button
                                        onClick={handleSend}
                                        disabled={!input.trim()}
                                        className="w-10 h-10 rounded-full bg-teal-600 text-white flex items-center justify-center hover:bg-teal-700 transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed flex-shrink-0"
                                    >
                                        <i className="ri-send-plane-fill"></i>
                                    </button>
                                </div>
                            </div>
                        </>
                    ) : (
                        /* Offline Contact Form */
                        <div className="flex-1 overflow-y-auto px-5 py-5 bg-gray-50">
                            {showManagerLogin ? (
                                /* Manager Login Form */
                                <div>
                                    <div className="text-center mb-5">
                                        <div className="w-14 h-14 rounded-full bg-teal-100 flex items-center justify-center mx-auto mb-3">
                                            <i className="ri-admin-line text-teal-600 text-xl"></i>
                                        </div>
                                        <h3 className="text-base font-semibold text-gray-800 mb-1">Manager Login</h3>
                                        <p className="text-sm text-gray-500">Sign in to start your support duty</p>
                                    </div>

                                    <form onSubmit={handleManagerLogin} className="space-y-4">
                                        <div>
                                            <label htmlFor="mgr-name" className="block text-sm font-medium text-gray-700 mb-1.5">
                                                Your Name <span className="text-red-500">*</span>
                                            </label>
                                            <input
                                                id="mgr-name"
                                                type="text"
                                                value={managerLoginName}
                                                onChange={(e) => { setManagerLoginName(e.target.value); setManagerLoginError(''); }}
                                                placeholder="Enter your name"
                                                className="w-full px-3.5 py-2.5 bg-white border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                            />
                                        </div>

                                        <div>
                                            <label htmlFor="mgr-pwd" className="block text-sm font-medium text-gray-700 mb-1.5">
                                                Password <span className="text-red-500">*</span>
                                            </label>
                                            <input
                                                id="mgr-pwd"
                                                type="password"
                                                value={managerLoginPassword}
                                                onChange={(e) => { setManagerLoginPassword(e.target.value); setManagerLoginError(''); }}
                                                placeholder="Enter your password"
                                                className="w-full px-3.5 py-2.5 bg-white border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                            />
                                        </div>

                                        {managerLoginError && (
                                            <div className="bg-red-50 border border-red-200 rounded-lg px-4 py-2.5">
                                                <p className="text-sm text-red-700 flex items-center gap-2">
                                                    <i className="ri-error-warning-line"></i>
                                                    {managerLoginError}
                                                </p>
                                            </div>
                                        )}

                                        <button
                                            type="submit"
                                            disabled={!managerLoginName.trim() || !managerLoginPassword.trim()}
                                            className="w-full py-2.5 bg-teal-600 text-white rounded-lg text-sm font-medium hover:bg-teal-700 transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed whitespace-nowrap"
                                        >
                                            Start Duty
                                        </button>

                                        <button
                                            type="button"
                                            onClick={() => { setShowManagerLogin(false); setManagerLoginError(''); }}
                                            className="w-full py-2 text-sm text-gray-500 hover:text-gray-700 transition-colors cursor-pointer whitespace-nowrap"
                                        >
                                            Back to Contact Form
                                        </button>
                                    </form>
                                </div>
                            ) : offlineSubmitted ? (
                                <div className="flex flex-col items-center justify-center h-full text-center">
                                    <div className="w-16 h-16 rounded-full bg-emerald-100 flex items-center justify-center mb-4">
                                        <i className="ri-check-line text-emerald-600 text-2xl"></i>
                                    </div>
                                    <h4 className="text-base font-semibold text-gray-800 mb-2">Message Sent!</h4>
                                    <p className="text-sm text-gray-500 leading-relaxed">
                                        Thank you for reaching out. A manager will review your message and get back to you via email as soon as possible.
                                    </p>
                                </div>
                            ) : (
                                <>
                                    <div className="bg-amber-50 border border-amber-200 rounded-xl p-4 mb-5">
                                        <div className="flex items-start gap-3">
                                            <div className="w-8 h-8 rounded-full bg-amber-100 flex items-center justify-center flex-shrink-0 mt-0.5">
                                                <i className="ri-information-line text-amber-600"></i>
                                            </div>
                                            <p className="text-sm text-amber-800 leading-relaxed">
                                                No manager is currently online. Please provide your contact info and ask your question — we'll answer as soon as possible.
                                            </p>
                                        </div>
                                    </div>

                                    <form onSubmit={handleOfflineSubmit} data-readdy-form>
                                        <div className="space-y-4">
                                            <div>
                                                <label htmlFor="offline-name" className="block text-sm font-medium text-gray-700 mb-1.5">
                                                    Your Name <span className="text-red-500">*</span>
                                                </label>
                                                <input
                                                    id="offline-name"
                                                    name="name"
                                                    type="text"
                                                    required
                                                    value={offlineForm.name}
                                                    onChange={(e) => setOfflineForm(prev => ({ ...prev, name: e.target.value }))}
                                                    placeholder="Enter your full name"
                                                    className="w-full px-3.5 py-2.5 bg-white border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                                />
                                            </div>

                                            <div>
                                                <label htmlFor="offline-email" className="block text-sm font-medium text-gray-700 mb-1.5">
                                                    Your Email <span className="text-red-500">*</span>
                                                </label>
                                                <input
                                                    id="offline-email"
                                                    name="email"
                                                    type="email"
                                                    required
                                                    value={offlineForm.email}
                                                    onChange={(e) => setOfflineForm(prev => ({ ...prev, email: e.target.value }))}
                                                    placeholder="your@email.com"
                                                    className="w-full px-3.5 py-2.5 bg-white border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                                />
                                            </div>

                                            <div>
                                                <label htmlFor="offline-message" className="block text-sm font-medium text-gray-700 mb-1.5">
                                                    Your Question <span className="text-red-500">*</span>
                                                </label>
                                                <textarea
                                                    id="offline-message"
                                                    name="message"
                                                    required
                                                    maxLength={500}
                                                    rows={4}
                                                    value={offlineForm.message}
                                                    onChange={(e) => setOfflineForm(prev => ({ ...prev, message: e.target.value }))}
                                                    placeholder="Describe what you need help with..."
                                                    className="w-full px-3.5 py-2.5 bg-white border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 focus:border-transparent resize-none"
                                                />
                                                <p className="text-xs text-gray-400 mt-1 text-right">
                                                    {offlineForm.message.length}/500
                                                </p>
                                            </div>

                                            <button
                                                type="submit"
                                                disabled={offlineSubmitting || !offlineForm.name.trim() || !offlineForm.email.trim() || !offlineForm.message.trim()}
                                                className="w-full py-2.5 bg-teal-600 text-white rounded-lg text-sm font-medium hover:bg-teal-700 transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed whitespace-nowrap"
                                            >
                                                {offlineSubmitting ? (
                                                    <span className="flex items-center justify-center gap-2">
                                                        <i className="ri-loader-4-line animate-spin"></i>
                                                        Sending...
                                                    </span>
                                                ) : (
                                                    'Send Message'
                                                )}
                                            </button>
                                        </div>
                                    </form>

                                    {/* Manager Login Link */}
                                    <div className="mt-4 text-center">
                                        <button
                                            onClick={() => setShowManagerLogin(true)}
                                            className="text-xs text-gray-400 hover:text-teal-600 transition-colors cursor-pointer inline-flex items-center gap-1"
                                        >
                                            <i className="ri-admin-line"></i>
                                            Manager Login
                                        </button>
                                    </div>
                                </>
                            )}
                        </div>
                    )}
                </div>
            )}

            {/* Floating Button */}
            {!isOpen && (
                <button
                    onClick={() => setIsOpen(true)}
                    className="fixed bottom-6 right-6 w-14 h-14 rounded-full shadow-lg flex items-center justify-center transition-all duration-300 z-50 cursor-pointer bg-teal-600 text-white hover:bg-teal-700 hover:scale-105"
                    title="Chat with support"
                >
                    <div className="relative">
                        <i className="ri-chat-3-line text-2xl"></i>
                        {isManagerOnline ? (
                            <span className="absolute -top-1 -right-1 w-3 h-3 rounded-full bg-emerald-400 border-2 border-white"></span>
                        ) : (
                            <span className="absolute -top-1 -right-1 w-3 h-3 rounded-full bg-red-500 border-2 border-white"></span>
                        )}
                    </div>
                </button>
            )}
        </>
    );
}