"use client";

import { useEffect, useState } from "react";
import Cookies from "js-cookie";

import PoolToggle from "./PoolToggle";
import TimeControlButton from "./TimeControlButton";
import constants from "@/lib/constants";
import Card from "@/components/ui/Card";
import clsx from "clsx";
import { PoolType } from "@/lib/apiClient";

/**
 * Card containing the variant and time control options.
 * When the one of the time control buttons is clicked, a request to enter the pool will be sent.
 */
const PlayOptions = () => {
    const [showPoolToggle, setShowPoolToggle] = useState(false);
    const [isRated, setIsRated] = useState(false);

    useEffect(() => {
        const isAuthed = Cookies.get(constants.COOKIES.IS_AUTHED);
        setShowPoolToggle(isAuthed !== undefined);
    }, []);

    const renderButtons = (poolType: PoolType) =>
        constants.STANDARD_TIME_CONTROLS.map((timeControl, i) => {
            return (
                <TimeControlButton
                    key={i}
                    timeControl={timeControl.settings}
                    poolType={poolType}
                    isMostPopular={timeControl.isMostPopular}
                    label={timeControl.label}
                />
            );
        });

    return (
        <Card
            data-testid="playOptions"
            className="flex-col items-center overflow-auto pt-10"
        >
            <h1 className="text-5xl">Play Chess 2</h1>

            {/* spacer */}
            <div className="h-10" />

            {showPoolToggle && (
                <PoolToggle isRated={isRated} onToggle={setIsRated} />
            )}
            <section
                className={clsx(
                    "relative grid w-full grid-cols-3 gap-x-3 gap-y-7",
                    isRated && "hidden",
                )}
            >
                {renderButtons(PoolType.CASUAL)}
            </section>

            <section
                className={clsx(
                    "relative grid w-full grid-cols-3 gap-x-3 gap-y-7",
                    !isRated && "hidden",
                )}
            >
                {renderButtons(PoolType.RATED)}
            </section>
        </Card>
    );
};
export default PlayOptions;
