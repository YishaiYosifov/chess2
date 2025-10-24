import ProfilePicture from "@/features/profile/components/ProfilePicture";
import Flag from "@/features/profile/components/Flag";
import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { invertColor } from "@/lib/utils/chessUtils";
import { GameColor } from "@/lib/apiClient";
import useLiveChessStore from "../hooks/useLiveChessStore";
import GameClock from "./GameClock";
import ProfileTooltip from "@/features/profile/components/ProfileTooltip";

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
        <div className="flex max-w-screen items-center justify-between">
            <div className="flex items-center gap-3">
                <ProfileTooltip
                    username={player.userName}
                    userId={player.userId}
                >
                    <ProfilePicture userId={player.userId} size={50} />
                    <p className="truncate">{player.userName}</p>
                </ProfileTooltip>
                <Flag countryCode={player.countryCode} size={30} />

                {player.rating && (
                    <span className="text-text/80 w-fit rounded bg-white/10 px-2 py-0.5 text-xs">
                        {player.rating}
                    </span>
                )}
            </div>
            <GameClock color={color} />
        </div>
    );
};
export default LiveChessboardProfile;
