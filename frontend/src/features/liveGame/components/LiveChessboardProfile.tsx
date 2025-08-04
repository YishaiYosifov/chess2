import ProfilePicture from "@/features/profile/components/ProfilePicture";
import Flag from "@/features/profile/components/Flag";
import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { invertColor } from "@/lib/utils/chessUtils";
import { GameColor } from "@/lib/apiClient";
import useLiveChessStore from "../hooks/useLiveChessStore";
import GameClock from "./GameClock";

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
            <GameClock color={color} />
        </div>
    );
};
export default LiveChessboardProfile;
