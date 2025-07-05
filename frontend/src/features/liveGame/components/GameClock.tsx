import { GameColor } from "@/lib/apiClient";
import { useLiveChessStore } from "../hooks/useLiveChess";
import { useCallback, useEffect, useState } from "react";
import clsx from "clsx";

const GameClock = ({ color }: { color: GameColor }) => {
    const clocks = useLiveChessStore((x) => x.clocks);
    const sideToMove = useLiveChessStore((x) => x.sideToMove);
    const result = useLiveChessStore((x) => x.resultData);

    const baseTimeLeft =
        color === GameColor.WHITE ? clocks.whiteClock : clocks.blackClock;

    const calculateTimeLeft = useCallback(() => {
        if (sideToMove != color) return baseTimeLeft;
        const timePassed = new Date().valueOf() - clocks.lastUpdated;
        return baseTimeLeft - timePassed;
    }, [clocks.lastUpdated, baseTimeLeft, sideToMove, color]);

    const [timeLeft, setTimeLeft] = useState<number>(0);

    useEffect(() => {
        setTimeLeft(calculateTimeLeft());
        if (sideToMove !== color || result) {
            return;
        }

        const interval = setInterval(
            () => setTimeLeft(calculateTimeLeft()),
            10,
        );

        return () => clearInterval(interval);
    }, [sideToMove, color, calculateTimeLeft, result]);

    const minutes = Math.max(0, Math.floor(timeLeft / 60000));
    const seconds = Math.max(0, (timeLeft % 60000) / 1000);

    const strMinutes = minutes.toString().padStart(2, "0");

    const isInTimeTrouble = minutes < 1 && seconds < 30;
    const strSeconds = isInTimeTrouble
        ? seconds.toFixed(2).padStart(5, "0") // xx.yy
        : Math.floor(seconds).toString().padStart(2, "0"); // xx

    return (
        <span
            className={clsx(
                "font-mono text-2xl",
                isInTimeTrouble && seconds > 0 && "blinking",
                seconds <= 0 && "text-red-600",
            )}
        >
            {strMinutes}:{strSeconds}
        </span>
    );
};
export default GameClock;
