"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import Cookies from "js-cookie";

import {
    useMatchmakingEmitter,
    useMatchmakingEvent,
} from "@/hooks/signalR/useSignalRHubs";

import PoolToggle, { PoolToggleRef } from "./PoolToggle";
import TimeControlButton from "./TimeControlButton";
import constants from "@/lib/constants";
import Card from "../helpers/Card";
import SeekingOverlay from "./SeekingOverlay";
import { TimeControlSettings } from "@/lib/apiClient";

/**
 * Card containing the variant and time control options.
 * When the one of the time control buttons is clicked, a request to enter the pool will be sent.
 */
const PlayOptions = () => {
    const [showPoolToggle, setShowPoolToggle] = useState(false);
    const [isSeeking, setIsSeeking] = useState(false);
    const poolToggleRef = useRef<PoolToggleRef>(null);
    const router = useRouter();

    const sendMatchmakingEvent = useMatchmakingEmitter();
    useMatchmakingEvent("MatchFoundAsync", (token) =>
        router.push(`${constants.PATHS.GAME}/${token}`),
    );
    useMatchmakingEvent("MatchFailedAsync", () => setIsSeeking(false));

    useEffect(() => {
        const isAuthed = Cookies.get(constants.COOKIES.IS_AUTHED);
        setShowPoolToggle(isAuthed !== undefined);
    }, []);

    async function handleSeekMatch(timeControl: TimeControlSettings) {
        setIsSeeking(true);

        const isRated = poolToggleRef.current?.isRated ?? false;
        if (isRated) await sendMatchmakingEvent("SeekRatedAsync", timeControl);
        else await sendMatchmakingEvent("SeekCasualAsync", timeControl);
    }

    async function cancelSeek() {
        setIsSeeking(false);
        await sendMatchmakingEvent("CancelSeekAsync");
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
            <section className="relative grid w-full grid-cols-3 gap-x-3 gap-y-7">
                {isSeeking && <SeekingOverlay onClick={cancelSeek} />}

                {constants.TIME_CONTROLS.map((timeControl) => {
                    const formattedTimeControl = `${timeControl.settings.baseSeconds / 60} + ${timeControl.settings.incrementSeconds}`;
                    return (
                        <TimeControlButton
                            key={formattedTimeControl}
                            timeControl={timeControl.settings}
                            formattedTimeControl={formattedTimeControl}
                            isMostPopular={timeControl.isMostPopular}
                            type={timeControl.type}
                            onClick={handleSeekMatch}
                            isSeeking={isSeeking}
                        />
                    );
                })}
            </section>
        </Card>
    );
};
export default PlayOptions;
