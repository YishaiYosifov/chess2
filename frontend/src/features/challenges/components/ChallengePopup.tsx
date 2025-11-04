import Popup, { PopupRef } from "@/components/Popup";
import Button from "@/components/ui/Button";
import Range from "@/components/ui/Range";
import Selector from "@/components/ui/Selector";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import { isAuthed, isGuest } from "@/features/auth/lib/userGuard";
import useLocalPref from "@/hooks/useLocalPref";
import {
    createChallenge,
    ErrorCode,
    PoolType,
    PublicUser,
} from "@/lib/apiClient";
import constants from "@/lib/constants";
import { findMatchingError as filterError } from "@/lib/utils/errorUtils";
import { useRouter } from "next/navigation";
import { forwardRef, ForwardRefRenderFunction, useState } from "react";

const ChallengePopup: ForwardRefRenderFunction<
    PopupRef,
    { recipient?: PublicUser }
> = ({ recipient }, ref) => {
    const [error, setError] = useState<string | null>(null);

    const [minutesIdx, setMinutesIdx] = useLocalPref<number>(
        constants.LOCALSTORAGE.PREFERS_TIME_CONTROL_MINUTES_IDX,
        constants.DEFAULT_CHALLENGE_MINUTE_OPTION_IDX,
    );
    const [incrementIdx, setIncrementIdx] = useLocalPref<number>(
        constants.LOCALSTORAGE.PREFERS_TIME_CONTROL_INCREMENT_IDX,
        constants.DEFAULT_CHALLENGE_INCREMENT_OPTION_IDX,
    );
    const [poolType, setPoolType] = useLocalPref(
        constants.LOCALSTORAGE.PREFERS_CHALLENGE_POOL,
        PoolType.RATED,
    );

    const user = useSessionUser();
    const router = useRouter();

    async function sendChallenge() {
        const isUserGuest = isGuest(user);
        const effectivePoolType = isUserGuest ? PoolType.CASUAL : poolType;

        const { error, data: challenge } = await createChallenge({
            query: { recipientId: recipient?.userId },
            body: {
                poolType: effectivePoolType,
                timeControl: {
                    baseSeconds:
                        constants.CHALLENGE_MINUTES_OPTIONS[minutesIdx] * 60,
                    incrementSeconds:
                        constants.CHALLENGE_INCREMENT_SECONDS_OPTIONS[
                            incrementIdx
                        ],
                },
            },
        });
        if (error || challenge === undefined) {
            setError(
                filterError(
                    error,
                    new Set([
                        ErrorCode.CHALLENGE_RECIPIENT_NOT_ACCEPTING,
                        ErrorCode.CHALLENGE_ALREADY_EXISTS,
                    ]),
                    "Something went wrong",
                ),
            );
            console.error(error);
            return;
        }
        setError(null);

        router.push(`${constants.PATHS.CHALLENGE}/${challenge.challengeToken}`);
    }

    return (
        <Popup className="bg-card gap-8" data-testid="challengePopup" ref={ref}>
            <h1 className="text-center text-3xl font-bold">Create Challenge</h1>

            <div className="flex flex-col gap-3">
                <div>
                    <p data-testid="challengePopupMinutesText">
                        Minutes per side:{" "}
                        {constants.CHALLENGE_MINUTES_OPTIONS[minutesIdx]}
                    </p>
                    <Range
                        min={0}
                        max={constants.CHALLENGE_MINUTES_OPTIONS.length - 1}
                        value={minutesIdx}
                        onChange={(e) =>
                            setMinutesIdx(parseInt(e.target.value))
                        }
                        data-testid="challengePopupMinutes"
                    />
                </div>
                <div>
                    <p data-testid="challengePopupIncrementText">
                        Increment in seconds:{" "}
                        {
                            constants.CHALLENGE_INCREMENT_SECONDS_OPTIONS[
                                incrementIdx
                            ]
                        }
                    </p>
                    <Range
                        min={0}
                        max={
                            constants.CHALLENGE_INCREMENT_SECONDS_OPTIONS
                                .length - 1
                        }
                        value={incrementIdx}
                        onChange={(e) =>
                            setIncrementIdx(parseInt(e.target.value))
                        }
                        data-testid="challengePopupIncrement"
                    />
                </div>

                {isAuthed(user) && (
                    <Selector
                        options={[
                            { label: "Rated", value: PoolType.RATED },
                            { label: "Casual", value: PoolType.CASUAL },
                        ]}
                        value={poolType}
                        onChange={(e) => setPoolType(e.target.value)}
                        data-testid="challengePopupPoolType"
                    />
                )}
            </div>

            <div>
                <Button
                    className="border-secondary flex w-full items-center justify-center gap-1 border-4 text-xl"
                    onClick={sendChallenge}
                    data-testid="challengePopupCreate"
                >
                    Challenge {recipient?.userName ?? "a friend"}
                </Button>
                {error && (
                    <span
                        className="text-error"
                        data-testid="challengePopupError"
                    >
                        {error}
                    </span>
                )}
            </div>
        </Popup>
    );
};
export default forwardRef(ChallengePopup);
