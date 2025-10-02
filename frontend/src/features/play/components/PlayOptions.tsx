"use client";

import { useEffect, useState } from "react";
import Cookies from "js-cookie";

import PoolToggle from "./PoolToggle";
import PoolButton from "./PoolButton";
import constants from "@/lib/constants";
import Card from "@/components/ui/Card";
import { PoolType } from "@/lib/apiClient";
import useLocalPref from "@/hooks/useLocalPref";

const PlayOptions = () => {
    const [showPoolToggle, setShowPoolToggle] = useState(false);
    const [poolType, setPoolType] = useLocalPref(
        constants.LOCALSTORAGE.PREFERS_POOL,
        PoolType.RATED,
    );
    const isRated = poolType === PoolType.RATED;

    useEffect(() => {
        const isLoggedIn = Cookies.get(constants.COOKIES.IS_LOGGED_IN);
        setShowPoolToggle(isLoggedIn !== undefined);
    }, []);

    return (
        <Card data-testid="playOptions" className="items-center gap-0 pt-10">
            <h1 className="text-5xl">Play Chess 2</h1>

            {/* spacer */}
            <div className="h-10" />

            {showPoolToggle && (
                <PoolToggle
                    isRated={isRated}
                    onToggle={(isRated) =>
                        setPoolType(isRated ? PoolType.RATED : PoolType.CASUAL)
                    }
                />
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
