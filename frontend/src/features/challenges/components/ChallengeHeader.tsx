import TimeControlIcon from "@/features/lobby/Components/TimeControlIcon";
import { ChallengeRequest, PoolType } from "@/lib/apiClient";

const ChallengeHeader = ({ challenge }: { challenge: ChallengeRequest }) => {
    const isRated = challenge.pool.poolType == PoolType.RATED;

    return (
        <>
            <h1 className="text-4xl font-bold">Challenge</h1>
            <div className="flex items-center gap-2">
                <TimeControlIcon
                    timeControl={challenge.timeControl}
                    className="h-8 w-8"
                />
                <p className="text-2xl font-semibold">
                    {challenge.pool.timeControl.baseSeconds / 60}+
                    {challenge.pool.timeControl.incrementSeconds}{" "}
                    {isRated ? "Rated" : "Casual"}
                </p>
            </div>
        </>
    );
};
export default ChallengeHeader;
