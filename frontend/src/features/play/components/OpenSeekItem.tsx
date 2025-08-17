import { FireIcon } from "@heroicons/react/24/outline";

import TimeControlIcon from "@/features/lobby/Components/TimeControlIcon";
import { OpenSeek } from "@/features/lobby/lib/types";
import { PoolType } from "@/lib/apiClient";

const OpenSeekItem = ({ seek }: { seek: OpenSeek }) => {
    return (
        <div
            className="hover:bg-primary flex transform cursor-pointer items-center gap-5 rounded-lg p-3"
            data-testid="openSeek"
        >
            <div className="flex flex-col items-center">
                <TimeControlIcon
                    className="h-10 w-10"
                    timeControl={seek.timeControl}
                />
                <span className="text-2xl" data-testid="openSeekTimeControl">
                    {seek.pool.timeControl.baseSeconds / 60}+
                    {seek.pool.timeControl.incrementSeconds}
                </span>
            </div>

            <div className="flex-1">
                <p className="text-xl" data-testid="openSeekUsername">
                    {seek.userName}
                </p>
                <p
                    className="text-text/50 text-sm"
                    data-testid="openSeekPoolType"
                >
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
