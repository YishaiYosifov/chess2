import ProfilePicture from "@/features/profile/components/ProfilePicture";
import Flag from "@/features/profile/components/Flag";
import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { invertColor } from "@/lib/utils/chessUtils";
import { GameColor } from "@/lib/apiClient";
import { useLiveChessStore } from "../hooks/useLiveChess";
import { useCallback, useEffect, useState } from "react";

export enum ProfileSide {
    CurrentlyPlaying,
    Opponent,
}

const LiveChessboardProfile = ({ side }: { side: ProfileSide }) => {
    const viewingFrom = useChessboardStore((state) => state.viewingFrom);
    const color =
        side === ProfileSide.CurrentlyPlaying
            ? viewingFrom
            : invertColor(viewingFrom);

    const player = useLiveChessStore((x) =>
        color === GameColor.WHITE ? x.whitePlayer : x.blackPlayer,
    );

    return (
        <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
                <ProfilePicture height={50} width={50} />
                <Flag countryCode={player.countryCode} size={30} />

                <span className="overflow-hidden font-medium text-ellipsis whitespace-nowrap text-white">
                    {player.userName}
                </span>

                {player.rating && (
                    <span className="w-fit rounded bg-white/10 px-2 py-0.5 text-xs text-white/80">
                        {player.rating}
                    </span>
                )}
            </div>
            <Clock color={color} />
        </div>
    );
};
export default LiveChessboardProfile;

const Clock = ({ color }: { color: GameColor }) => {
    const clocks = useLiveChessStore((x) => x.clocks);
    const sideToMove = useLiveChessStore((x) => x.sideToMove);
    const baseTimeLeft =
        color === GameColor.WHITE ? clocks.whiteClock : clocks.blackClock;

    const calculateTimeLeft = useCallback(() => {
        if (sideToMove != color) return baseTimeLeft;
        const timePassed = new Date().valueOf() - clocks.lastUpdated;
        return baseTimeLeft - timePassed;
    }, [clocks, baseTimeLeft, sideToMove, color]);

    const [timeLeft, setTimeLeft] = useState<number>(0);

    useEffect(() => {
        if (sideToMove !== color) {
            setTimeLeft(calculateTimeLeft());
            return;
        }

        const interval = setInterval(
            () => setTimeLeft(calculateTimeLeft()),
            100,
        );

        return () => clearInterval(interval);
    }, [sideToMove, color, calculateTimeLeft]);

    const minutes = Math.floor(timeLeft / 60000);
    const seconds = (timeLeft % 6000) / 1000;
    return (
        <span className="font-mono text-2xl">
            {minutes.toString().padStart(2, "0")}:
            {seconds.toFixed(2).padStart(5, "0")}
        </span>
    );
};
