"use client";

import { useEffect, useRef, useState } from "react";
import Cookies from "js-cookie";

import PoolToggle, { PoolToggleRef } from "./PoolToggle";
import TimeControlButton from "./TimeControlButton";
import constants from "@/lib/constants";
import Card from "@/components/ui/Card";
import SeekingOverlay from "./SeekingOverlay";
import { TimeControlSettings } from "@/lib/apiClient";
import useMatchmaking from "@/features/lobby/hooks/useMatchmaking";

/**
 * Card containing the variant and time control options.
 * When the one of the time control buttons is clicked, a request to enter the pool will be sent.
 */
const PlayOptions = () => {
    const [showPoolToggle, setShowPoolToggle] = useState(false);
    const poolToggleRef = useRef<PoolToggleRef>(null);

    const { createSeek, cancelSeek, isSeeking } = useMatchmaking();
    useEffect(() => {
        const isAuthed = Cookies.get(constants.COOKIES.IS_AUTHED);
        setShowPoolToggle(isAuthed !== undefined);
    }, []);

    async function handleSeekMatch(timeControl: TimeControlSettings) {
        const isRated = poolToggleRef.current?.isRated ?? false;
        await createSeek(isRated, timeControl);
    }

    return (
        <Card
            data-testid="playOptions"
            className="flex-col items-center overflow-auto pt-10"
        >
            <h1 className="text-5xl">Play Chess 2</h1>

            {/* spacer */}
            <div className="h-10" />

            {showPoolToggle && <PoolToggle ref={poolToggleRef} />}
            <section className="relative grid w-full grid-cols-3 gap-x-3 gap-y-7">
                {isSeeking && <SeekingOverlay onClick={cancelSeek} />}

                {constants.STANDARD_TIME_CONTROLS.map((timeControl) => {
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
