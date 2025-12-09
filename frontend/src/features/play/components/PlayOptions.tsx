"use client";

import { useEffect, useRef, useState } from "react";
import Button from "@/components/ui/Button";

import useLocalPref from "@/hooks/useLocalPref";
import { PoolType } from "@/lib/apiClient";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";
import PoolToggle from "./PoolToggle";
import PoolButton from "./PoolButton";
import ChallengePopup from "@/features/challenges/components/ChallengePopup";
import { PopupRef } from "@/components/Popup";
import useLobbyStore from "@/features/lobby/stores/lobbyStore";
import OngoingGamesPopup from "@/features/lobby/components/OngoingGamesPopup";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import { isAuthed } from "@/features/auth/lib/userGuard";

const PlayOptions = () => {
    const [showPoolToggle, setShowPoolToggle] = useState(false);
    const [poolType, setPoolType] = useLocalPref(
        constants.LOCALSTORAGE.PREFERS_MATCHMAKING_POOL,
        PoolType.CASUAL,
    );
    const user = useSessionUser();

    const isRated = poolType === PoolType.RATED;

    const challengePopupRef = useRef<PopupRef>(null);
    const ongoingGamesPopupRef = useRef<PopupRef>(null);
    const numberOfOngoingGames = useLobbyStore((x) => x.ongoingGames.size);

    useEffect(() => {
        const isLoggedIn = isAuthed(user);

        setShowPoolToggle(isLoggedIn);
        if (!isLoggedIn) {
            setPoolType(PoolType.CASUAL);
        }
    }, [setPoolType]);

    return (
        <Card data-testid="playOptions" className="items-center gap-5 pt-10">
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

            <hr className="text-secondary w-full" />

            <div className="flex w-full flex-col gap-3">
                <Button
                    className="flex flex-1 items-center justify-center gap-1"
                    onClick={() => challengePopupRef.current?.open()}
                >
                    Challenge a Friend
                </Button>
                <ChallengePopup ref={challengePopupRef} />

                {numberOfOngoingGames > 0 && (
                    <Button
                        className="flex flex-1 items-center justify-center
                            gap-1"
                        onClick={() => ongoingGamesPopupRef.current?.open()}
                    >
                        Resume Ongoing Games
                    </Button>
                )}
                <OngoingGamesPopup ref={ongoingGamesPopupRef} />
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
