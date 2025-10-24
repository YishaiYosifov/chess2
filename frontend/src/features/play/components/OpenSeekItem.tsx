import { FireIcon } from "@heroicons/react/24/outline";

import TimeControlIcon from "@/features/lobby/components/TimeControlIcon";
import { OpenSeek } from "@/features/lobby/lib/types";
import { PoolType } from "@/lib/apiClient";
import useLobbyStore from "@/features/lobby/stores/lobbyStore";
import { useLobbyEmitter } from "@/features/lobby/hooks/useLobbyHub";

const OpenSeekItem = ({ seek }: { seek: OpenSeek }) => {
    const sendLobbyEvents = useLobbyEmitter();
    const setRequestedOpenSeek = useLobbyStore((x) => x.setRequestedOpenSeek);

    async function match() {
        setRequestedOpenSeek(true);
        await sendLobbyEvents("MatchWithOpenSeekAsync", seek.userId, seek.pool);
    }

    return (
        <div
            className="hover:bg-primary flex transform cursor-pointer items-center gap-2 rounded-lg p-3"
            data-testid="openSeek"
            onClick={match}
        >
            <div className="flex w-20 flex-col items-center">
                <TimeControlIcon
                    className="h-10 w-10"
                    timeControl={seek.timeControl}
                />
                <span className="text-2xl" data-testid="openSeekTimeControl">
                    {seek.pool.timeControl.baseSeconds / 60}+
                    {seek.pool.timeControl.incrementSeconds}
                </span>
            </div>

            <div className="min-w-0 flex-1">
                <p className="truncate text-xl" data-testid="openSeekUsername">
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
