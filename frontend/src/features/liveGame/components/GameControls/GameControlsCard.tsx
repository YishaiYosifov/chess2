"use client";

import {
    ArrowPathIcon,
    ScaleIcon,
    XMarkIcon,
    PlusIcon,
    FlagIcon,
} from "@heroicons/react/24/solid";
import { useGameEmitter } from "@/features/signalR/hooks/useSignalRHubs";
import useLiveChessStore from "../../hooks/useLiveChessStore";
import GameControlButton from "./GameControlButton";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";
import DrawCard from "./DrawCard";
import { GameColor } from "@/lib/apiClient";

const GameControlsCard = () => {
    const resultData = useLiveChessStore((state) => state.resultData);

    return (
        <Card className="h justify-center gap-2">
            {resultData ? <GameOverControls /> : <LiveGameControls />}
        </Card>
    );
};
export default GameControlsCard;

const LiveGameControls = () => {
    const { gameToken, playerColor, canAbort, drawState } = useLiveChessStore(
        (state) => ({
            gameToken: state.gameToken,
            playerColor: state.playerColor,
            canAbort:
                state.positionHistory.length <=
                constants.ALLOW_ABORTION_UNTIL_MOVE,
            drawState: state.drawState,
        }),
    );

    const sendGameEvent = useGameEmitter(gameToken);
    const cooldown =
        playerColor === GameColor.WHITE
            ? drawState.whiteCooldown
            : drawState.blackCooldown;

    const endGame = () => sendGameEvent("EndGameAsync", gameToken);

    if (
        typeof drawState.activeRequester === "number" &&
        drawState.activeRequester !== playerColor
    )
        return <DrawCard />;

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
                disabled={
                    canAbort ||
                    cooldown > 0 ||
                    drawState.activeRequester === playerColor
                }
                onClick={() => sendGameEvent("RequestDrawAsync", gameToken)}
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
