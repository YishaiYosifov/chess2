import useChallengeStore from "@/features/challenges/hooks/useChallengeStore";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import RecipientChallengeView from "./RecipientChallengeView";
import DirectChallengeView from "./DirectChallengeView";
import OpenChallengeView from "./OpenChallengeView";
import Card from "@/components/ui/Card";

const ChallengeDescription = () => {
    const user = useSessionUser();

    const challenge = useChallengeStore((x) => x.challenge);
    return (
        <Card className="flex-col items-center justify-center gap-5">
            {user?.userId === challenge.requester.userId &&
                !challenge.recipient && <OpenChallengeView />}
            {user?.userId === challenge.requester.userId &&
                challenge.recipient && <DirectChallengeView />}

            {user?.userId !== challenge.requester.userId && (
                <RecipientChallengeView />
            )}
        </Card>
    );
};
export default ChallengeDescription;
