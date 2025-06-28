import useLiveChessboardStore from "@/stores/liveChessboardStore";
import ProfilePicture from "../profile/ProfilePicture";
import Card from "../helpers/Card";
import { GamePlayer } from "@/lib/apiClient";
import clsx from "clsx";
import Button from "../helpers/Button";
import { GameResult } from "@/types/tempModels";

const GameOverPopup = () => {
    const whitePlayer = useLiveChessboardStore((x) => x.whitePlayer);
    const blackPlayer = useLiveChessboardStore((x) => x.blackPlayer);
    const result = useLiveChessboardStore((x) => x.result);
    if (!whitePlayer || !blackPlayer || !result) return;

    return (
        <div className="fixed inset-0 z-50 flex min-h-screen items-center justify-center bg-black/60 p-4">
            <div
                className="bg-background shadow-x4 flex h-min max-h-full w-full max-w-md flex-col gap-3
                    overflow-auto rounded-2xl p-8"
            >
                <h2 className="text-center text-3xl font-bold">GAME OVER</h2>
                <p className="text-secondary text-center">
                    You lost by resignation. Better luck next time!
                </p>

                <div className="grid grid-cols-2 justify-center gap-2">
                    <PopupCardProfile
                        player={whitePlayer}
                        ratingDelta={result.whiteRatingDelta}
                        isWinner={result.gameResult === GameResult.WHITE_WIN}
                    />

                    <PopupCardProfile
                        player={blackPlayer}
                        ratingDelta={result.blackRatingDelta}
                        isWinner={result.gameResult === GameResult.BLACK_WIN}
                    />
                </div>
                <div className="flex gap-3">
                    <Button className="w-full">NEW GAME</Button>
                    <Button className="w-full">REMATCH</Button>
                </div>
            </div>
        </div>
    );
};
export default GameOverPopup;

const PopupCardProfile = ({
    player,
    ratingDelta,
    isWinner,
}: {
    player: GamePlayer;
    ratingDelta?: number;
    isWinner: boolean;
}) => {
    return (
        <Card
            className={clsx(
                "flex-col items-center gap-3 text-center",
                isWinner && "border-3 border-amber-500",
            )}
        >
            <ProfilePicture userId={player.userId} />
            <p className="w-full overflow-hidden text-sm text-ellipsis whitespace-nowrap">
                {player.userName}
            </p>
            {player.rating && ratingDelta !== undefined && (
                <p className="flex gap-2 text-xl">
                    {player.rating}
                    <span
                        className={clsx(
                            ratingDelta > 0 && "text-green-500",
                            ratingDelta < 0 && "text-red-500",
                            ratingDelta === 0 && "text-gray-400",
                        )}
                    >
                        {ratingDelta >= 0 ? `+${ratingDelta}` : ratingDelta}
                    </span>
                </p>
            )}
        </Card>
    );
};
