import Popup from "@/components/Popup";
import Button from "@/components/ui/Button";
import Range from "@/components/ui/Range";
import Selector from "@/components/ui/Selector";
import {
    createChallenge,
    ErrorCode,
    PoolType,
    PublicUser,
} from "@/lib/apiClient";
import constants from "@/lib/constants";
import { findMatchingError as filterError } from "@/lib/utils/errorUtils";
import { useRouter } from "next/navigation";
import {
    forwardRef,
    ForwardRefRenderFunction,
    useImperativeHandle,
    useState,
} from "react";

export interface ChallengePopupRef {
    open(): void;
}

const ChallengePopup: ForwardRefRenderFunction<
    ChallengePopupRef,
    { profile: PublicUser }
> = ({ profile }, ref) => {
    const [isOpen, setIsOpen] = useState(false);
    const [minutes, setMinutes] = useState(0);
    const [increment, setIncrement] = useState(0);
    const [poolType, setPoolType] = useState(PoolType.RATED);
    const [error, setError] = useState<string | null>(null);

    const router = useRouter();

    const openPopup = () => setIsOpen(true);
    const closePopup = () => setIsOpen(false);

    useImperativeHandle(ref, () => ({
        open: openPopup,
    }));

    if (!isOpen) return;

    async function sendChallenge() {
        const { error, data: challenge } = await createChallenge({
            path: { recipientId: profile.userId },
            body: {
                poolType,
                timeControl: {
                    baseSeconds: minutes * 60,
                    incrementSeconds: increment,
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
            return;
        }
        setError(null);

        router.push(`${constants.PATHS.CHALLENGE}/${challenge.challengeId}`);
    }

    return (
        <Popup
            closePopup={closePopup}
            className="bg-card gap-8"
            data-testid="challengePopup"
        >
            <h1 className="text-center text-3xl font-bold">Create Challenge</h1>

            <div className="flex flex-col gap-3">
                <div>
                    <p>
                        Minutes per side:{" "}
                        {constants.CHALLENGE_MINUTES_OPTIONS[minutes]}
                    </p>
                    <Range
                        min={0}
                        max={constants.CHALLENGE_MINUTES_OPTIONS.length - 1}
                        value={minutes}
                        onChange={(e) => setMinutes(parseInt(e.target.value))}
                    />
                </div>
                <div>
                    <p>
                        Increments in seconds:{" "}
                        {
                            constants.CHALLENGE_INCREMENT_SECONDS_OPTIONS[
                                increment
                            ]
                        }
                    </p>
                    <Range
                        min={0}
                        max={
                            constants.CHALLENGE_INCREMENT_SECONDS_OPTIONS
                                .length - 1
                        }
                        value={increment}
                        onChange={(e) => setIncrement(parseInt(e.target.value))}
                    />
                </div>

                <Selector
                    options={[
                        { label: "Rated", value: PoolType.RATED },
                        { label: "Casual", value: PoolType.CASUAL },
                    ]}
                    value={poolType}
                    onChange={(e) => setPoolType(e.target.value)}
                />
            </div>

            <div>
                <Button
                    className="border-secondary flex w-full items-center justify-center gap-1 border-4 text-xl"
                    onClick={sendChallenge}
                >
                    Challenge {profile.userName}
                </Button>
                {error && <span className="text-error">{error}</span>}
            </div>
        </Popup>
    );
};
export default forwardRef(ChallengePopup);
