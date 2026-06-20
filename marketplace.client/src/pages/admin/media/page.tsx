import { useState } from 'react';
import { mockNews, mockEvents } from '@/mocks/media';
import { mockPolls } from '@/mocks/polls';
import Pagination from '@/components/base/Pagination';

interface NewsItem {
    id: string;
    title: string;
    description: string;
    image: string;
    publishedAt: string;
    author: string;
    category: string;
    tags: string[];
}

interface EventItem {
    id: string;
    title: string;
    description: string;
    startDate: string;
    endDate: string;
    location: string;
    approximateVisitors: number;
    image: string;
    status: string;
    ticketPrice: number;
    organizer: string;
}

interface PollOption {
    id: string;
    text: string;
    votes: number;
}

interface PollItem {
    id: string;
    question: string;
    options: PollOption[];
    totalVotes: number;
    status: string;
    createdAt: string;
}

const ITEMS_PER_PAGE = 5;

export default function MediaAdmin() {
    const [activeTab, setActiveTab] = useState<'news' | 'events' | 'polls'>('news');
    const [news, setNews] = useState<NewsItem[]>(mockNews);
    const [events, setEvents] = useState<EventItem[]>(mockEvents);
    const [polls, setPolls] = useState<PollItem[]>(mockPolls);
    const [showNewsModal, setShowNewsModal] = useState(false);
    const [showEventModal, setShowEventModal] = useState(false);
    const [showPollModal, setShowPollModal] = useState(false);
    const [editingNewsId, setEditingNewsId] = useState<string | null>(null);
    const [editingEventId, setEditingEventId] = useState<string | null>(null);
    const [editingPollId, setEditingPollId] = useState<string | null>(null);
    const [deleteNewsId, setDeleteNewsId] = useState<string | null>(null);
    const [deleteEventId, setDeleteEventId] = useState<string | null>(null);
    const [deletePollId, setDeletePollId] = useState<string | null>(null);

    const [newsForm, setNewsForm] = useState({ title: '', description: '', author: '', category: 'Education', tags: '' });
    const [eventForm, setEventForm] = useState({
        title: '', description: '', startDate: '', endDate: '', location: '',
        approximateVisitors: '', ticketPrice: '', organizer: '', status: 'Upcoming'
    });
    const [pollForm, setPollForm] = useState({ question: '', optionTexts: ['', ''] });

    const [newsPage, setNewsPage] = useState(1);
    const [eventsPage, setEventsPage] = useState(1);
    const [pollsPage, setPollsPage] = useState(1);

    const [newsSearch, setNewsSearch] = useState('');
    const [activeNewsSearch, setActiveNewsSearch] = useState('');
    const [eventsSearch, setEventsSearch] = useState('');
    const [activeEventsSearch, setActiveEventsSearch] = useState('');
    const [pollsSearch, setPollsSearch] = useState('');
    const [activePollsSearch, setActivePollsSearch] = useState('');

    const handleNewsSearch = () => setActiveNewsSearch(newsSearch.trim());
    const handleEventsSearch = () => setActiveEventsSearch(eventsSearch.trim());
    const handlePollsSearch = () => setActivePollsSearch(pollsSearch.trim());

    const filteredNews = news.filter(item => {
        if (!activeNewsSearch) return true;
        const q = activeNewsSearch.toLowerCase();
        return item.title.toLowerCase().includes(q) || item.description.toLowerCase().includes(q) || item.author.toLowerCase().includes(q) || item.tags.some(t => t.toLowerCase().includes(q));
    });

    const filteredEvents = events.filter(item => {
        if (!activeEventsSearch) return true;
        const q = activeEventsSearch.toLowerCase();
        return item.title.toLowerCase().includes(q) || item.description.toLowerCase().includes(q) || item.location.toLowerCase().includes(q) || item.organizer.toLowerCase().includes(q);
    });

    const filteredPolls = polls.filter(item => {
        if (!activePollsSearch) return true;
        const q = activePollsSearch.toLowerCase();
        return item.question.toLowerCase().includes(q);
    });

    const resetNewsForm = () => setNewsForm({ title: '', description: '', author: '', category: 'Education', tags: '' });
    const resetEventForm = () => setEventForm({
        title: '', description: '', startDate: '', endDate: '', location: '',
        approximateVisitors: '', ticketPrice: '', organizer: '', status: 'Upcoming'
    });
    const resetPollForm = () => setPollForm({ question: '', optionTexts: ['', ''] });

    const handleCreateNews = (e: React.FormEvent) => {
        e.preventDefault();
        const newNews: NewsItem = {
            id: 'news-' + Date.now(),
            title: newsForm.title,
            description: newsForm.description,
            author: newsForm.author,
            category: newsForm.category,
            tags: newsForm.tags.split(',').map(t => t.trim()).filter(Boolean),
            image: 'https://readdy.ai/api/search-image?query=charity%20non%20profit%20news%20article%20illustration%20clean%20modern%20design%20with%20teal%20accents%20abstract%20background&width=800&height=500&seq=news-gen-default&orientation=landscape',
            publishedAt: new Date().toISOString()
        };
        setNews([newNews, ...news]);
        setShowNewsModal(false);
        resetNewsForm();
    };

    const handleUpdateNews = (e: React.FormEvent) => {
        e.preventDefault();
        if (!editingNewsId) return;
        setNews(news.map(n => n.id === editingNewsId ? {
            ...n,
            title: newsForm.title,
            description: newsForm.description,
            author: newsForm.author,
            category: newsForm.category,
            tags: newsForm.tags.split(',').map(t => t.trim()).filter(Boolean)
        } : n));
        setShowNewsModal(false);
        setEditingNewsId(null);
        resetNewsForm();
    };

    const handleEditNews = (item: NewsItem) => {
        setEditingNewsId(item.id);
        setNewsForm({
            title: item.title,
            description: item.description,
            author: item.author,
            category: item.category,
            tags: item.tags.join(', ')
        });
        setShowNewsModal(true);
    };

    const handleDeleteNews = (id: string) => {
        setNews(news.filter(n => n.id !== id));
        setDeleteNewsId(null);
    };

    const handleCreateEvent = (e: React.FormEvent) => {
        e.preventDefault();
        const newEvent: EventItem = {
            id: 'event-' + Date.now(),
            title: eventForm.title,
            description: eventForm.description,
            startDate: eventForm.startDate,
            endDate: eventForm.endDate || eventForm.startDate,
            location: eventForm.location,
            approximateVisitors: parseInt(eventForm.approximateVisitors) || 0,
            ticketPrice: parseFloat(eventForm.ticketPrice) || 0,
            organizer: eventForm.organizer,
            status: eventForm.status,
            image: 'https://readdy.ai/api/search-image?query=charity%20fundraising%20event%20illustration%20clean%20modern%20design%20with%20teal%20accents%20community%20gathering&width=800&height=500&seq=event-gen-default&orientation=landscape'
        };
        setEvents([newEvent, ...events]);
        setShowEventModal(false);
        resetEventForm();
    };

    const handleUpdateEvent = (e: React.FormEvent) => {
        e.preventDefault();
        if (!editingEventId) return;
        setEvents(events.map(ev => ev.id === editingEventId ? {
            ...ev,
            title: eventForm.title,
            description: eventForm.description,
            startDate: eventForm.startDate,
            endDate: eventForm.endDate || eventForm.startDate,
            location: eventForm.location,
            approximateVisitors: parseInt(eventForm.approximateVisitors) || 0,
            ticketPrice: parseFloat(eventForm.ticketPrice) || 0,
            organizer: eventForm.organizer,
            status: eventForm.status
        } : ev));
        setShowEventModal(false);
        setEditingEventId(null);
        resetEventForm();
    };

    const handleEditEvent = (item: EventItem) => {
        setEditingEventId(item.id);
        setEventForm({
            title: item.title,
            description: item.description,
            startDate: item.startDate.split('T')[0],
            endDate: item.endDate.split('T')[0],
            location: item.location,
            approximateVisitors: item.approximateVisitors.toString(),
            ticketPrice: item.ticketPrice.toString(),
            organizer: item.organizer,
            status: item.status
        });
        setShowEventModal(true);
    };

    const handleDeleteEvent = (id: string) => {
        setEvents(events.filter(ev => ev.id !== id));
        setDeleteEventId(null);
    };

    const handleCreatePoll = (e: React.FormEvent) => {
        e.preventDefault();
        const validOptions = pollForm.optionTexts.map(t => t.trim()).filter(Boolean);
        if (validOptions.length < 2) return;
        const newPoll: PollItem = {
            id: 'poll-' + Date.now(),
            question: pollForm.question.trim(),
            options: validOptions.map((text, i) => ({ id: `opt-new-${Date.now()}-${i}`, text, votes: 0 })),
            totalVotes: 0,
            status: 'Active',
            createdAt: new Date().toISOString(),
        };
        setPolls([newPoll, ...polls]);
        setShowPollModal(false);
        resetPollForm();
    };

    const handleUpdatePoll = (e: React.FormEvent) => {
        e.preventDefault();
        if (!editingPollId) return;
        const validOptions = pollForm.optionTexts.map(t => t.trim()).filter(Boolean);
        if (validOptions.length < 2) return;
        setPolls(polls.map(p => p.id === editingPollId ? {
            ...p,
            question: pollForm.question.trim(),
            options: validOptions.map((text, i) => {
                const existing = p.options[i];
                return existing ? { ...existing, text } : { id: `opt-new-${Date.now()}-${i}`, text, votes: 0 };
            }),
        } : p));
        setShowPollModal(false);
        setEditingPollId(null);
        resetPollForm();
    };

    const handleEditPoll = (item: PollItem) => {
        setEditingPollId(item.id);
        setPollForm({
            question: item.question,
            optionTexts: item.options.map(o => o.text),
        });
        setShowPollModal(true);
    };

    const handleDeletePoll = (id: string) => {
        setPolls(polls.filter(p => p.id !== id));
        setDeletePollId(null);
    };

    const togglePollStatus = (id: string) => {
        setPolls(polls.map(p => p.id === id ? {
            ...p,
            status: p.status === 'Active' ? 'Closed' : 'Active'
        } : p));
    };

    const addOptionField = () => {
        if (pollForm.optionTexts.length < 10) {
            setPollForm({ ...pollForm, optionTexts: [...pollForm.optionTexts, ''] });
        }
    };

    const removeOptionField = (index: number) => {
        if (pollForm.optionTexts.length <= 2) return;
        setPollForm({
            ...pollForm,
            optionTexts: pollForm.optionTexts.filter((_, i) => i !== index)
        });
    };

    const updateOptionText = (index: number, value: string) => {
        const newOptions = [...pollForm.optionTexts];
        newOptions[index] = value;
        setPollForm({ ...pollForm, optionTexts: newOptions });
    };

    const totalNewsPages = Math.ceil(filteredNews.length / ITEMS_PER_PAGE);
    const totalEventsPages = Math.ceil(filteredEvents.length / ITEMS_PER_PAGE);
    const totalPollsPages = Math.ceil(filteredPolls.length / ITEMS_PER_PAGE);
    const pagedNews = filteredNews.slice((newsPage - 1) * ITEMS_PER_PAGE, newsPage * ITEMS_PER_PAGE);
    const pagedEvents = filteredEvents.slice((eventsPage - 1) * ITEMS_PER_PAGE, eventsPage * ITEMS_PER_PAGE);
    const pagedPolls = filteredPolls.slice((pollsPage - 1) * ITEMS_PER_PAGE, pollsPage * ITEMS_PER_PAGE);

    return (
        <div className="p-8">
            <div className="max-w-5xl mx-auto space-y-6">
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">Media Management</h1>
                        <p className="text-sm text-gray-600 mt-1">Create and manage news articles, events and polls</p>
                    </div>
                </div>

                <div className="flex items-center gap-2 bg-gray-100 p-1 rounded-full w-fit">
                    <button
                        onClick={() => setActiveTab('news')}
                        className={`px-5 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${activeTab === 'news' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'
                            }`}
                    >
                        <i className="ri-newspaper-line mr-2"></i>News
                        <span className="ml-1.5 px-1.5 py-0.5 bg-gray-200 text-gray-700 text-xs rounded-full">{news.length}</span>
                    </button>
                    <button
                        onClick={() => setActiveTab('events')}
                        className={`px-5 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${activeTab === 'events' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'
                            }`}
                    >
                        <i className="ri-calendar-event-line mr-2"></i>Events
                        <span className="ml-1.5 px-1.5 py-0.5 bg-gray-200 text-gray-700 text-xs rounded-full">{events.length}</span>
                    </button>
                    <button
                        onClick={() => setActiveTab('polls')}
                        className={`px-5 py-2 text-sm font-medium rounded-full transition-colors cursor-pointer whitespace-nowrap ${activeTab === 'polls' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-600 hover:text-gray-900'
                            }`}
                    >
                        <i className="ri-bar-chart-horizontal-line mr-2"></i>Polls
                        <span className="ml-1.5 px-1.5 py-0.5 bg-gray-200 text-gray-700 text-xs rounded-full">{polls.length}</span>
                    </button>
                </div>

                {/* News Tab */}
                {activeTab === 'news' && (
                    <>
                        <div className="flex items-center justify-between">
                            <p className="text-sm text-gray-500">{news.length} article{news.length !== 1 ? 's' : ''} published{activeNewsSearch ? ` · ${filteredNews.length} match` : ''}</p>
                            <button
                                onClick={() => { setEditingNewsId(null); resetNewsForm(); setShowNewsModal(true); }}
                                className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                            >
                                <i className="ri-add-line"></i>Create News
                            </button>
                        </div>

                        {/* Search Panel */}
                        <div className="flex items-center gap-3 bg-gray-50/50 rounded-lg p-3">
                            <div className="relative flex-1">
                                <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                                <input
                                    type="text"
                                    placeholder="Search news by title, description, author or tags..."
                                    value={newsSearch}
                                    onChange={(e) => setNewsSearch(e.target.value)}
                                    onKeyDown={(e) => e.key === 'Enter' && handleNewsSearch()}
                                    className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                                />
                            </div>
                            <button
                                onClick={handleNewsSearch}
                                className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                            >
                                <i className="ri-search-line mr-1.5"></i>Search
                            </button>
                        </div>

                        {filteredNews.length === 0 ? (
                            <div className="text-center py-16 bg-white rounded-lg border border-gray-200">
                                <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                                    <i className="ri-newspaper-line text-5xl text-gray-300"></i>
                                </div>
                                <p className="text-gray-500 text-sm">{activeNewsSearch ? 'No news match your search' : 'No news articles yet'}</p>
                            </div>
                        ) : (
                            <>
                                <div className="grid gap-4">
                                    {pagedNews.map(item => (
                                        <div key={item.id} className="bg-white rounded-lg border border-gray-200 p-5">
                                            <div className="flex items-start gap-4">
                                                <div className="w-32 h-24 rounded-lg bg-gray-100 overflow-hidden flex-shrink-0 border border-gray-200">
                                                    <img src={item.image} alt={item.title} className="w-full h-full object-cover object-top" />
                                                </div>
                                                <div className="flex-1 min-w-0">
                                                    <div className="flex items-start justify-between mb-2">
                                                        <div>
                                                            <span className="text-xs text-teal-600 bg-teal-50 px-2 py-0.5 rounded-full">{item.category}</span>
                                                            <h3 className="text-base font-semibold text-gray-900 mt-1">{item.title}</h3>
                                                        </div>
                                                        <div className="flex items-center gap-1">
                                                            <button onClick={() => handleEditNews(item)} className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-gray-100 transition-colors cursor-pointer text-gray-500">
                                                                <i className="ri-edit-line"></i>
                                                            </button>
                                                            <button onClick={() => setDeleteNewsId(item.id)} className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-red-50 transition-colors cursor-pointer text-gray-500 hover:text-red-500">
                                                                <i className="ri-delete-bin-line"></i>
                                                            </button>
                                                        </div>
                                                    </div>
                                                    <p className="text-sm text-gray-600 line-clamp-2 mb-2">{item.description}</p>
                                                    <div className="flex items-center gap-3 text-xs text-gray-400">
                                                        <span className="flex items-center gap-1"><i className="ri-user-line"></i>{item.author}</span>
                                                        <span className="flex items-center gap-1"><i className="ri-calendar-line"></i>{new Date(item.publishedAt).toLocaleDateString()}</span>
                                                        <div className="flex items-center gap-1">
                                                            {item.tags.map((tag, i) => (
                                                                <span key={i} className="px-1.5 py-0.5 bg-gray-100 rounded text-xs text-gray-500">{tag}</span>
                                                            ))}
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                                <Pagination currentPage={newsPage} totalPages={totalNewsPages} onPageChange={setNewsPage} />
                            </>
                        )}
                    </>
                )}

                {/* Events Tab */}
                {activeTab === 'events' && (
                    <>
                        <div className="flex items-center justify-between">
                            <p className="text-sm text-gray-500">{events.length} event{events.length !== 1 ? 's' : ''} created{activeEventsSearch ? ` · ${filteredEvents.length} match` : ''}</p>
                            <button
                                onClick={() => { setEditingEventId(null); resetEventForm(); setShowEventModal(true); }}
                                className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                            >
                                <i className="ri-add-line"></i>Create Event
                            </button>
                        </div>

                        {/* Search Panel */}
                        <div className="flex items-center gap-3 bg-gray-50/50 rounded-lg p-3">
                            <div className="relative flex-1">
                                <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                                <input
                                    type="text"
                                    placeholder="Search events by title, description, location or organizer..."
                                    value={eventsSearch}
                                    onChange={(e) => setEventsSearch(e.target.value)}
                                    onKeyDown={(e) => e.key === 'Enter' && handleEventsSearch()}
                                    className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                                />
                            </div>
                            <button
                                onClick={handleEventsSearch}
                                className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                            >
                                <i className="ri-search-line mr-1.5"></i>Search
                            </button>
                        </div>

                        {filteredEvents.length === 0 ? (
                            <div className="text-center py-16 bg-white rounded-lg border border-gray-200">
                                <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                                    <i className="ri-calendar-event-line text-5xl text-gray-300"></i>
                                </div>
                                <p className="text-gray-500 text-sm">{activeEventsSearch ? 'No events match your search' : 'No events created yet'}</p>
                            </div>
                        ) : (
                            <>
                                <div className="grid gap-4">
                                    {pagedEvents.map(item => (
                                        <div key={item.id} className="bg-white rounded-lg border border-gray-200 p-5">
                                            <div className="flex items-start gap-4">
                                                <div className="w-32 h-24 rounded-lg bg-gray-100 overflow-hidden flex-shrink-0 border border-gray-200">
                                                    <img src={item.image} alt={item.title} className="w-full h-full object-cover object-top" />
                                                </div>
                                                <div className="flex-1 min-w-0">
                                                    <div className="flex items-start justify-between mb-2">
                                                        <div>
                                                            <span className={`text-xs px-2 py-0.5 rounded-full ${item.status === 'Upcoming' ? 'bg-teal-100 text-teal-700' : 'bg-emerald-100 text-emerald-700'}`}>
                                                                {item.status}
                                                            </span>
                                                            <h3 className="text-base font-semibold text-gray-900 mt-1">{item.title}</h3>
                                                        </div>
                                                        <div className="flex items-center gap-1">
                                                            <button onClick={() => handleEditEvent(item)} className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-gray-100 transition-colors cursor-pointer text-gray-500">
                                                                <i className="ri-edit-line"></i>
                                                            </button>
                                                            <button onClick={() => setDeleteEventId(item.id)} className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-red-50 transition-colors cursor-pointer text-gray-500 hover:text-red-500">
                                                                <i className="ri-delete-bin-line"></i>
                                                            </button>
                                                        </div>
                                                    </div>
                                                    <p className="text-sm text-gray-600 line-clamp-2 mb-2">{item.description}</p>
                                                    <div className="grid grid-cols-4 gap-3 text-xs text-gray-500">
                                                        <span className="flex items-center gap-1"><i className="ri-map-pin-line"></i>{item.location}</span>
                                                        <span className="flex items-center gap-1"><i className="ri-calendar-line"></i>{new Date(item.startDate).toLocaleDateString()} - {new Date(item.endDate).toLocaleDateString()}</span>
                                                        <span className="flex items-center gap-1"><i className="ri-team-line"></i>~{item.approximateVisitors.toLocaleString()} visitors</span>
                                                        <span className="flex items-center gap-1"><i className="ri-ticket-line"></i>{item.ticketPrice === 0 ? 'Free' : '$' + item.ticketPrice}</span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                                <Pagination currentPage={eventsPage} totalPages={totalEventsPages} onPageChange={setEventsPage} />
                            </>
                        )}
                    </>
                )}

                {/* Polls Tab */}
                {activeTab === 'polls' && (
                    <>
                        <div className="flex items-center justify-between">
                            <p className="text-sm text-gray-500">{polls.length} poll{polls.length !== 1 ? 's' : ''} created · {polls.filter(p => p.status === 'Active').length} active{activePollsSearch ? ` · ${filteredPolls.length} match` : ''}</p>
                            <button
                                onClick={() => { setEditingPollId(null); resetPollForm(); setShowPollModal(true); }}
                                className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                            >
                                <i className="ri-add-line"></i>Create Poll
                            </button>
                        </div>

                        {/* Search Panel */}
                        <div className="flex items-center gap-3 bg-gray-50/50 rounded-lg p-3">
                            <div className="relative flex-1">
                                <i className="ri-search-line absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
                                <input
                                    type="text"
                                    placeholder="Search polls by question..."
                                    value={pollsSearch}
                                    onChange={(e) => setPollsSearch(e.target.value)}
                                    onKeyDown={(e) => e.key === 'Enter' && handlePollsSearch()}
                                    className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-teal-500"
                                />
                            </div>
                            <button
                                onClick={handlePollsSearch}
                                className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-md hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap"
                            >
                                <i className="ri-search-line mr-1.5"></i>Search
                            </button>
                        </div>

                        {filteredPolls.length === 0 ? (
                            <div className="text-center py-16 bg-white rounded-lg border border-gray-200">
                                <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                                    <i className="ri-bar-chart-horizontal-line text-5xl text-gray-300"></i>
                                </div>
                                <p className="text-gray-500 text-sm">{activePollsSearch ? 'No polls match your search' : 'No polls created yet'}</p>
                            </div>
                        ) : (
                            <>
                                <div className="grid gap-4">
                                    {pagedPolls.map(poll => {
                                        const maxVotes = Math.max(...poll.options.map(o => o.votes), 1);
                                        return (
                                            <div key={poll.id} className="bg-white rounded-lg border border-gray-200 p-5">
                                                <div className="flex items-start justify-between mb-4">
                                                    <div className="flex-1 min-w-0">
                                                        <div className="flex items-center gap-2 mb-1">
                                                            <span className={`px-2 py-0.5 text-xs font-medium rounded-full whitespace-nowrap ${poll.status === 'Active' ? 'bg-emerald-100 text-emerald-700' : 'bg-gray-100 text-gray-600'
                                                                }`}>
                                                                {poll.status}
                                                            </span>
                                                            <span className="text-xs text-gray-400">{new Date(poll.createdAt).toLocaleDateString()}</span>
                                                        </div>
                                                        <h3 className="text-base font-semibold text-gray-900">{poll.question}</h3>
                                                        <p className="text-xs text-gray-500 mt-1">{poll.totalVotes} total vote{poll.totalVotes !== 1 ? 's' : ''}</p>
                                                    </div>
                                                    <div className="flex items-center gap-1 ml-4">
                                                        <button
                                                            onClick={() => togglePollStatus(poll.id)}
                                                            className={`w-8 h-8 flex items-center justify-center rounded-lg transition-colors cursor-pointer ${poll.status === 'Active' ? 'hover:bg-gray-100 text-gray-500' : 'hover:bg-emerald-50 text-gray-400 hover:text-emerald-600'
                                                                }`}
                                                            title={poll.status === 'Active' ? 'Close Poll' : 'Reopen Poll'}
                                                        >
                                                            <i className={poll.status === 'Active' ? 'ri-stop-circle-line' : 'ri-play-circle-line'}></i>
                                                        </button>
                                                        <button onClick={() => handleEditPoll(poll)} className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-gray-100 transition-colors cursor-pointer text-gray-500">
                                                            <i className="ri-edit-line"></i>
                                                        </button>
                                                        <button onClick={() => setDeletePollId(poll.id)} className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-red-50 transition-colors cursor-pointer text-gray-500 hover:text-red-500">
                                                            <i className="ri-delete-bin-line"></i>
                                                        </button>
                                                    </div>
                                                </div>

                                                <div className="space-y-2.5">
                                                    {poll.options.map(option => {
                                                        const pct = poll.totalVotes > 0 ? Math.round((option.votes / poll.totalVotes) * 100) : 0;
                                                        const barWidth = maxVotes > 0 ? Math.round((option.votes / maxVotes) * 100) : 0;
                                                        return (
                                                            <div key={option.id}>
                                                                <div className="flex items-center justify-between mb-1">
                                                                    <span className="text-sm text-gray-700">{option.text}</span>
                                                                    <span className="text-xs text-gray-500">{option.votes} votes ({pct}%)</span>
                                                                </div>
                                                                <div className="w-full h-2 bg-gray-100 rounded-full overflow-hidden">
                                                                    <div
                                                                        className="h-full rounded-full bg-teal-500 transition-all duration-300"
                                                                        style={{ width: `${barWidth}%` }}
                                                                    ></div>
                                                                </div>
                                                            </div>
                                                        );
                                                    })}
                                                </div>
                                            </div>
                                        );
                                    })}
                                </div>
                                <Pagination currentPage={pollsPage} totalPages={totalPollsPages} onPageChange={setPollsPage} />
                            </>
                        )}
                    </>
                )}
            </div>

            {/* News Form Modal */}
            {showNewsModal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-lg w-full p-6 max-h-[90vh] overflow-y-auto">
                        <h3 className="text-lg font-semibold text-gray-900 mb-4">{editingNewsId ? 'Edit News' : 'Create News'}</h3>
                        <form onSubmit={editingNewsId ? handleUpdateNews : handleCreateNews} className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Title</label>
                                <input type="text" value={newsForm.title} onChange={e => setNewsForm({ ...newsForm, title: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                                <textarea value={newsForm.description} onChange={e => setNewsForm({ ...newsForm, description: e.target.value })} rows={4} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Author</label>
                                    <input type="text" value={newsForm.author} onChange={e => setNewsForm({ ...newsForm, author: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Category</label>
                                    <select value={newsForm.category} onChange={e => setNewsForm({ ...newsForm, category: e.target.value })} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer">
                                        <option>Education</option>
                                        <option>Health</option>
                                        <option>Wildlife</option>
                                        <option>Technology</option>
                                        <option>Environment</option>
                                        <option>Community</option>
                                        <option>Culture</option>
                                    </select>
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Tags (comma separated)</label>
                                <input type="text" value={newsForm.tags} onChange={e => setNewsForm({ ...newsForm, tags: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" placeholder="e.g. Kenya, Education, Construction" />
                            </div>
                            <div className="flex items-center gap-3 pt-2">
                                <button type="submit" className="flex-1 px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap">
                                    {editingNewsId ? 'Save Changes' : 'Create News'}
                                </button>
                                <button type="button" onClick={() => { setShowNewsModal(false); setEditingNewsId(null); resetNewsForm(); }} className="px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">
                                    Cancel
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Event Form Modal */}
            {showEventModal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-lg w-full p-6 max-h-[90vh] overflow-y-auto">
                        <h3 className="text-lg font-semibold text-gray-900 mb-4">{editingEventId ? 'Edit Event' : 'Create Event'}</h3>
                        <form onSubmit={editingEventId ? handleUpdateEvent : handleCreateEvent} className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Title</label>
                                <input type="text" value={eventForm.title} onChange={e => setEventForm({ ...eventForm, title: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                                <textarea value={eventForm.description} onChange={e => setEventForm({ ...eventForm, description: e.target.value })} rows={3} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
                                    <input type="date" value={eventForm.startDate} onChange={e => setEventForm({ ...eventForm, startDate: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">End Date</label>
                                    <input type="date" value={eventForm.endDate} onChange={e => setEventForm({ ...eventForm, endDate: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" />
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Location</label>
                                <input type="text" value={eventForm.location} onChange={e => setEventForm({ ...eventForm, location: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Approx. Visitors</label>
                                    <input type="number" value={eventForm.approximateVisitors} onChange={e => setEventForm({ ...eventForm, approximateVisitors: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" min="0" />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Ticket Price ($)</label>
                                    <input type="number" value={eventForm.ticketPrice} onChange={e => setEventForm({ ...eventForm, ticketPrice: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" min="0" step="0.01" />
                                </div>
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Organizer</label>
                                    <input type="text" value={eventForm.organizer} onChange={e => setEventForm({ ...eventForm, organizer: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" required />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
                                    <select value={eventForm.status} onChange={e => setEventForm({ ...eventForm, status: e.target.value })} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent cursor-pointer">
                                        <option>Upcoming</option>
                                        <option>Completed</option>
                                    </select>
                                </div>
                            </div>
                            <div className="flex items-center gap-3 pt-2">
                                <button type="submit" className="flex-1 px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap">
                                    {editingEventId ? 'Save Changes' : 'Create Event'}
                                </button>
                                <button type="button" onClick={() => { setShowEventModal(false); setEditingEventId(null); resetEventForm(); }} className="px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">
                                    Cancel
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Poll Form Modal */}
            {showPollModal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-lg w-full p-6 max-h-[90vh] overflow-y-auto">
                        <h3 className="text-lg font-semibold text-gray-900 mb-4">{editingPollId ? 'Edit Poll' : 'Create Poll'}</h3>
                        <form onSubmit={editingPollId ? handleUpdatePoll : handleCreatePoll} className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Question</label>
                                <input type="text" value={pollForm.question} onChange={e => setPollForm({ ...pollForm, question: e.target.value })} className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent" placeholder="What would you like to ask?" required />
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">Options</label>
                                <div className="space-y-2">
                                    {pollForm.optionTexts.map((text, idx) => (
                                        <div key={idx} className="flex items-center gap-2">
                                            <span className="text-xs text-gray-400 w-5 flex-shrink-0">{idx + 1}.</span>
                                            <input
                                                type="text"
                                                value={text}
                                                onChange={e => updateOptionText(idx, e.target.value)}
                                                className="flex-1 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-teal-500 focus:border-transparent"
                                                placeholder={`Option ${idx + 1}`}
                                                required
                                            />
                                            {pollForm.optionTexts.length > 2 && (
                                                <button type="button" onClick={() => removeOptionField(idx)} className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-red-50 text-gray-400 hover:text-red-500 cursor-pointer flex-shrink-0">
                                                    <i className="ri-close-line"></i>
                                                </button>
                                            )}
                                        </div>
                                    ))}
                                </div>
                                {pollForm.optionTexts.length < 10 && (
                                    <button
                                        type="button"
                                        onClick={addOptionField}
                                        className="mt-2 text-sm text-teal-600 hover:text-teal-700 cursor-pointer flex items-center gap-1"
                                    >
                                        <i className="ri-add-line"></i>Add Option
                                    </button>
                                )}
                            </div>

                            <div className="flex items-center gap-3 pt-2">
                                <button type="submit" className="flex-1 px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap">
                                    {editingPollId ? 'Save Changes' : 'Create Poll'}
                                </button>
                                <button type="button" onClick={() => { setShowPollModal(false); setEditingPollId(null); resetPollForm(); }} className="px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">
                                    Cancel
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Delete News Confirm */}
            {deleteNewsId && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
                        <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center mx-auto mb-4">
                            <i className="ri-alert-line text-red-600 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900 text-center mb-2">Delete News?</h3>
                        <p className="text-sm text-gray-600 text-center mb-6">This cannot be undone.</p>
                        <div className="flex items-center gap-3">
                            <button onClick={() => setDeleteNewsId(null)} className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">Cancel</button>
                            <button onClick={() => handleDeleteNews(deleteNewsId)} className="flex-1 px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors cursor-pointer whitespace-nowrap">Delete</button>
                        </div>
                    </div>
                </div>
            )}

            {/* Delete Event Confirm */}
            {deleteEventId && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
                        <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center mx-auto mb-4">
                            <i className="ri-alert-line text-red-600 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900 text-center mb-2">Delete Event?</h3>
                        <p className="text-sm text-gray-600 text-center mb-6">This cannot be undone.</p>
                        <div className="flex items-center gap-3">
                            <button onClick={() => setDeleteEventId(null)} className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">Cancel</button>
                            <button onClick={() => handleDeleteEvent(deleteEventId)} className="flex-1 px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors cursor-pointer whitespace-nowrap">Delete</button>
                        </div>
                    </div>
                </div>
            )}

            {/* Delete Poll Confirm */}
            {deletePollId && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
                        <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center mx-auto mb-4">
                            <i className="ri-alert-line text-red-600 text-2xl"></i>
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900 text-center mb-2">Delete Poll?</h3>
                        <p className="text-sm text-gray-600 text-center mb-6">This cannot be undone.</p>
                        <div className="flex items-center gap-3">
                            <button onClick={() => setDeletePollId(null)} className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-300 transition-colors cursor-pointer whitespace-nowrap">Cancel</button>
                            <button onClick={() => handleDeletePoll(deletePollId)} className="flex-1 px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors cursor-pointer whitespace-nowrap">Delete</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}