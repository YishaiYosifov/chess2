import clsx from "clsx";
import Button from "../helpers/Button";
import { TimeControlSettings } from "@/lib/apiClient";

const TimeControlButton = ({
    timeControl,
    formattedTimeControl,
    type,
    isMostPopular,
    onClick,
    isSeeking,
}: {
    timeControl: TimeControlSettings;
    formattedTimeControl: string;
    type: string;
    isMostPopular?: boolean;
    onClick?: (timeControl: TimeControlSettings) => void;
    isSeeking?: boolean;
}) => {
    return (
        <div className={clsx("relative", isSeeking && "blur-sm")}>
            {isMostPopular && (
                <span className="absolute -top-5 left-1/2 -translate-x-1/2 transform text-sm text-nowrap">
                    Most Popular
                </span>
            )}
            <Button
                onClick={() => onClick?.(timeControl)}
                className={clsx(
                    "flex h-full w-full flex-col items-center justify-center rounded-sm",
                    isMostPopular && "border border-amber-300",
                )}
            >
                <span className="text-[1.6rem] text-nowrap">
                    {formattedTimeControl}
                </span>
                <span className="text-[1rem] text-nowrap">{type}</span>
            </Button>
        </div>
    );
};
export default TimeControlButton;
