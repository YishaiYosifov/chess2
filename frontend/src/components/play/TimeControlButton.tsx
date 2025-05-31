import clsx from "clsx";
import Button from "../helpers/Button";

const TimeControlButton = ({
    baseMinutes,
    increment,
    formattedTimeControl,
    type,
    isMostPopular,
    onClick,
    isSeeking,
}: {
    baseMinutes: number;
    increment: number;
    formattedTimeControl: string;
    type: string;
    isMostPopular?: boolean;
    onClick?: (baseMinutes: number, increment: number) => void;
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
                onClick={() => onClick?.(baseMinutes, increment)}
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
