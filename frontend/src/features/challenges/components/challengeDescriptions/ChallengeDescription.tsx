import Card from "@/components/ui/Card";
import useChallengeStore from "../../hooks/useChallengeStore";
import DirectChallengeDescription from "./DirectChallengeDescription";
import OpenChallengeDescription from "./OpenChallengeDescription";

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
