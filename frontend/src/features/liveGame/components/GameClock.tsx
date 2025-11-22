import { GameColor } from "@/lib/apiClient";
import useLiveChessStore from "../hooks/useLiveChessStore";
import { useCallback, useEffect, useRef, useState } from "react";
import clsx from "clsx";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";

const GameClock = ({ color }: { color: GameColor }) => {
    const { clocks, sideToMove, viewer } = useLiveChessStore((x) => ({
        clocks: x.clocks,
        sideToMove: x.sideToMove,
        viewer: x.viewer,
    }));

    const playedWarningSoundRef = useRef<boolean>(false);

    const baseTimeLeft =
        color === GameColor.WHITE ? clocks.whiteClock : clocks.blackClock;
    const isTicking = sideToMove === color && !clocks.isFrozen;

    const [timeLeft, setTimeLeft] = useState<number>(baseTimeLeft);

    const calculateTimeLeft = useCallback(() => {
        if (!isTicking) return baseTimeLeft;

        const timePassed = new Date().valueOf() - clocks.lastUpdated;
        return baseTimeLeft - timePassed;
    }, [clocks.lastUpdated, baseTimeLeft, isTicking]);

    useEffect(() => {
        setTimeLeft(calculateTimeLeft());
        if (!isTicking) return;

        const interval = setInterval(
            () => setTimeLeft(calculateTimeLeft()),
            100,
        );

        return () => clearInterval(interval);
    }, [calculateTimeLeft, isTicking]);

    useEffect(() => {
        if (
            clocks.isFrozen ||
            timeLeft > 20000 ||
            playedWarningSoundRef.current ||
            viewer.playerColor !== color
        )
            return;

        AudioPlayer.playAudio(AudioType.LOW_TIME);
        playedWarningSoundRef.current = true;
    }, [timeLeft, color, viewer.playerColor, clocks.isFrozen]);

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
                isInTimeTrouble && isTicking && "animate-freakout",
                seconds <= 0 &&
                    minutes <= 0 &&
                    clocks.isFrozen &&
                    "text-red-600",
            )}
        >
            {strMinutes}:{strSeconds}
        </span>
    );
};
export default GameClock;
