import { Renderable, renderRenderable } from "@/lib/utils/renderable";
import { useEffect, useRef, useState } from "react";

const CountdownText = ({
    getTimeUntil,
    onDateReached,
    children,
}: {
    getTimeUntil: () => Date;
    onDateReached?: () => void;
    children: Renderable<{ countdown: string }>;
}) => {
    const [countdown, setCountdown] = useState("");
    const until = useRef<Date>(null);

    function formatTime(until: Date) {
        const now = new Date();

        const diffSeconds = Math.floor(
            (until.valueOf() - now.valueOf()) / 1000,
        );

        const days = Math.floor(diffSeconds / 86400);
        const hours = Math.floor((diffSeconds % 86400) / 3600);
        const minutes = Math.floor((diffSeconds % 3600) / 60);
        const seconds = diffSeconds % 60;

        const hh = String(hours).padStart(2, "0");
        const mm = String(minutes).padStart(2, "0");
        const ss = String(seconds).padStart(2, "0");

        return days > 0 ? `${days}:${hh}:${mm}:${ss}` : `${hh}:${mm}:${ss}`;
    }

    useEffect(() => {
        function updateFormattedCountdown() {
            if (!until.current) until.current = getTimeUntil();

            const now = new Date().valueOf();
            if (until.current.valueOf() - now <= 0) {
                onDateReached?.();
                until.current = getTimeUntil();
            }

            setCountdown(formatTime(until.current));
        }

        updateFormattedCountdown();
        const interval = setInterval(updateFormattedCountdown, 1000);

        return () => clearInterval(interval);
    }, [getTimeUntil, onDateReached]);

    return renderRenderable(children, { countdown });
};
export default CountdownText;
