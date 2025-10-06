import Card from "@/components/ui/Card";
import DirectChallengeDescription from "./DirectChallengeDescription";
import OpenChallengeDescription from "./OpenChallengeDescription";
import useChallengeStore from "@/features/challenges/hooks/useChallengeStore";

const ChallengeDescription = () => {
    const challenge = useChallengeStore((x) => x.challenge);
    return (
        <Card className="flex-col items-center justify-center gap-5">
            {challenge.recipient ? (
                <DirectChallengeDescription />
            ) : (
                <OpenChallengeDescription />
            )}
        </Card>
    );
};
export default ChallengeDescription;
