import { Link } from 'react-router-dom';
import { useState } from 'react';
import { mockNews, mockEvents } from '@/mocks/media';
import { mockReports } from '@/mocks/reports';
import { mockFaqs } from '@/mocks/faqs';

export default function Home() {
    const comingSoon = true; // Set to false to enable all features

    const [reportDateFilter, setReportDateFilter] = useState('');
    const [openFaqId, setOpenFaqId] = useState<string | null>(null);
    const [activeFaqCategory, setActiveFaqCategory] = useState<string>('All');

    const filteredReports = reportDateFilter
        ? mockReports.filter(r => r.spentAt.split('T')[0] >= reportDateFilter)
        : mockReports.slice(0, 4);
    const latestNews = mockNews.slice(0, 3);
    const upcomingEvents = mockEvents.filter(e => e.status === 'Upcoming').slice(0, 3);

    const faqCategories = ['All', ...Array.from(new Set(mockFaqs.map(f => f.category)))];
    const filteredFaqs = activeFaqCategory === 'All'
        ? mockFaqs
        : mockFaqs.filter(f => f.category === activeFaqCategory);

    return (
        <div className="min-h-screen bg-white">
            {/* Navigation */}
            <nav className="fixed top-0 left-0 right-0 z-50 transition-all duration-300 bg-white/95 backdrop-blur-sm shadow-sm">
                <div className="max-w-7xl mx-auto px-6 py-4 flex items-center justify-between">
                    <img
                        src="https://public.readdy.ai/ai/img_res/fd4376ec-da1c-49df-8529-c6f422339bdf.png"
                        alt="Logo"
                        className="h-10 w-auto"
                    />
                    <div className="flex items-center gap-8">
                        <a href="#services" className="text-gray-700 hover:text-teal-500 transition-colors whitespace-nowrap cursor-pointer">Services</a>
                        <a href="#about" className="text-gray-700 hover:text-teal-500 transition-colors whitespace-nowrap cursor-pointer">About</a>
                        <Link to="/marketplace" className="px-6 py-2.5 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors whitespace-nowrap cursor-pointer">
                            Visit Marketplace
                        </Link>
                    </div>
                </div>
            </nav>

            {/* Hero Section */}
            <section className="relative pt-32 pb-20 px-6 overflow-hidden">
                <div className="absolute inset-0 bg-gradient-to-br from-teal-50 via-white to-emerald-50"></div>
                <div className="relative max-w-7xl mx-auto text-center">
                    <h1 className="text-5xl md:text-6xl font-bold text-gray-900 mb-6">
                        Your Complete Digital Ecosystem
                    </h1>
                    <p className="text-xl text-gray-600 mb-10 max-w-3xl mx-auto">
                        Access a comprehensive platform offering marketplace solutions, media services, personal account management, IT infrastructure, and compliance tools all in one place
                    </p>
                    <div className="flex items-center justify-center gap-4">
                        <button className="px-8 py-3.5 bg-teal-500 text-white rounded-lg hover:bg-teal-600 transition-colors text-lg font-medium whitespace-nowrap cursor-pointer">
                            Donate Now
                        </button>
                        <button className="px-8 py-3.5 bg-white text-teal-500 border-2 border-teal-500 rounded-lg hover:bg-teal-50 transition-colors text-lg font-medium whitespace-nowrap cursor-pointer">
                            Sign Up
                        </button>
                    </div>
                </div>
            </section>

            {/* Key Advantages */}
            <section className="py-20 px-6 bg-white">
                <div className="max-w-7xl mx-auto">
                    <h2 className="text-4xl font-bold text-center text-gray-900 mb-16">Why Choose Us</h2>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                        <div className="p-8 bg-gradient-to-br from-teal-50 to-emerald-50 rounded-2xl hover:shadow-lg transition-shadow cursor-pointer">
                            <div className="w-14 h-14 bg-teal-500 rounded-xl flex items-center justify-center mb-6">
                                <i className="ri-shield-check-line text-2xl text-white"></i>
                            </div>
                            <h3 className="text-2xl font-bold text-gray-900 mb-4">Secure & Trusted</h3>
                            <p className="text-gray-600 leading-relaxed">
                                Enterprise-grade security with end-to-end encryption ensuring your data and transactions are always protected
                            </p>
                        </div>
                        <div className="p-8 bg-gradient-to-br from-teal-50 to-emerald-50 rounded-2xl hover:shadow-lg transition-shadow cursor-pointer">
                            <div className="w-14 h-14 bg-teal-500 rounded-xl flex items-center justify-center mb-6">
                                <i className="ri-rocket-line text-2xl text-white"></i>
                            </div>
                            <h3 className="text-2xl font-bold text-gray-900 mb-4">Fast & Efficient</h3>
                            <p className="text-gray-600 leading-relaxed">
                                Streamlined processes and optimized performance deliver results quickly without compromising quality
                            </p>
                        </div>
                        <div className="p-8 bg-gradient-to-br from-teal-50 to-emerald-50 rounded-2xl hover:shadow-lg transition-shadow cursor-pointer">
                            <div className="w-14 h-14 bg-teal-500 rounded-xl flex items-center justify-center mb-6">
                                <i className="ri-customer-service-2-line text-2xl text-white"></i>
                            </div>
                            <h3 className="text-2xl font-bold text-gray-900 mb-4">24/7 Support</h3>
                            <p className="text-gray-600 leading-relaxed">
                                Dedicated support team available around the clock to assist you with any questions or concerns
                            </p>
                        </div>
                    </div>
                </div>
            </section>

            {/* Services Cards */}
            <section id="services" className="py-20 px-6 bg-gradient-to-b from-white to-teal-50">
                <div className="max-w-7xl mx-auto">
                    <h2 className="text-4xl font-bold text-center text-gray-900 mb-16">Our Services</h2>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        <Link to="/marketplace" className="group p-8 bg-white rounded-2xl shadow-md hover:shadow-xl transition-all cursor-pointer border-2 border-transparent hover:border-teal-500">
                            <div className="w-16 h-16 bg-teal-500 rounded-xl flex items-center justify-center mb-6 group-hover:scale-110 transition-transform">
                                <i className="ri-store-2-line text-3xl text-white"></i>
                            </div>
                            <h3 className="text-2xl font-bold text-gray-900 mb-3">Marketplace</h3>
                            <p className="text-gray-600 mb-4">Buy and sell products with secure transactions and verified sellers</p>
                            <span className="inline-flex items-center text-teal-500 font-medium whitespace-nowrap">
                                Visit Now <i className="ri-arrow-right-line ml-2"></i>
                            </span>
                        </Link>

                        <div className="relative p-8 bg-white rounded-2xl shadow-md cursor-not-allowed opacity-75">
                            <div className="absolute top-4 right-4 px-3 py-1 bg-amber-100 text-amber-700 text-sm font-medium rounded-full whitespace-nowrap">
                                Coming Soon
                            </div>
                            <div className="w-16 h-16 bg-gray-300 rounded-xl flex items-center justify-center mb-6">
                                <i className="ri-film-line text-3xl text-gray-500"></i>
                            </div>
                            <h3 className="text-2xl font-bold text-gray-900 mb-3">Media</h3>
                            <p className="text-gray-600">Stream and share multimedia content across the platform</p>
                        </div>

                        <div className="relative p-8 bg-white rounded-2xl shadow-md cursor-not-allowed opacity-75">
                            <div className="absolute top-4 right-4 px-3 py-1 bg-amber-100 text-amber-700 text-sm font-medium rounded-full whitespace-nowrap">
                                Coming Soon
                            </div>
                            <div className="w-16 h-16 bg-gray-300 rounded-xl flex items-center justify-center mb-6">
                                <i className="ri-user-settings-line text-3xl text-gray-500"></i>
                            </div>
                            <h3 className="text-2xl font-bold text-gray-900 mb-3">Personal Accounts</h3>
                            <p className="text-gray-600">Manage your profile and preferences in one centralized location</p>
                        </div>

                        <div className="relative p-8 bg-white rounded-2xl shadow-md cursor-not-allowed opacity-75">
                            <div className="absolute top-4 right-4 px-3 py-1 bg-amber-100 text-amber-700 text-sm font-medium rounded-full whitespace-nowrap">
                                Coming Soon
                            </div>
                            <div className="w-16 h-16 bg-gray-300 rounded-xl flex items-center justify-center mb-6">
                                <i className="ri-code-box-line text-3xl text-gray-500"></i>
                            </div>
                            <h3 className="text-2xl font-bold text-gray-900 mb-3">IT Solutions</h3>
                            <p className="text-gray-600">Advanced technology infrastructure and development tools</p>
                        </div>

                        <div className="relative p-8 bg-white rounded-2xl shadow-md cursor-not-allowed opacity-75">
                            <div className="absolute top-4 right-4 px-3 py-1 bg-amber-100 text-amber-700 text-sm font-medium rounded-full whitespace-nowrap">
                                Coming Soon
                            </div>
                            <div className="w-16 h-16 bg-gray-300 rounded-xl flex items-center justify-center mb-6">
                                <i className="ri-file-shield-2-line text-3xl text-gray-500"></i>
                            </div>
                            <h3 className="text-2xl font-bold text-gray-900 mb-3">Compliance</h3>
                            <p className="text-gray-600">Regulatory compliance and legal documentation services</p>
                        </div>
                    </div>
                </div>
            </section>

            {/* Media Section — Reports, Events & News */}
            {!comingSoon && <section className="py-20 px-6 bg-white">
                <div className="max-w-7xl mx-auto">
                    <div className="text-center mb-16">
                        <h2 className="text-4xl font-bold text-gray-900 mb-4">Media & Reports</h2>
                        <p className="text-lg text-gray-600 max-w-2xl mx-auto">
                            Stay informed with our latest reports on charitable spending, upcoming events, and recent news from our community
                        </p>
                    </div>

                    {/* Reports with Datepicker */}
                    <div className="mb-16">
                        <div className="flex items-center justify-between mb-8">
                            <h3 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                                <i className="ri-file-chart-line text-teal-500"></i>
                                Money Spent Reports
                            </h3>
                            <div className="flex items-center gap-2">
                                <label className="text-sm text-gray-600">From:</label>
                                <input
                                    type="date"
                                    value={reportDateFilter}
                                    onChange={(e) => setReportDateFilter(e.target.value)}
                                    className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer"
                                />
                                {reportDateFilter && (
                                    <button
                                        onClick={() => setReportDateFilter('')}
                                        className="text-xs text-teal-600 hover:text-teal-700 cursor-pointer whitespace-nowrap"
                                    >
                                        Clear
                                    </button>
                                )}
                            </div>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
                            {filteredReports.map(report => {
                                const pctSpent = Math.round((report.moneySpent / report.moneyBudget) * 100);
                                return (
                                    <div key={report.id} className="bg-white rounded-xl border border-gray-200 p-5 hover:border-teal-200 transition-colors">
                                        <div className="flex items-center justify-between mb-3">
                                            <span className="text-xs text-gray-400">{report.organizationName}</span>
                                            <span className={`px-2 py-0.5 text-xs font-medium rounded-full ${report.status === 'Completed' ? 'bg-emerald-100 text-emerald-700' : 'bg-amber-100 text-amber-700'}`}>
                                                {report.status}
                                            </span>
                                        </div>
                                        <h4 className="text-sm font-semibold text-gray-900 mb-2 line-clamp-2">{report.goalTitle}</h4>
                                        <div className="flex items-center justify-between mb-3">
                                            <span className="text-xs text-gray-500">Spent</span>
                                            <span className="text-lg font-bold text-emerald-600">${report.moneySpent.toLocaleString()}</span>
                                        </div>
                                        <div className="w-full h-1.5 bg-gray-100 rounded-full overflow-hidden mb-2">
                                            <div className={`h-full rounded-full ${report.status === 'Completed' ? 'bg-emerald-500' : 'bg-teal-500'}`} style={{ width: `${Math.min(pctSpent, 100)}%` }}></div>
                                        </div>
                                        <p className="text-xs text-gray-400">{new Date(report.spentAt).toLocaleDateString()}</p>
                                    </div>
                                );
                            })}
                        </div>
                        <div className="text-center">
                            <Link to="/admin/reports" className="inline-flex items-center gap-2 px-6 py-2.5 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap">
                                <i className="ri-file-chart-line"></i>View All Reports
                            </Link>
                        </div>
                    </div>

                    {/* Events */}
                    <div className="mb-16">
                        <h3 className="text-2xl font-bold text-gray-900 mb-8 flex items-center gap-2">
                            <i className="ri-calendar-event-line text-teal-500"></i>
                            Upcoming Events
                        </h3>
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                            {upcomingEvents.map(event => (
                                <div key={event.id} className="group bg-white rounded-xl border border-gray-200 overflow-hidden hover:border-teal-300 transition-colors cursor-pointer">
                                    <div className="h-48 overflow-hidden">
                                        <img src={event.image} alt={event.title} className="w-full h-full object-cover object-top group-hover:scale-105 transition-transform duration-500" />
                                    </div>
                                    <div className="p-5">
                                        <div className="flex items-center gap-2 mb-2">
                                            {event.ticketPrice === 0 ? (
                                                <span className="px-2 py-0.5 bg-emerald-100 text-emerald-700 text-xs font-medium rounded-full whitespace-nowrap">Free</span>
                                            ) : (
                                                <span className="px-2 py-0.5 bg-amber-100 text-amber-700 text-xs font-medium rounded-full whitespace-nowrap">${event.ticketPrice}</span>
                                            )}
                                            <span className="text-xs text-gray-400">
                                                {new Date(event.startDate).toLocaleDateString()} - {new Date(event.endDate).toLocaleDateString()}
                                            </span>
                                        </div>
                                        <h4 className="text-base font-semibold text-gray-900 mb-2 line-clamp-2">{event.title}</h4>
                                        <p className="text-sm text-gray-600 line-clamp-2 mb-3">{event.description}</p>
                                        <div className="flex items-center gap-3 text-xs text-gray-500">
                                            <span className="flex items-center gap-1"><i className="ri-map-pin-line"></i>{event.location}</span>
                                            <span className="flex items-center gap-1"><i className="ri-team-line"></i>~{event.approximateVisitors.toLocaleString()}</span>
                                        </div>
                                        <p className="text-xs text-gray-400 mt-2">Organized by {event.organizer}</p>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* News */}
                    <div>
                        <h3 className="text-2xl font-bold text-gray-900 mb-8 flex items-center gap-2">
                            <i className="ri-newspaper-line text-teal-500"></i>
                            Latest News
                        </h3>
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                            {latestNews.map(article => (
                                <div key={article.id} className="group bg-white rounded-xl border border-gray-200 overflow-hidden hover:border-teal-300 transition-colors cursor-pointer">
                                    <div className="h-48 overflow-hidden">
                                        <img src={article.image} alt={article.title} className="w-full h-full object-cover object-top group-hover:scale-105 transition-transform duration-500" />
                                    </div>
                                    <div className="p-5">
                                        <div className="flex items-center gap-2 mb-2">
                                            <span className="px-2 py-0.5 bg-teal-100 text-teal-700 text-xs font-medium rounded-full whitespace-nowrap">{article.category}</span>
                                            <span className="text-xs text-gray-400">{new Date(article.publishedAt).toLocaleDateString()}</span>
                                        </div>
                                        <h4 className="text-base font-semibold text-gray-900 mb-2 line-clamp-2">{article.title}</h4>
                                        <p className="text-sm text-gray-600 line-clamp-2 mb-3">{article.description}</p>
                                        <div className="flex items-center justify-between">
                                            <span className="text-xs text-gray-500 flex items-center gap-1">
                                                <i className="ri-user-line"></i>{article.author}
                                            </span>
                                            <div className="flex gap-1">
                                                {article.tags.slice(0, 2).map((tag, i) => (
                                                    <span key={i} className="px-1.5 py-0.5 bg-gray-100 text-xs text-gray-500 rounded">{tag}</span>
                                                ))}
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            </section>
            }

            {/* Service Details */}
            <section id="about" className="py-20 px-6 bg-white">
                <div className="max-w-7xl mx-auto space-y-24">
                    {/* Marketplace Detail */}
                    <div className="grid md:grid-cols-2 gap-12 items-center">
                        <div>
                            <div className="inline-block px-4 py-2 bg-teal-100 text-teal-700 rounded-full text-sm font-medium mb-6 whitespace-nowrap">
                                Active Now
                            </div>
                            <h3 className="text-4xl font-bold text-gray-900 mb-6">Marketplace</h3>
                            <p className="text-lg text-gray-600 mb-8 leading-relaxed">
                                A comprehensive e-commerce platform connecting buyers and sellers worldwide. Features secure payment processing, verified seller profiles, and advanced search capabilities.
                            </p>
                            <div className="space-y-4">
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-teal-500 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">Auction System:</strong>
                                        <span className="text-gray-600"> Bid on exclusive items with real-time updates</span>
                                    </div>
                                </div>
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-teal-500 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">Draw Lots:</strong>
                                        <span className="text-gray-600"> Participate in lucky draws for limited edition products</span>
                                    </div>
                                </div>
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-teal-500 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">Instant Purchase:</strong>
                                        <span className="text-gray-600"> Buy products immediately at fixed prices</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div className="relative h-96 rounded-2xl overflow-hidden shadow-xl">
                            <img
                                src="https://readdy.ai/api/search-image?query=modern%20minimalist%20online%20marketplace%20interface%20showing%20product%20grid%20with%20clean%20white%20background%20and%20mint%20green%20accents%20professional%20ecommerce%20platform%20design%20with%20organized%20product%20cards%20and%20shopping%20features&width=600&height=400&seq=marketplace1&orientation=landscape"
                                alt="Marketplace"
                                className="w-full h-full object-cover object-top"
                            />
                        </div>
                    </div>

                    {/* Media Detail */}
                    <div className="grid md:grid-cols-2 gap-12 items-center">
                        <div className="order-2 md:order-1 relative h-96 rounded-2xl overflow-hidden shadow-xl">
                            <img
                                src="https://readdy.ai/api/search-image?query=sleek%20media%20streaming%20platform%20interface%20with%20video%20thumbnails%20and%20content%20library%20clean%20modern%20design%20with%20mint%20green%20highlights%20professional%20multimedia%20service%20dashboard&width=600&height=400&seq=media1&orientation=landscape"
                                alt="Media"
                                className="w-full h-full object-cover object-top"
                            />
                        </div>
                        <div className="order-1 md:order-2">
                            <div className="inline-block px-4 py-2 bg-amber-100 text-amber-700 rounded-full text-sm font-medium mb-6 whitespace-nowrap">
                                Coming Soon
                            </div>
                            <h3 className="text-4xl font-bold text-gray-900 mb-6">Media Services</h3>
                            <p className="text-lg text-gray-600 mb-8 leading-relaxed">
                                Stream, share, and manage multimedia content with our advanced media platform. Perfect for content creators and consumers alike.
                            </p>
                            <div className="space-y-4">
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-gray-400 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">HD Streaming:</strong>
                                        <span className="text-gray-600"> High-quality video and audio playback</span>
                                    </div>
                                </div>
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-gray-400 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">Content Library:</strong>
                                        <span className="text-gray-600"> Organize and access your media collection</span>
                                    </div>
                                </div>
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-gray-400 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">Social Sharing:</strong>
                                        <span className="text-gray-600"> Share content across multiple platforms</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Personal Accounts Detail */}
                    <div className="grid md:grid-cols-2 gap-12 items-center">
                        <div>
                            <div className="inline-block px-4 py-2 bg-amber-100 text-amber-700 rounded-full text-sm font-medium mb-6 whitespace-nowrap">
                                Coming Soon
                            </div>
                            <h3 className="text-4xl font-bold text-gray-900 mb-6">Personal Accounts</h3>
                            <p className="text-lg text-gray-600 mb-8 leading-relaxed">
                                Centralized account management system providing complete control over your profile, preferences, and activity across all services.
                            </p>
                            <div className="space-y-4">
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-gray-400 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">Unified Dashboard:</strong>
                                        <span className="text-gray-600"> Manage all services from one place</span>
                                    </div>
                                </div>
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-gray-400 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">Privacy Controls:</strong>
                                        <span className="text-gray-600"> Customize your data sharing preferences</span>
                                    </div>
                                </div>
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-gray-400 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">Activity History:</strong>
                                        <span className="text-gray-600"> Track your interactions and transactions</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div className="relative h-96 rounded-2xl overflow-hidden shadow-xl">
                            <img
                                src="https://readdy.ai/api/search-image?query=modern%20user%20account%20dashboard%20interface%20with%20profile%20settings%20and%20personal%20information%20management%20clean%20minimalist%20design%20with%20mint%20accents%20professional%20account%20control%20panel&width=600&height=400&seq=accounts1&orientation=landscape"
                                alt="Personal Accounts"
                                className="w-full h-full object-cover object-top"
                            />
                        </div>
                    </div>

                    {/* IT Solutions Detail */}
                    <div className="grid md:grid-cols-2 gap-12 items-center">
                        <div className="order-2 md:order-1 relative h-96 rounded-2xl overflow-hidden shadow-xl">
                            <img
                                src="https://readdy.ai/api/search-image?query=technology%20infrastructure%20dashboard%20showing%20cloud%20services%20and%20development%20tools%20modern%20IT%20platform%20interface%20with%20mint%20green%20theme%20professional%20technical%20solutions%20display&width=600&height=400&seq=it1&orientation=landscape"
                                alt="IT Solutions"
                                className="w-full h-full object-cover object-top"
                            />
                        </div>
                        <div className="order-1 md:order-2">
                            <div className="inline-block px-4 py-2 bg-amber-100 text-amber-700 rounded-full text-sm font-medium mb-6 whitespace-nowrap">
                                Coming Soon
                            </div>
                            <h3 className="text-4xl font-bold text-gray-900 mb-6">IT Solutions</h3>
                            <p className="text-lg text-gray-600 mb-8 leading-relaxed">
                                Enterprise-grade IT infrastructure and development tools designed to power modern businesses and applications.
                            </p>
                            <div className="space-y-4">
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-gray-400 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">Cloud Infrastructure:</strong>
                                        <span className="text-gray-600"> Scalable hosting and storage solutions</span>
                                    </div>
                                </div>
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-gray-400 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">API Integration:</strong>
                                        <span className="text-gray-600"> Connect with third-party services seamlessly</span>
                                    </div>
                                </div>
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-gray-400 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">Development Tools:</strong>
                                        <span className="text-gray-600"> Complete toolkit for building applications</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Compliance Detail */}
                    <div className="grid md:grid-cols-2 gap-12 items-center">
                        <div>
                            <div className="inline-block px-4 py-2 bg-amber-100 text-amber-700 rounded-full text-sm font-medium mb-6 whitespace-nowrap">
                                Coming Soon
                            </div>
                            <h3 className="text-4xl font-bold text-gray-900 mb-6">Compliance Services</h3>
                            <p className="text-lg text-gray-600 mb-8 leading-relaxed">
                                Comprehensive compliance and legal documentation services ensuring your business meets all regulatory requirements.
                            </p>
                            <div className="space-y-4">
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-gray-400 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">Regulatory Monitoring:</strong>
                                        <span className="text-gray-600"> Stay updated with changing regulations</span>
                                    </div>
                                </div>
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-gray-400 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">Document Management:</strong>
                                        <span className="text-gray-600"> Organize and track compliance documents</span>
                                    </div>
                                </div>
                                <div className="flex items-start gap-3">
                                    <i className="ri-checkbox-circle-fill text-gray-400 text-xl mt-1"></i>
                                    <div>
                                        <strong className="text-gray-900">Audit Support:</strong>
                                        <span className="text-gray-600"> Prepare for and manage compliance audits</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div className="relative h-96 rounded-2xl overflow-hidden shadow-xl">
                            <img
                                src="https://readdy.ai/api/search-image?query=professional%20compliance%20and%20legal%20documentation%20interface%20showing%20regulatory%20management%20system%20clean%20business%20platform%20with%20mint%20green%20accents%20organized%20document%20workflow%20display&width=600&height=400&seq=compliance1&orientation=landscape"
                                alt="Compliance"
                                className="w-full h-full object-cover object-top"
                            />
                        </div>
                    </div>
                </div>
            </section>

            {/* FAQ Section */}
            {!comingSoon && <section id="faq" className="py-20 px-6 bg-gradient-to-b from-white to-teal-50">
                <div className="max-w-4xl mx-auto">
                    <div className="text-center mb-12">
                        <h2 className="text-4xl font-bold text-gray-900 mb-4">Frequently Asked Questions</h2>
                        <p className="text-lg text-gray-600 max-w-2xl mx-auto">
                            Find answers to the most common questions about our platform, marketplace, accounts, and more
                        </p>
                    </div>

                    {/* Category Tabs */}
                    <div className="flex flex-wrap items-center justify-center gap-2 mb-10">
                        {faqCategories.map(cat => (
                            <button
                                key={cat}
                                onClick={() => { setActiveFaqCategory(cat); setOpenFaqId(null); }}
                                className={`px-5 py-2 rounded-full text-sm font-medium transition-all cursor-pointer whitespace-nowrap ${activeFaqCategory === cat
                                        ? 'bg-teal-600 text-white'
                                        : 'bg-white text-gray-600 border border-gray-200 hover:border-teal-300 hover:text-teal-600'
                                    }`}
                            >
                                {cat}
                            </button>
                        ))}
                    </div>

                    {/* FAQ Accordion */}
                    <div className="space-y-3">
                        {filteredFaqs.map(faq => (
                            <div
                                key={faq.id}
                                className="bg-white rounded-xl border border-gray-200 overflow-hidden transition-all duration-300"
                            >
                                <button
                                    onClick={() => setOpenFaqId(openFaqId === faq.id ? null : faq.id)}
                                    className="w-full flex items-center justify-between px-6 py-5 text-left cursor-pointer hover:bg-gray-50 transition-colors"
                                >
                                    <div className="flex items-start gap-4 pr-4">
                                        <span className="flex-shrink-0 w-8 h-8 rounded-full bg-teal-100 flex items-center justify-center mt-0.5">
                                            {openFaqId === faq.id ? (
                                                <i className="ri-subtract-line text-teal-600"></i>
                                            ) : (
                                                <i className="ri-add-line text-teal-600"></i>
                                            )}
                                        </span>
                                        <div>
                                            <span className="inline-block px-2.5 py-0.5 bg-gray-100 text-gray-500 text-xs rounded-full mb-1.5">{faq.category}</span>
                                            <h3 className="text-base font-semibold text-gray-900">{faq.question}</h3>
                                        </div>
                                    </div>
                                    {openFaqId === faq.id ? (
                                        <i className="ri-arrow-up-s-line text-gray-400 flex-shrink-0 text-lg transition-transform duration-300"></i>
                                    ) : (
                                        <i className="ri-arrow-down-s-line text-gray-400 flex-shrink-0 text-lg transition-transform duration-300"></i>
                                    )}
                                </button>
                                <div
                                    className={`overflow-hidden transition-all duration-300 ${openFaqId === faq.id ? 'max-h-96 opacity-100' : 'max-h-0 opacity-0'
                                        }`}
                                >
                                    <div className="px-6 pb-5 pl-[4.5rem]">
                                        <p className="text-gray-600 leading-relaxed text-sm">{faq.answer}</p>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>

                    {filteredFaqs.length === 0 && (
                        <div className="text-center py-16">
                            <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                                <i className="ri-question-line text-gray-400 text-2xl"></i>
                            </div>
                            <p className="text-gray-500">No FAQs found in this category.</p>
                        </div>
                    )}
                </div>
            </section>}

            {/* Footer */}
            <footer className="bg-gradient-to-br from-teal-500 to-emerald-600 text-white py-16 px-6">
                <div className="max-w-7xl mx-auto">
                    <div className="grid grid-cols-1 md:grid-cols-4 gap-12 mb-12">
                        <div>
                            <img
                                src="https://public.readdy.ai/ai/img_res/fd4376ec-da1c-49df-8529-c6f422339bdf.png"
                                alt="Logo"
                                className="h-10 w-auto mb-6 brightness-0 invert"
                            />
                            <p className="text-teal-50 leading-relaxed">
                                Your complete digital ecosystem for marketplace, media, and business solutions.
                            </p>
                        </div>
                        <div>
                            <h4 className="text-lg font-bold mb-4">Services</h4>
                            <ul className="space-y-3">
                                <li><Link to="/marketplace" className="text-teal-50 hover:text-white transition-colors cursor-pointer">Marketplace</Link></li>
                                <li><span className="text-teal-50">Media</span></li>
                                <li><span className="text-teal-50">Personal Accounts</span></li>
                                <li><span className="text-teal-50">IT Solutions</span></li>
                                <li><span className="text-teal-50">Compliance</span></li>
                            </ul>
                        </div>
                        <div>
                            <h4 className="text-lg font-bold mb-4">Company</h4>
                            <ul className="space-y-3">
                                <li><a href="#about" className="text-teal-50 hover:text-white transition-colors cursor-pointer">About Us</a></li>
                                <li><a href="#" className="text-teal-50 hover:text-white transition-colors cursor-pointer">Contact</a></li>
                                <li><a href="#" className="text-teal-50 hover:text-white transition-colors cursor-pointer">Careers</a></li>
                                <li><a href="#" className="text-teal-50 hover:text-white transition-colors cursor-pointer">Blog</a></li>
                            </ul>
                        </div>
                        <div>
                            <h4 className="text-lg font-bold mb-4">Connect</h4>
                            <div className="flex gap-4">
                                <a href="#" className="w-10 h-10 bg-white/20 rounded-lg flex items-center justify-center hover:bg-white/30 transition-colors cursor-pointer">
                                    <i className="ri-facebook-fill text-xl"></i>
                                </a>
                                <a href="#" className="w-10 h-10 bg-white/20 rounded-lg flex items-center justify-center hover:bg-white/30 transition-colors cursor-pointer">
                                    <i className="ri-twitter-fill text-xl"></i>
                                </a>
                                <a href="#" className="w-10 h-10 bg-white/20 rounded-lg flex items-center justify-center hover:bg-white/30 transition-colors cursor-pointer">
                                    <i className="ri-linkedin-fill text-xl"></i>
                                </a>
                                <a href="#" className="w-10 h-10 bg-white/20 rounded-lg flex items-center justify-center hover:bg-white/30 transition-colors cursor-pointer">
                                    <i className="ri-instagram-fill text-xl"></i>
                                </a>
                            </div>
                        </div>
                    </div>
                    <div className="border-t border-white/20 pt-8 flex flex-col md:flex-row justify-between items-center gap-4">
                        <p className="text-teal-50 text-sm">© 2025 All rights reserved.</p>
                        <a href="https://readdy.ai/?ref=logo" target="_blank" rel="noopener noreferrer" className="text-teal-50 hover:text-white text-sm transition-colors cursor-pointer">
                            Powered by Okhy 
                        </a>
                    </div>
                </div>
            </footer>
        </div>
    );
}
