import { FireIcon } from "@heroicons/react/24/outline";

import TimeControlIcon from "@/features/lobby/Components/TimeControlIcon";
import { TimeControl } from "@/lib/apiClient";

const ChallengeItem = () => {
    return (
        <div
            className="hover:bg-primary flex cursor-pointer items-center gap-5 rounded-lg p-3
                duration-200"
        >
            <div className="flex flex-col items-center">
                <TimeControlIcon
                    className="h-10 w-10"
                    timeControl={TimeControl.BULLET}
                />
                <span className="text-2xl">1+0</span>
            </div>

            <div className="flex-1">
                <p className="text-xl">username</p>
                <p className="text-text/50 text-sm">161660 - rated</p>
            </div>

            <FireIcon className="h-8 w-8" />
        </div>
    );
};
export default ChallengeItem;
