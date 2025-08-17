"use client";

import { useEffect, useState } from "react";
import Cookies from "js-cookie";

import PoolToggle from "./PoolToggle";
import TimeControlButton from "./TimeControlButton";
import constants from "@/lib/constants";
import Card from "@/components/ui/Card";
import clsx from "clsx";
import { PoolType } from "@/lib/apiClient";

const PlayOptions = () => {
    const [showPoolToggle, setShowPoolToggle] = useState(false);
    const [isRated, setIsRated] = useState(false);

    useEffect(() => {
        const isLoggedIn = Cookies.get(constants.COOKIES.IS_LOGGED_IN);
        setShowPoolToggle(isLoggedIn !== undefined);

        const storedPrefersRated = localStorage.getItem(
            constants.LOCALSTORAGE.PREFERS_RATED_POOL,
        );
        setIsRated(storedPrefersRated ? JSON.parse(storedPrefersRated) : false);
    }, []);

    function toggleIsRated(toggleTo: boolean): void {
        setIsRated(toggleTo);
        localStorage.setItem(
            constants.LOCALSTORAGE.PREFERS_RATED_POOL,
            JSON.stringify(toggleTo),
        );
    }

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
        <Card data-testid="playOptions" className="flex-col items-center pt-10">
            <h1 className="text-5xl">Play Chess 2</h1>

            {/* spacer */}
            <div className="h-10" />

            {showPoolToggle && (
                <PoolToggle isRated={isRated} onToggle={toggleIsRated} />
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
