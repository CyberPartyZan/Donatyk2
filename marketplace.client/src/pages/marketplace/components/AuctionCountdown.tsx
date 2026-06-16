import { useState, useEffect } from 'react';

interface AuctionCountdownProps {
    endsAt: string;
}

interface TimeLeft {
    days: number;
    hours: number;
    minutes: number;
    seconds: number;
}

function getTimeLeft(endsAt: string): TimeLeft {
    const diff = new Date(endsAt).getTime() - Date.now();
    if (diff <= 0) return { days: 0, hours: 0, minutes: 0, seconds: 0 };
    return {
        days: Math.floor(diff / (1000 * 60 * 60 * 24)),
        hours: Math.floor((diff / (1000 * 60 * 60)) % 24),
        minutes: Math.floor((diff / (1000 * 60)) % 60),
        seconds: Math.floor((diff / 1000) % 60),
    };
}

export default function AuctionCountdown({ endsAt }: AuctionCountdownProps) {
    const [timeLeft, setTimeLeft] = useState<TimeLeft>(getTimeLeft(endsAt));

    useEffect(() => {
        const timer = setInterval(() => {
            setTimeLeft(getTimeLeft(endsAt));
        }, 1000);
        return () => clearInterval(timer);
    }, [endsAt]);

    const isExpired = timeLeft.days === 0 && timeLeft.hours === 0 && timeLeft.minutes === 0 && timeLeft.seconds === 0;
    const isUrgent = !isExpired && timeLeft.days === 0 && timeLeft.hours < 2;

    if (isExpired) {
        return (
            <div className="flex items-center gap-1.5 px-2 py-1 bg-gray-100 rounded-md">
                <i className="ri-time-line text-gray-400 text-xs"></i>
                <span className="text-xs font-medium text-gray-400">Auction ended</span>
            </div>
        );
    }

    return (
        <div className={`flex items-center gap-1.5 px-2 py-1 rounded-md ${isUrgent ? 'bg-red-50' : 'bg-amber-50'}`}>
            <i className={`ri-alarm-line text-xs ${isUrgent ? 'text-red-500' : 'text-amber-500'}`}></i>
            <div className="flex items-center gap-0.5">
                {timeLeft.days > 0 && (
                    <>
                        <span className={`text-xs font-bold tabular-nums ${isUrgent ? 'text-red-600' : 'text-amber-600'}`}>
                            {String(timeLeft.days).padStart(2, '0')}
                        </span>
                        <span className={`text-xs ${isUrgent ? 'text-red-400' : 'text-amber-400'}`}>d</span>
                        <span className={`text-xs mx-0.5 ${isUrgent ? 'text-red-400' : 'text-amber-400'}`}>:</span>
                    </>
                )}
                <span className={`text-xs font-bold tabular-nums ${isUrgent ? 'text-red-600' : 'text-amber-600'}`}>
                    {String(timeLeft.hours).padStart(2, '0')}
                </span>
                <span className={`text-xs ${isUrgent ? 'text-red-400' : 'text-amber-400'}`}>h</span>
                <span className={`text-xs mx-0.5 ${isUrgent ? 'text-red-400' : 'text-amber-400'}`}>:</span>
                <span className={`text-xs font-bold tabular-nums ${isUrgent ? 'text-red-600' : 'text-amber-600'}`}>
                    {String(timeLeft.minutes).padStart(2, '0')}
                </span>
                <span className={`text-xs ${isUrgent ? 'text-red-400' : 'text-amber-400'}`}>m</span>
                <span className={`text-xs mx-0.5 ${isUrgent ? 'text-red-400' : 'text-amber-400'}`}>:</span>
                <span className={`text-xs font-bold tabular-nums ${isUrgent ? 'text-red-600' : 'text-amber-600'}`}>
                    {String(timeLeft.seconds).padStart(2, '0')}
                </span>
                <span className={`text-xs ${isUrgent ? 'text-red-400' : 'text-amber-400'}`}>s</span>
            </div>
        </div>
    );
}
