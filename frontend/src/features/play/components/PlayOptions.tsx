"use client";

import { useEffect, useState } from "react";
import Cookies from "js-cookie";

import useLocalPref from "@/hooks/useLocalPref";
import { PoolType } from "@/lib/apiClient";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";
import PoolToggle from "./PoolToggle";
import PoolButton from "./PoolButton";

const PlayOptions = () => {
    const [showPoolToggle, setShowPoolToggle] = useState(false);
    const [poolType, setPoolType] = useLocalPref(
        constants.LOCALSTORAGE.PREFERS_MATCHMAKING_POOL,
        PoolType.CASUAL,
    );
    const isRated = poolType === PoolType.RATED;

    useEffect(() => {
        const isLoggedIn = Cookies.get(constants.COOKIES.IS_LOGGED_IN);
        setShowPoolToggle(isLoggedIn !== undefined);
        if (!isLoggedIn) {
            setPoolType(PoolType.CASUAL);
        }
    }, [setPoolType]);

    return (
        <Card data-testid="playOptions" className="items-center gap-7 pt-10">
            <h1 className="text-center text-5xl">Play Anarchy Chess</h1>

            <div className="flex w-full flex-col">
                {showPoolToggle && (
                    <PoolToggle
                        isRated={isRated}
                        onToggle={(isRated) =>
                            setPoolType(
                                isRated ? PoolType.RATED : PoolType.CASUAL,
                            )
                        }
                    />
                )}

                <PoolButtons hidden={isRated} poolType={PoolType.CASUAL} />
                <PoolButtons hidden={!isRated} poolType={PoolType.RATED} />
            </div>
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
                />
            ))}
        </section>
    );
};
