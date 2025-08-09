import Card from "@/components/ui/Card";
import ChallengeItem from "./ChallengeItem";

const OpenChallenges = () => {
    return (
        <Card className="h-full flex-col gap-5 overflow-auto">
            <h2 className="text-3xl">Open Challenges</h2>

            <div className="flex h-full flex-col gap-3 overflow-auto">
                <ChallengeItem />
            </div>
        </Card>
    );
};
export default OpenChallenges;
