import ProfilePicture from "../profile/ProfilePicture";
import Flag from "../profile/Flag";
import useLiveChessboardStore from "@/stores/liveChessboardStore";
import { useChessStore } from "@/hooks/useChess";
import { invertColor } from "@/lib/utils/chessUtils";
import { GameColor } from "@/lib/apiClient";

export enum ProfileSide {
    CurrentlyPlaying,
    Opponent,
}

const LiveChessboardProfile = ({ side }: { side: ProfileSide }) => {
    const playingAs = useChessStore((state) => state.playingAs);
    const viewingFrom = useChessStore((state) => state.viewingFrom);
    const players = useLiveChessboardStore((state) => state.players);
    if (!playingAs) return null;

    const showSide = getShownSide(side, playingAs, viewingFrom);
    const player = players.colorToPlayer.get(showSide);
    if (!player) return null;

    return (
        <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
                <ProfilePicture height={50} width={50} />
                <Flag countryCode={player.countryCode} size={30} />

                <span className="font-medium text-white">
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

function getShownSide(
    side: ProfileSide,
    playingAs: GameColor,
    viewingFrom: GameColor,
): GameColor {
    const showSide =
        side === ProfileSide.CurrentlyPlaying
            ? playingAs
            : invertColor(playingAs);

    const shouldFlip = viewingFrom !== playingAs;
    return shouldFlip ? invertColor(showSide) : showSide;
}
