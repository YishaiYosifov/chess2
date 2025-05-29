"use client";

import constants from "@/lib/constants";
import Card from "../helpers/Card";
import Button from "../helpers/Button";
import PoolToggle from "./PoolToggle";
import clsx from "clsx";
import {
    useMatchmakingEmitter,
    useMatchmakingEvent,
} from "@/hooks/signalR/useSignalRHubs";

/**
 * Card containing the variant and time control options.
 * When the one of the time control buttons is clicked, a request to enter the pool will be sent.
 */
const PlayOptions = () => {
    return (
        <Card
            data-testid="playOptions"
            className="flex h-full w-full min-w-xs flex-col items-center overflow-auto pt-10
                lg:max-w-md"
        >
            <h1 className="text-5xl">Play Chess 2</h1>

            {/* spacer */}
            <div className="h-10" />

            <PoolToggle />
            <div className="grid w-full grid-cols-3 gap-x-3 gap-y-7">
                {constants.TIME_CONTROLS.map((timeControl) => {
                    const formattedTimeControl = `${timeControl.baseMinutes} + ${timeControl.increment}`;
                    return (
                        <PlayButton
                            key={formattedTimeControl}
                            timeControl={formattedTimeControl}
                            isMostPopular={timeControl.isMostPopular}
                            type={timeControl.type}
                        />
                    );
                })}
            </div>
        </Card>
    );
};
export default PlayOptions;

type MatchmakingHubEvents = {
    TestHub: [a: string];
};

const PlayButton = ({
    timeControl,
    type,
    isMostPopular,
}: {
    timeControl: string;
    type: string;
    isMostPopular?: boolean;
}) => {
    useMatchmakingEvent("TestClient", console.log);
    const sendMatchmakingEvent = useMatchmakingEmitter();

    return (
        <div className="relative">
            {isMostPopular && (
                <span className="absolute -top-5 left-1/2 -translate-x-1/2 transform text-sm text-nowrap">
                    Most Popular
                </span>
            )}
            <Button
                onClick={() => sendMatchmakingEvent("TestHub", "hello")}
                className={clsx(
                    "flex h-full w-full flex-col items-center justify-center rounded-sm",
                    isMostPopular && "border border-amber-300",
                )}
            >
                <span className="text-[1.6rem] text-nowrap">{timeControl}</span>
                <span className="text-[1rem] text-nowrap">{type}</span>
            </Button>
        </div>
    );
};
