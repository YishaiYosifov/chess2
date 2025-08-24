"use client";

import { useEffect, useState } from "react";
import Cookies from "js-cookie";

import PoolToggle from "./PoolToggle";
import PoolButton from "./PoolButton";
import constants from "@/lib/constants";
import Card from "@/components/ui/Card";
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

    return (
        <Card data-testid="playOptions" className="items-center gap-0 pt-10">
            <h1 className="text-5xl">Play Chess 2</h1>

            {/* spacer */}
            <div className="h-10" />

            {showPoolToggle && (
                <PoolToggle isRated={isRated} onToggle={toggleIsRated} />
            )}

            <PoolButtons hidden={isRated} poolType={PoolType.CASUAL} />
            <PoolButtons hidden={!isRated} poolType={PoolType.RATED} />
        </Card>
    );
};
export default PlayOptions;

const PoolButtons = ({
    hidden,
    poolType,
}: {
    hidden: boolean;
    poolType: PoolType;
}) => {
    return (
        <section
            className="relative grid w-full grid-cols-3 gap-x-3 gap-y-7"
            hidden={hidden}
            data-testid={`poolButtonsSection-${poolType}`}
        >
            {constants.STANDARD_TIME_CONTROLS.map((timeControl, i) => (
                <PoolButton
                    key={i}
                    timeControl={timeControl.settings}
                    poolType={poolType}
                    isMostPopular={timeControl.isMostPopular}
                    label={timeControl.label}
                />
            ))}
        </section>
    );
};
