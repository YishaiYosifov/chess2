"use client";

import { ArrowPathIcon } from "@heroicons/react/24/solid";
import { ScaleIcon } from "@heroicons/react/24/outline";
import { PlusIcon } from "@heroicons/react/24/solid";
import { FlagIcon } from "@heroicons/react/24/solid";

import Button from "../helpers/Button";
import Card from "../helpers/Card";
import useLiveChessboardStore from "@/stores/liveChessboardStore";
import { useGameEmitter } from "@/hooks/signalR/useSignalRHubs";

const GameControls = () => {
    const resultData = useLiveChessboardStore((state) => state.resultData);

    return (
        <Card className="gap-5">
            {resultData ? <GameOverControls /> : <LiveGameControls />}
        </Card>
    );
};
export default GameControls;

const LiveGameControls = () => {
    const gameToken = useLiveChessboardStore((state) => state.gameToken);
    const sendGameEvent = useGameEmitter(gameToken);

    return (
        <>
            <Button
                className="flex w-full justify-center gap-2"
                onClick={() => sendGameEvent("EndGameAsync", gameToken)}
            >
                <FlagIcon className="size-6" /> Resign
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
