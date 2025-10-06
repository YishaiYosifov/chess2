import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import useChallengeStore from "../hooks/useChallengeStore";

const ChallengeStatusText = ({
    activeText,
    activeClassName,
    overClassName,
}: {
    activeText: string;
    activeClassName: string;
    overClassName: string;
}) => {
    const user = useSessionUser();
    const { isCancelled, cancelledBy, isExpired } = useChallengeStore((x) => ({
        challenge: x.challenge,
        isCancelled: x.isCancelled,
        cancelledBy: x.cancelledBy,
        isExpired: x.isExpired,
    }));

    let text: string;
    let className: string;

    if (isExpired) {
        text = "Challenge Expired";
        className = overClassName;
    } else if (isCancelled) {
        text =
            cancelledBy !== null && cancelledBy !== user?.userId
                ? "Challenge Declined"
                : "Challenge Cancelled";
        className = overClassName;
    } else {
        text = activeText;
        className = activeClassName;
    }

    return (
        <p className={className} data-testid="challengeStatusText">
            {text}
        </p>
    );
};
export default ChallengeStatusText;
