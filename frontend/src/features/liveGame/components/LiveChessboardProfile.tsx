import ProfilePicture from "../profile/ProfilePicture";
import Flag from "../profile/Flag";
import useLiveChessboardStore from "@/features/liveGame/stores/liveChessboardStore";
import { useChessStore } from "@/features/chessboard/hooks/useChess";
import { invertColor } from "@/lib/utils/chessUtils";
import { GameColor } from "@/lib/apiClient";

export enum ProfileSide {
    CurrentlyPlaying,
    Opponent,
}

const LiveChessboardProfile = ({ side }: { side: ProfileSide }) => {
    const viewingFrom = useChessStore((state) => state.viewingFrom);
    const showSide =
        side === ProfileSide.CurrentlyPlaying
            ? viewingFrom
            : invertColor(viewingFrom);

    const player = useLiveChessboardStore((state) =>
        showSide === GameColor.WHITE ? state.whitePlayer : state.blackPlayer,
    );
    if (!player) return null;

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
            <span className="text-2xl">1:23:45</span>
        </div>
    );
};
export default LiveChessboardProfile;
