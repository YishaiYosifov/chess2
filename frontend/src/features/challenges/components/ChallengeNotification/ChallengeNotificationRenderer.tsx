"use client";

import { EyeDropperIcon } from "@heroicons/react/24/outline";
import { useReducer, useState } from "react";

import { useChallengeEvent } from "../../hooks/useChallengeHub";
import ChallengeNotification from "./ChallengeNotification";
import { ChallengeRequest } from "@/lib/apiClient";
import Card from "@/components/ui/Card";

const MAX_CHALLENGES = 5;

type State = {
    incoming: Map<string, ChallengeRequest>;
    overflow: Map<string, ChallengeRequest>;
};

type Action =
    | { type: "add"; challenge: ChallengeRequest }
    | { type: "remove"; id: string };

function reducer(state: State, action: Action): State {
    switch (action.type) {
        case "add": {
            if (state.incoming.size >= MAX_CHALLENGES) {
                const newOverflow = new Map(state.overflow);
                newOverflow.set(action.challenge.challengeId, action.challenge);
                return {
                    ...state,
                    overflow: newOverflow,
                };
            }

            const newIncoming = new Map(state.incoming);
            newIncoming.set(action.challenge.challengeId, action.challenge);
            return { ...state, incoming: newIncoming };
        }

        case "remove": {
            const newIncoming = new Map(state.incoming);
            newIncoming.delete(action.id);

            const newOverflow = new Map(state.overflow);
            newOverflow.delete(action.id);

            const nextOverflow = newOverflow.entries().next().value;
            if (nextOverflow) {
                const [nextId, nextChallenge] = nextOverflow;
                newIncoming.set(nextId, nextChallenge);
                newOverflow.delete(nextId);
            }

            return { incoming: newIncoming, overflow: newOverflow };
        }

        default:
            return state;
    }
}

const ChallengeNotificationRenderer = () => {
    const [show, setShow] = useState(false);
    const [state, dispatch] = useReducer(reducer, {
        incoming: new Map(),
        overflow: new Map(),
    });

    useChallengeEvent("ChallengeReceivedAsync", addChallenge);
    useChallengeEvent("ChallengeCancelledAsync", (_, challengeId) =>
        removeChallenge(challengeId),
    );

    function addChallenge(challenge: ChallengeRequest) {
        dispatch({ type: "add", challenge });
        setShow((prev) => {
            if (prev) return prev;
            return state.incoming.size < MAX_CHALLENGES;
        });
    }

    function removeChallenge(challengeId: string) {
        dispatch({ type: "remove", id: challengeId });
    }
    if (state.incoming.size === 0) return null;

    const totalCount = state.incoming.size + state.overflow.size;
    const notificationCount = totalCount > 9 ? "9+" : totalCount;
    return (
        <div
            className="fixed right-10 bottom-10 z-50 flex flex-col items-end gap-y-1"
            onMouseEnter={() => setShow(true)}
            onMouseLeave={() => setShow(false)}
        >
            {show && (
                <div className="flex max-w-[calc(100vw-50px)] flex-col gap-y-1">
                    {[...state.incoming.values()]
                        .toReversed()
                        .map((challenge) => (
                            <ChallengeNotification
                                key={challenge.challengeId}
                                challenge={challenge}
                                removeChallenge={removeChallenge}
                            />
                        ))}
                </div>
            )}

            <Card className="relative w-min cursor-pointer p-2">
                <EyeDropperIcon className="h-6 w-6" />
                <span
                    className="absolute -right-1.5 -bottom-1.5 flex h-5 w-5 items-center justify-center
                        rounded-full bg-red-500 text-sm"
                    data-testid="challengeNotificationRendererCount"
                >
                    {notificationCount}
                </span>
            </Card>
        </div>
    );
};
export default ChallengeNotificationRenderer;
