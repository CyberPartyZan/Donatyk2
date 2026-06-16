import { useState } from 'react';
import { mockNews, mockEvents } from '@/mocks/media';

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

export default function MediaAdmin() {
    const [activeTab, setActiveTab] = useState<'news' | 'events'>('news');
    const [news, setNews] = useState<NewsItem[]>(mockNews);
    const [events, setEvents] = useState<EventItem[]>(mockEvents);
    const [showNewsModal, setShowNewsModal] = useState(false);
    const [showEventModal, setShowEventModal] = useState(false);
    const [editingNewsId, setEditingNewsId] = useState<string | null>(null);
    const [editingEventId, setEditingEventId] = useState<string | null>(null);
    const [deleteNewsId, setDeleteNewsId] = useState<string | null>(null);
    const [deleteEventId, setDeleteEventId] = useState<string | null>(null);

    const [newsForm, setNewsForm] = useState({ title: '', description: '', author: '', category: 'Education', tags: '' });
    const [eventForm, setEventForm] = useState({
        title: '', description: '', startDate: '', endDate: '', location: '',
        approximateVisitors: '', ticketPrice: '', organizer: '', status: 'Upcoming'
    });

    const resetNewsForm = () => setNewsForm({ title: '', description: '', author: '', category: 'Education', tags: '' });
    const resetEventForm = () => setEventForm({
        title: '', description: '', startDate: '', endDate: '', location: '',
        approximateVisitors: '', ticketPrice: '', organizer: '', status: 'Upcoming'
    });

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

    return (
        <div className="p-8">
            <div className="max-w-5xl mx-auto space-y-6">
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">Media Management</h1>
                        <p className="text-sm text-gray-600 mt-1">Create and manage news articles and events</p>
                    </div>
                </div>

                {/* Tabs */}
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
                </div>

                {/* News Tab */}
                {activeTab === 'news' && (
                    <>
                        <div className="flex items-center justify-between">
                            <p className="text-sm text-gray-500">{news.length} article{news.length !== 1 ? 's' : ''} published</p>
                            <button
                                onClick={() => { setEditingNewsId(null); resetNewsForm(); setShowNewsModal(true); }}
                                className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                            >
                                <i className="ri-add-line"></i>Create News
                            </button>
                        </div>

                        {news.length === 0 ? (
                            <div className="text-center py-16 bg-white rounded-lg border border-gray-200">
                                <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                                    <i className="ri-newspaper-line text-5xl text-gray-300"></i>
                                </div>
                                <p className="text-gray-500 text-sm">No news articles yet</p>
                            </div>
                        ) : (
                            <div className="grid gap-4">
                                {news.map(item => (
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
                        )}
                    </>
                )}

                {/* Events Tab */}
                {activeTab === 'events' && (
                    <>
                        <div className="flex items-center justify-between">
                            <p className="text-sm text-gray-500">{events.length} event{events.length !== 1 ? 's' : ''} created</p>
                            <button
                                onClick={() => { setEditingEventId(null); resetEventForm(); setShowEventModal(true); }}
                                className="px-4 py-2 bg-teal-600 text-white text-sm font-medium rounded-lg hover:bg-teal-700 transition-colors cursor-pointer whitespace-nowrap flex items-center gap-2"
                            >
                                <i className="ri-add-line"></i>Create Event
                            </button>
                        </div>

                        {events.length === 0 ? (
                            <div className="text-center py-16 bg-white rounded-lg border border-gray-200">
                                <div className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                                    <i className="ri-calendar-event-line text-5xl text-gray-300"></i>
                                </div>
                                <p className="text-gray-500 text-sm">No events created yet</p>
                            </div>
                        ) : (
                            <div className="grid gap-4">
                                {events.map(item => (
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
        </div>
    );
}