"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import Cookies from "js-cookie";

import {
    useMatchmakingEmitter,
    useMatchmakingEvent,
} from "@/hooks/signalR/useSignalRHubs";

import PoolToggle, { PoolToggleRef } from "./PoolToggle";
import constants from "@/lib/constants";
import Button from "../helpers/Button";
import Card from "../helpers/Card";
import clsx from "clsx";

/**
 * Card containing the variant and time control options.
 * When the one of the time control buttons is clicked, a request to enter the pool will be sent.
 */
const PlayOptions = () => {
    const [showPoolToggle, setShowPoolToggle] = useState(false);
    const poolToggleRef = useRef<PoolToggleRef>(null);
    const router = useRouter();

    const sendMatchmakingEvent = useMatchmakingEmitter();
    useMatchmakingEvent("MatchFoundAsync", (token) =>
        router.push(`/game/${token}`),
    );

    useEffect(() => {
        const isAuthed = Cookies.get(constants.COOKIES.IS_AUTHED);
        setShowPoolToggle(isAuthed !== undefined);
    }, []);

    async function handleSeekMatch(baseMinutes: number, increment: number) {
        const isRated = poolToggleRef.current?.isRated ?? false;
        console.log(isRated);
        if (isRated) {
            await sendMatchmakingEvent(
                "SeekRatedAsync",
                baseMinutes,
                increment,
            );
        } else {
            await sendMatchmakingEvent(
                "SeekCasualAsync",
                baseMinutes,
                increment,
            );
        }
    }

    return (
        <Card
            data-testid="playOptions"
            className="flex h-full w-full min-w-xs flex-col items-center overflow-auto pt-10
                lg:max-w-md"
        >
            <h1 className="text-5xl">Play Chess 2</h1>

            {/* spacer */}
            <div className="h-10" />

            {showPoolToggle && <PoolToggle ref={poolToggleRef} />}
            <div className="grid w-full grid-cols-3 gap-x-3 gap-y-7">
                {constants.TIME_CONTROLS.map((timeControl) => {
                    const formattedTimeControl = `${timeControl.baseMinutes} + ${timeControl.increment}`;
                    return (
                        <PlayButton
                            key={formattedTimeControl}
                            baseMinutes={timeControl.baseMinutes}
                            increment={timeControl.increment}
                            formattedTimeControl={formattedTimeControl}
                            isMostPopular={timeControl.isMostPopular}
                            type={timeControl.type}
                            onClick={handleSeekMatch}
                        />
                    );
                })}
            </div>
        </Card>
    );
};
export default PlayOptions;

const PlayButton = ({
    baseMinutes,
    increment,
    formattedTimeControl,
    type,
    isMostPopular,
    onClick,
}: {
    baseMinutes: number;
    increment: number;
    formattedTimeControl: string;
    type: string;
    isMostPopular?: boolean;
    onClick?: (baseMinutes: number, increment: number) => void;
}) => {
    return (
        <div className="relative">
            {isMostPopular && (
                <span className="absolute -top-5 left-1/2 -translate-x-1/2 transform text-sm text-nowrap">
                    Most Popular
                </span>
            )}
            <Button
                onClick={() => onClick?.(baseMinutes, increment)}
                className={clsx(
                    "flex h-full w-full flex-col items-center justify-center rounded-sm",
                    isMostPopular && "border border-amber-300",
                )}
            >
                <span className="text-[1.6rem] text-nowrap">
                    {formattedTimeControl}
                </span>
                <span className="text-[1rem] text-nowrap">{type}</span>
            </Button>
        </div>
    );
};
