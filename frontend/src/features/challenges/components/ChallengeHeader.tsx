import Card from "@/components/ui/Card";
import TimeControlIcon from "@/features/lobby/Components/TimeControlIcon";
import { PoolType } from "@/lib/apiClient";
import useChallengeStore from "../hooks/useChallengeStore";

const ChallengeHeader = () => {
    const challenge = useChallengeStore((x) => x.challenge);
    const isRated = challenge.pool.poolType == PoolType.RATED;

    return (
        <Card className="items-center">
            <h1
                data-testid="challengeHeaderTitle"
                className="text-4xl font-bold"
            >
                Challenge
            </h1>
            <div className="flex items-center gap-2">
                <TimeControlIcon
                    timeControl={challenge.timeControl}
                    className="h-8 w-8"
                    data-testid="challengeHeaderTimeControlIcon"
                />
                <p
                    data-testid="challengeHeaderTimeControl"
                    className="text-2xl font-semibold"
                >
                    {challenge.pool.timeControl.baseSeconds / 60}+
                    {challenge.pool.timeControl.incrementSeconds}{" "}
                    {isRated ? "Rated" : "Casual"}
                </p>
            </div>
        </Card>
    );
};
export default ChallengeHeader;
