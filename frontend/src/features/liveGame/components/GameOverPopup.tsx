import ProfilePicture from "@/features/profile/components/ProfilePicture";
import Card from "@/components/ui/Card";
import { GameColor, GamePlayer, GameResult } from "@/lib/apiClient";
import clsx from "clsx";
import Button from "@/components/ui/Button";
import {
    forwardRef,
    ForwardRefRenderFunction,
    useImperativeHandle,
    useState,
} from "react";
import useLiveChessStore from "../hooks/useLiveChessStore";
import useMatchmaking from "@/features/lobby/hooks/useMatchmaking";
import Popup from "@/components/Popup";

export interface GameOverPopupRef {
    open(): void;
}

const GameOverPopup: ForwardRefRenderFunction<GameOverPopupRef, unknown> = (
    _,
    ref,
) => {
    const { whitePlayer, blackPlayer, resultData, playerColor, pool } =
        useLiveChessStore((x) => ({
            whitePlayer: x.whitePlayer,
            blackPlayer: x.blackPlayer,
            resultData: x.resultData,
            playerColor: x.viewer.playerColor,
            pool: x.pool,
        }));
    const [isOpen, setIsOpen] = useState(false);

    const closePopup = () => setIsOpen(false);
    const openPopup = () => setIsOpen(true);

    const { toggleSeek, isSeeking } = useMatchmaking(pool);

    useImperativeHandle(ref, () => ({
        open: openPopup,
    }));

    if (!resultData || !isOpen) return;

    function getGameOverTitle(): string {
        if (!resultData) return "GAME OVER";

        switch (resultData.result) {
            case GameResult.ABORTED:
                return "ABORTED";
            case GameResult.DRAW:
                return "DRAW";
            case GameResult.WHITE_WIN:
            case GameResult.BLACK_WIN:
                const winColor =
                    resultData.result === GameResult.WHITE_WIN
                        ? GameColor.WHITE
                        : GameColor.BLACK;
                return playerColor === winColor ? "VICTORY" : "YOU LOST";
            default:
                return "GAME OVER";
        }
    }
    const gameOverTitle = getGameOverTitle();

    return (
        <Popup closePopup={closePopup} data-testid="gameOverPopup">
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
                        "w-full",
                        isSeeking && "animate-subtle-ping",
                    )}
                    onClick={() => toggleSeek()}
                >
                    {isSeeking ? "SEARCHING..." : "NEW GAME"}
                </Button>
                <Button className="w-full">REMATCH</Button>
            </div>
        </Popup>
    );
};
export default forwardRef(GameOverPopup);

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
