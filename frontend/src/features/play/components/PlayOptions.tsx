"use client";

import { useEffect, useRef, useState } from "react";
import Cookies from "js-cookie";

import ChallengePopup from "@/features/challenges/components/ChallengePopup";

import useLocalPref from "@/hooks/useLocalPref";
import Button from "@/components/ui/Button";
import { PoolType } from "@/lib/apiClient";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";
import PoolToggle from "./PoolToggle";
import PoolButton from "./PoolButton";
import { PopupRef } from "@/components/Popup";

const PlayOptions = () => {
    const [showPoolToggle, setShowPoolToggle] = useState(false);
    const [poolType, setPoolType] = useLocalPref(
        constants.LOCALSTORAGE.PREFERS_MATCHMAKING_POOL,
        PoolType.CASUAL,
    );
    const isRated = poolType === PoolType.RATED;

    const challengePopupRef = useRef<PopupRef>(null);

    useEffect(() => {
        const isLoggedIn = Cookies.get(constants.COOKIES.IS_LOGGED_IN);
        setShowPoolToggle(isLoggedIn !== undefined);
    }, []);

    return (
        <Card data-testid="playOptions" className="items-center gap-7 pt-10">
            <h1 className="text-5xl">Play Chess 2</h1>

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

            <Button
                className="w-full"
                onClick={() => challengePopupRef.current?.open()}
                data-testid="playOptionsChallengeFriend"
            >
                Challenge a friend
            </Button>
            <ChallengePopup ref={challengePopupRef} />
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
