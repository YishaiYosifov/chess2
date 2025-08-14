import { FireIcon } from "@heroicons/react/24/outline";

import TimeControlIcon from "@/features/lobby/Components/TimeControlIcon";
import { OpenSeek, PoolType } from "@/features/lobby/lib/types";

const OpenSeekItem = ({ seek }: { seek: OpenSeek }) => {
    return (
        <div className="hover:bg-primary flex transform cursor-pointer items-center gap-5 rounded-lg p-3">
            <div className="flex flex-col items-center">
                <TimeControlIcon
                    className="h-10 w-10"
                    timeControl={seek.timeControl}
                />
                <span className="text-2xl">
                    {seek.pool.timeControl.baseSeconds / 60}+
                    {seek.pool.timeControl.incrementSeconds}
                </span>
            </div>

            <div className="flex-1">
                <p className="text-xl">{seek.userName}</p>
                <p className="text-text/50 text-sm">
                    {seek.pool.poolType === PoolType.RATED
                        ? `rated - ${seek.rating}`
                        : "casual"}
                </p>
            </div>

            <FireIcon className="h-8 w-8" />
        </div>
    );
};

export default OpenSeekItem;
