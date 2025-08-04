"use client";

import { ArrowPathIcon } from "@heroicons/react/24/solid";
import { ScaleIcon } from "@heroicons/react/24/solid";
import { XMarkIcon } from "@heroicons/react/24/solid";
import { PlusIcon } from "@heroicons/react/24/solid";
import { FlagIcon } from "@heroicons/react/24/solid";

import { useGameEmitter } from "@/features/signalR/hooks/useSignalRHubs";
import useLiveChessStore from "../../hooks/useLiveChessStore";
import GameControlButton from "./GameControlButton";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";

const GameControlsCard = () => {
    const resultData = useLiveChessStore((state) => state.resultData);

    return (
        <Card className="justify-center gap-2">
            {resultData ? <GameOverControls /> : <LiveGameControls />}
        </Card>
    );
};
export default GameControlsCard;

const LiveGameControls = () => {
    const positionHistory = useLiveChessStore((state) => state.positionHistory);
    const gameToken = useLiveChessStore((state) => state.gameToken);
    const sendGameEvent = useGameEmitter(gameToken);

    const endGame = () => sendGameEvent("EndGameAsync", gameToken);

    const canAbort =
        positionHistory.length <= constants.ALLOW_ABORTION_UNTIL_MOVE;

    return (
        <>
            {canAbort && (
                <GameControlButton
                    icon={XMarkIcon}
                    title="Abort"
                    onClick={endGame}
                />
            )}
            <GameControlButton
                icon={FlagIcon}
                title="Resign"
                disabled={canAbort}
                onClick={endGame}
                needsConfirmation
            />
            <GameControlButton
                disabled={canAbort}
                icon={ScaleIcon}
                title="Offer Draw"
                needsConfirmation
            />
        </>
    );
};

const GameOverControls = () => {
    return (
        <>
            <GameControlButton icon={PlusIcon}>New Game</GameControlButton>
            <GameControlButton icon={ArrowPathIcon}>Rematch</GameControlButton>
        </>
    );
};
