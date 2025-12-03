import clsx from "clsx";
import Button from "@/components/ui/Button";
import { PoolType, TimeControlSettings } from "@/lib/apiClient";
import useMatchmaking from "@/features/lobby/hooks/useMatchmaking";

const PoolButton = ({
    timeControl,
    poolType,
    label,
    isMostPopular,
}: {
    timeControl: TimeControlSettings;
    poolType: PoolType;
    label: string;
    isMostPopular?: boolean;
}) => {
    const formattedTimeControl = `${timeControl.baseSeconds / 60} + ${timeControl.incrementSeconds}`;

    const { isSeeking, toggleSeek } = useMatchmaking({
        poolType,
        timeControl,
    });

    return (
        <div className={clsx("relative", isSeeking && "animate-subtle-ping")}>
            {isMostPopular && (
                <span className="absolute -top-5 left-1/2 -translate-x-1/2 transform text-sm text-nowrap">
                    Most Popular
                </span>
            )}
            <Button
                onClick={toggleSeek}
                className={clsx(
                    "flex h-full w-full flex-col items-center justify-center rounded-sm",
                    isMostPopular && "border-accent border-3",
                )}
            >
                <span className="text-[1.6rem] text-nowrap">
                    {formattedTimeControl}
                </span>
                {isSeeking ? (
                    <span className="text-[0.85rem] text-nowrap">
                        searching...
                    </span>
                ) : (
                    <span className="text-[1rem] text-nowrap">{label}</span>
                )}
            </Button>
        </div>
    );
};
export default PoolButton;
