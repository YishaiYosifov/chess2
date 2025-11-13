import ProfilePicture from "@/features/profile/components/ProfilePicture";
import Card from "@/components/ui/Card";
import { GameColor, GamePlayer, GameResult } from "@/lib/apiClient";
import clsx from "clsx";
import Button from "@/components/ui/Button";
import { useEffect, useRef } from "react";
import useLiveChessStore from "../hooks/useLiveChessStore";
import useMatchmaking from "@/features/lobby/hooks/useMatchmaking";
import Popup, { PopupRef } from "@/components/Popup";
import useRematch from "../hooks/useRematch";

const GameOverPopup = () => {
    const { whitePlayer, blackPlayer, resultData, viewer, pool } =
        useLiveChessStore((x) => ({
            whitePlayer: x.whitePlayer,
            blackPlayer: x.blackPlayer,
            resultData: x.resultData,
            viewer: x.viewer,
            pool: x.pool,
        }));

    const { toggleSeek, isSeeking } = useMatchmaking(pool);
    const popupRef = useRef<PopupRef>(null);

    useEffect(() => {
        if (resultData && popupRef.current) popupRef.current.open();
    }, [resultData]);

    if (!resultData) return;

    function getGameOverTitle(): string {
        if (!resultData) return "GAME OVER";

        switch (resultData.result) {
            case GameResult.ABORTED:
                return "ABORTED";
            case GameResult.DRAW:
                return "DRAW";
            case GameResult.WHITE_WIN:
            case GameResult.BLACK_WIN:
                if (viewer.playerColor === null) return "GAME OVER";

                const winColor =
                    resultData.result === GameResult.WHITE_WIN
                        ? GameColor.WHITE
                        : GameColor.BLACK;
                return viewer.playerColor === winColor ? "VICTORY" : "YOU LOST";
            default:
                return "GAME OVER";
        }
    }
    const gameOverTitle = getGameOverTitle();

    return (
        <Popup data-testid="gameOverPopup" ref={popupRef}>
            <h2 className="text-center text-3xl font-bold">{gameOverTitle}</h2>
            <p className="text-secondary text-center">
                {resultData.resultDescription}
            </p>

            <div className="grid grid-cols-2 justify-center gap-2">
                <PopupCardProfile
                    player={whitePlayer}
                    ratingChange={resultData.whiteRatingChange ?? null}
                    isWinner={resultData.result === GameResult.WHITE_WIN}
                />
                <PopupCardProfile
                    player={blackPlayer}
                    ratingChange={resultData.blackRatingChange ?? null}
                    isWinner={resultData.result === GameResult.BLACK_WIN}
                />
            </div>

            <div className="flex gap-3">
                <Button
                    className={clsx(
                        "flex-1",
                        isSeeking && "animate-subtle-ping",
                    )}
                    onClick={() => toggleSeek()}
                >
                    {isSeeking ? "SEARCHING..." : "NEW GAME"}
                </Button>
                {viewer.playerColor !== null && <RematchButton />}
            </div>
        </Popup>
    );
};
export default GameOverPopup;

const RematchButton = () => {
    const {
        toggleRematch,
        requestRematch,
        isRequestingRematch,
        isRematchRequested,
    } = useRematch();

    if (isRematchRequested) {
        return (
            <Button
                onClick={requestRematch}
                className="bg-secondary flex-1 text-black"
            >
                REMATCH?
            </Button>
        );
    } else {
        return (
            <Button
                onClick={toggleRematch}
                className={clsx(
                    "flex-1",
                    isRequestingRematch && "animate-subtle-ping",
                )}
            >
                REMATCH
            </Button>
        );
    }
};

const PopupCardProfile = ({
    player,
    ratingChange,
    isWinner,
}: {
    player: GamePlayer;
    ratingChange: number | null;
    isWinner: boolean;
}) => {
    return (
        <Card
            data-testid={`gameOverPopupProfile-${player.color}`}
            className={clsx(
                "items-center text-center",
                isWinner && "border-3 border-amber-500",
            )}
        >
            <ProfilePicture userId={player.userId} />
            <p className="w-full overflow-hidden text-sm text-ellipsis whitespace-nowrap">
                {player.userName}
            </p>
            {player.rating && ratingChange !== null && (
                <p className="flex gap-2 text-xl">
                    {player.rating}
                    <span
                        className={clsx(
                            ratingChange > 0 && "text-green-500",
                            ratingChange < 0 && "text-red-500",
                            ratingChange === 0 && "text-gray-400",
                        )}
                    >
                        {ratingChange >= 0 ? `+${ratingChange}` : ratingChange}
                    </span>
                </p>
            )}
        </Card>
    );
};
