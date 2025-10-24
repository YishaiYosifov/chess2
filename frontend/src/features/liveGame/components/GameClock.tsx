import { GameColor } from "@/lib/apiClient";
import useLiveChessStore from "../hooks/useLiveChessStore";
import { useCallback, useEffect, useState } from "react";
import clsx from "clsx";

const GameClock = ({ color }: { color: GameColor }) => {
    const { clocks, sideToMove, result } = useLiveChessStore((x) => ({
        clocks: x.clocks,
        sideToMove: x.sideToMove,
        result: x.resultData,
    }));

    const baseTimeLeft =
        color === GameColor.WHITE ? clocks.whiteClock : clocks.blackClock;
    const isTicking = sideToMove === color;
    const isGameOver = Boolean(result);

    const calculateTimeLeft = useCallback(() => {
        if (!isTicking || !clocks.lastUpdated) return baseTimeLeft;

        const timePassed = new Date().valueOf() - clocks.lastUpdated;
        return baseTimeLeft - timePassed;
    }, [clocks.lastUpdated, baseTimeLeft, isTicking]);

    const [timeLeft, setTimeLeft] = useState<number>(baseTimeLeft);

    useEffect(() => {
        setTimeLeft(calculateTimeLeft());
        if (!isTicking || isGameOver) return;

        const interval = setInterval(
            () => setTimeLeft(calculateTimeLeft()),
            10,
        );

        return () => clearInterval(interval);
    }, [calculateTimeLeft, isTicking, isGameOver]);

    const minutes = Math.max(0, Math.floor(timeLeft / 60000));
    const seconds = Math.max(0, (timeLeft % 60000) / 1000);

    const strMinutes = minutes.toString().padStart(2, "0");

    const isInTimeTrouble = minutes < 1 && seconds < 20;
    const strSeconds = isInTimeTrouble
        ? seconds.toFixed(2).padStart(5, "0") // xx.yy
        : Math.floor(seconds).toString().padStart(2, "0"); // xx

    return (
        <span
            className={clsx(
                "font-mono text-2xl",
                isInTimeTrouble &&
                    isTicking &&
                    !isGameOver &&
                    "animate-freakout",
                seconds <= 0 && minutes <= 0 && isGameOver && "text-red-600",
            )}
        >
            {strMinutes}:{strSeconds}
        </span>
    );
};
export default GameClock;
