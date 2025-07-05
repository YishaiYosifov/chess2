"use client";

import { ArrowPathIcon } from "@heroicons/react/24/solid";
import { ScaleIcon } from "@heroicons/react/24/outline";
import { PlusIcon } from "@heroicons/react/24/solid";
import { FlagIcon } from "@heroicons/react/24/solid";

import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import { useGameEmitter } from "@/features/signalR/hooks/useSignalRHubs";
import { useLiveChessStore } from "../hooks/useLiveChessStore";

const GameControls = () => {
    const resultData = useLiveChessStore((state) => state.resultData);

    return (
        <Card className="gap-5">
            {resultData ? <GameOverControls /> : <LiveGameControls />}
        </Card>
    );
};
export default GameControls;

const LiveGameControls = () => {
    const moveHistory = useLiveChessStore((state) => state.moveHistory);
    const gameToken = useLiveChessStore((state) => state.gameToken);
    const sendGameEvent = useGameEmitter(gameToken);

    const endGameText = moveHistory.length < 3 ? "Abort" : "Resign";

    return (
        <>
            <Button
                className="flex w-full justify-center gap-2"
                onClick={() => sendGameEvent("EndGameAsync", gameToken)}
            >
                <FlagIcon className="size-6" /> {endGameText}
            </Button>
            <Button className="flex w-full justify-center gap-2">
                <ScaleIcon className="size-6" /> Draw
            </Button>
        </>
    );
};

const GameOverControls = () => {
    return (
        <>
            <Button className="flex w-full justify-center gap-2">
                <PlusIcon className="size-6" /> New Game
            </Button>
            <Button className="flex w-full justify-center gap-2">
                <ArrowPathIcon className="size-6" /> Rematch
            </Button>
        </>
    );
};
