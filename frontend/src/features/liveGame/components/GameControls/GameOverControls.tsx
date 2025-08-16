import { ArrowPathIcon, PlusIcon } from "@heroicons/react/24/solid";

import GameControlButton from "./GameControlButton";

import useLiveChessStore from "../../hooks/useLiveChessStore";
import useMatchmaking from "@/features/lobby/hooks/useMatchmaking";
import clsx from "clsx";

const GameOverControls = () => {
    const pool = useLiveChessStore((x) => x.pool);

    const { toggleSeek, isSeeking } = useMatchmaking(pool);

    return (
        <>
            <GameControlButton
                icon={PlusIcon}
                className={clsx(isSeeking && "animate-subtle-ping")}
                onClick={() => toggleSeek()}
            >
                {isSeeking ? "Searching..." : "New Game"}
            </GameControlButton>
            <GameControlButton icon={ArrowPathIcon}>Rematch</GameControlButton>
        </>
    );
};
export default GameOverControls;
