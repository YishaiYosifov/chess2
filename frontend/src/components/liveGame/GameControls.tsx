"use client";

import { ScaleIcon } from "@heroicons/react/24/outline";
import { FlagIcon } from "@heroicons/react/24/solid";

import Button from "../helpers/Button";
import Card from "../helpers/Card";
import useLiveChessboardStore from "@/stores/liveChessboardStore";
import { useGameEmitter } from "@/hooks/signalR/useSignalRHubs";

const GameControls = () => {
    const gameToken = useLiveChessboardStore((state) => state.gameToken);
    const sendGameEvent = useGameEmitter(gameToken);

    return (
        <Card className="gap-5">
            <Button
                className="flex w-full justify-center gap-2"
                onClick={() => sendGameEvent("EndGameAsync", gameToken)}
            >
                <FlagIcon className="size-6" /> Resign
            </Button>
            <Button className="flex w-full justify-center gap-2">
                <ScaleIcon className="size-6" /> Draw
            </Button>
        </Card>
    );
};
export default GameControls;
