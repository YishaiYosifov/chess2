"use client";

import { PuzzlePieceIcon, EyeDropperIcon } from "@heroicons/react/24/solid";
import { useRef } from "react";

import ChallengePopup from "@/features/challenges/components/ChallengePopup";
import { PopupRef } from "@/components/Popup";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import OngoingGamesPopup from "@/features/lobby/components/OngoingGamesPopup";
import useLobbyStore from "@/features/lobby/stores/lobbyStore";

const PlayActions = () => {
    const challengePopupRef = useRef<PopupRef>(null);
    const ongoingGamesPopupRef = useRef<PopupRef>(null);

    const numberOfOngoingGames = useLobbyStore((x) => x.ongoingGames.size);

    return (
        <Card>
            <Button
                className="flex items-center justify-center gap-1"
                onClick={() => challengePopupRef.current?.open()}
            >
                <EyeDropperIcon className="h-6 w-6" />
                Challenge a Friend
            </Button>
            <ChallengePopup ref={challengePopupRef} />

            {numberOfOngoingGames > 0 && (
                <Button
                    className="flex items-center justify-center gap-1"
                    onClick={() => ongoingGamesPopupRef.current?.open()}
                >
                    <PuzzlePieceIcon className="h-6 w-6" />
                    Resume Ongoing Games
                </Button>
            )}
            <OngoingGamesPopup ref={ongoingGamesPopupRef} />
        </Card>
    );
};
export default PlayActions;
