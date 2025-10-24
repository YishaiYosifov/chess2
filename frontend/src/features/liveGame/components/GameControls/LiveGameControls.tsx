import { ScaleIcon, XMarkIcon, FlagIcon } from "@heroicons/react/24/solid";

import { GameColor } from "@/lib/apiClient";
import DrawCard from "./DrawCard";
import GameControlButton from "./GameControlButton";
import constants from "@/lib/constants";
import useLiveChessStore from "../../hooks/useLiveChessStore";
import { useGameEmitter } from "../../hooks/useGameHub";

const LiveGameControls = () => {
    const { gameToken, playerColor, canAbort, drawState } = useLiveChessStore(
        (x) => ({
            gameToken: x.gameToken,
            playerColor: x.viewer.playerColor,
            canAbort:
                x.positionHistory.length <= constants.ALLOW_ABORTION_UNTIL_MOVE,
            drawState: x.drawState,
        }),
    );

    const sendGameEvent = useGameEmitter(gameToken);

    const isDrawPending =
        typeof drawState.activeRequester === "number" &&
        drawState.activeRequester !== playerColor;

    const cooldown =
        playerColor === GameColor.WHITE
            ? drawState.whiteCooldown
            : drawState.blackCooldown;

    const offerDrawDisabled =
        canAbort || cooldown > 0 || drawState.activeRequester === playerColor;

    const endGame = () => sendGameEvent("EndGameAsync", gameToken);

    if (isDrawPending) return <DrawCard />;

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
                icon={ScaleIcon}
                title="Offer Draw"
                disabled={offerDrawDisabled}
                onClick={() => sendGameEvent("RequestDrawAsync", gameToken)}
                needsConfirmation
            />
        </>
    );
};
export default LiveGameControls;
