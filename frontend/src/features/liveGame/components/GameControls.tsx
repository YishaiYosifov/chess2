"use client";

import { ArrowPathIcon } from "@heroicons/react/24/solid";
import { ScaleIcon } from "@heroicons/react/24/solid";
import { XMarkIcon } from "@heroicons/react/24/solid";
import { PlusIcon } from "@heroicons/react/24/solid";
import { FlagIcon } from "@heroicons/react/24/solid";

import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import { useGameEmitter } from "@/features/signalR/hooks/useSignalRHubs";
import { useLiveChessStore } from "../hooks/useLiveChessStore";
import constants from "@/lib/constants";
import React, { useRef, useState } from "react";
import clsx from "clsx";

const GameControls = () => {
    const resultData = useLiveChessStore((state) => state.resultData);

    return (
        <Card className="justify-center gap-5">
            {resultData ? <GameOverControls /> : <LiveGameControls />}
        </Card>
    );
};
export default GameControls;

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
                    needsConfirmation={false}
                />
            )}
            <GameControlButton
                icon={FlagIcon}
                title="Resign"
                disabled={canAbort}
                onClick={endGame}
            />
            <GameControlButton
                disabled={canAbort}
                icon={ScaleIcon}
                title="Offer Draw"
            />
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

const GameControlButton = ({
    icon,
    needsConfirmation = true,
    onClick,
    ...props
}: React.ButtonHTMLAttributes<HTMLButtonElement> & {
    icon: React.ElementType;
    needsConfirmation?: boolean;
}) => {
    const Component = icon;

    const [isConfirming, setIsConfirming] = useState(false);
    const timeoutRef = useRef<NodeJS.Timeout>(null);

    function confirmClick(
        event: React.MouseEvent<HTMLButtonElement, MouseEvent>,
    ) {
        if (!needsConfirmation || isConfirming) {
            onClick?.(event);
            return;
        }

        setIsConfirming(true);

        if (timeoutRef.current) clearTimeout(timeoutRef.current);
        timeoutRef.current = setTimeout(() => setIsConfirming(false), 3000);
    }

    return (
        <div className="flex gap-1">
            <button
                className={clsx(
                    "flex w-20 cursor-pointer items-center justify-center rounded-md transition",
                    isConfirming
                        ? "border-b-4 border-orange-800 bg-orange-600 p-1 hover:brightness-75"
                        : `enabled:hover:bg-secondary enabled:hover:text-neutral-900
                            disabled:cursor-not-allowed disabled:brightness-75`,
                )}
                onClick={confirmClick}
                {...props}
            >
                <Component className="h-full max-h-full w-full max-w-full" />
            </button>

            {isConfirming && (
                <button
                    type="button"
                    onClick={() => setIsConfirming(false)}
                    className="hover:text-secondary cursor-pointer p-1 transition"
                    title="Cancel"
                    aria-label="Cancel confirmation"
                >
                    <XMarkIcon className="h-6 w-6" />
                </button>
            )}
        </div>
    );
};
