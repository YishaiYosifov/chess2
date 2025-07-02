import useLiveChessboardStore from "@/stores/liveChessboardStore";
import ProfilePicture from "../profile/ProfilePicture";
import Card from "../helpers/Card";
import { GameColor, GamePlayer } from "@/lib/apiClient";
import clsx from "clsx";
import Button from "../helpers/Button";
import { GameResult } from "@/types/tempModels";
import {
    forwardRef,
    ForwardRefRenderFunction,
    useImperativeHandle,
    useState,
} from "react";

export interface GameOverPopupRef {
    open(): void;
}

const GameOverPopup: ForwardRefRenderFunction<GameOverPopupRef, unknown> = (
    _,
    ref,
) => {
    const whitePlayer = useLiveChessboardStore((x) => x.whitePlayer);
    const blackPlayer = useLiveChessboardStore((x) => x.blackPlayer);
    const resultData = useLiveChessboardStore((x) => x.resultData);
    const playerColor = useLiveChessboardStore((x) => x.playerColor);
    const [isOpen, setIsOpen] = useState(false);

    const closePopup = () => setIsOpen(false);
    const openPopup = () => setIsOpen(true);

    useImperativeHandle(ref, () => ({
        open: openPopup,
    }));

    if (!whitePlayer || !blackPlayer || !resultData || !isOpen) return;

    function getGameOverTitle(): string {
        if (!resultData) return "GAME OVER";

        if (resultData.result === GameResult.ABORTED) return "ABORTED";
        else if (resultData.result === GameResult.DRAW) return "DRAW";

        const winColor =
            resultData.result === GameResult.WHITE_WIN
                ? GameColor.WHITE
                : GameColor.BLACK;
        return playerColor === winColor ? "VICTORY" : "GAME OVER";
    }
    const gameOverTitle = getGameOverTitle();

    return (
        <div
            className="fixed inset-0 z-50 flex min-h-screen items-center justify-center bg-black/60 p-4"
            onClick={closePopup}
            data-testid="gameOverPopupBackground"
        >
            <div
                className="bg-background shadow-x4 relative flex h-min max-h-full w-full max-w-md flex-col
                    gap-3 overflow-auto rounded-2xl p-8"
                onClick={(e) => e.stopPropagation()}
                data-testid="gameOverPopup"
            >
                <button
                    onClick={closePopup}
                    aria-label="Close popup"
                    className="hover:text-text/80 absolute top-2 right-4 cursor-pointer text-4xl"
                    data-testid="closeGameOverPopup"
                >
                    ×
                </button>
                <h2 className="text-center text-3xl font-bold">
                    {gameOverTitle}
                </h2>
                <p className="text-secondary text-center">
                    {resultData.resultDescription}
                </p>

                <div className="grid grid-cols-2 justify-center gap-2">
                    <PopupCardProfile
                        player={whitePlayer}
                        ratingDelta={resultData.whiteRatingDelta}
                        isWinner={resultData.result === GameResult.WHITE_WIN}
                    />
                    <PopupCardProfile
                        player={blackPlayer}
                        ratingDelta={resultData.blackRatingDelta}
                        isWinner={resultData.result === GameResult.BLACK_WIN}
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
export default forwardRef(GameOverPopup);

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
            data-testid={`gameOverPopupProfile-${player.color}`}
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
