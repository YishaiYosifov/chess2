import { ArrowPathIcon, PlusIcon } from "@heroicons/react/24/solid";

import GameControlButton from "./GameControlButton";

import useLiveChessStore from "../../hooks/useLiveChessStore";
import useMatchmaking from "@/features/lobby/hooks/useMatchmaking";
import clsx from "clsx";
import useRematch from "../../hooks/useRematch";

const GameOverControls = () => {
    const pool = useLiveChessStore((x) => x.pool);

    const { toggleSeek, isSeeking } = useMatchmaking(pool);
    const {
        toggleRematch,
        requestRematch,
        isRequestingRematch,
        isRematchRequested,
    } = useRematch();

    return (
        <>
            <GameControlButton
                icon={PlusIcon}
                className={clsx(isSeeking && "animate-subtle-ping")}
                onClick={toggleSeek}
                data-testid="gameOverControlsNewGame"
            >
                {isSeeking ? "Searching..." : "New Game"}
            </GameControlButton>
            {isRematchRequested ? (
                <GameControlButton
                    icon={ArrowPathIcon}
                    onClick={requestRematch}
                    className="bg-secondary enabled:hover:bg-card text-black enabled:hover:text-white"
                    data-testid="gameOverControlsRematch"
                >
                    Rematch?
                </GameControlButton>
            ) : (
                <GameControlButton
                    icon={ArrowPathIcon}
                    className={clsx(
                        isRequestingRematch && "animate-subtle-ping",
                    )}
                    onClick={toggleRematch}
                    data-testid="gameOverControlsRematch"
                >
                    Rematch
                </GameControlButton>
            )}
        </>
    );
};
export default GameOverControls;
