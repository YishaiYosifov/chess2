import { ArrowPathIcon, PlusIcon } from "@heroicons/react/24/solid";

import GameControlButton from "./GameControlButton";

import useLiveChessStore from "../../hooks/useLiveChessStore";
import useMatchmaking from "@/features/lobby/hooks/useMatchmaking";
import clsx from "clsx";
import useRematch from "../../hooks/useRematch";

const GameOverControls = () => {
    const pool = useLiveChessStore((x) => x.pool);
    const viewer = useLiveChessStore((x) => x.viewer);

    const { toggleSeek, isSeeking } = useMatchmaking(pool);

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
            {viewer.playerColor !== null && <RematchControls />}
        </>
    );
};
export default GameOverControls;

const RematchControls = () => {
    const {
        toggleRematch,
        requestRematch,
        isRequestingRematch,
        isRematchRequested,
    } = useRematch();

    if (isRematchRequested) {
        return (
            <GameControlButton
                icon={ArrowPathIcon}
                onClick={requestRematch}
                className="bg-secondary enabled:hover:bg-card
                    enabled:hover:text-text text-black"
                data-testid="gameOverControlsRematch"
            >
                Rematch?
            </GameControlButton>
        );
    } else {
        return (
            <GameControlButton
                icon={ArrowPathIcon}
                className={clsx(isRequestingRematch && "animate-subtle-ping")}
                onClick={toggleRematch}
                data-testid="gameOverControlsRematch"
            >
                Rematch
            </GameControlButton>
        );
    }
};
