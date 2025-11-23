import clsx from "clsx";

const PoolToggle = ({
    isRated,
    onToggle,
}: {
    isRated: boolean;
    onToggle: (value: boolean) => void;
}) => {
    return (
        <div className="grid w-full grid-rows-2 justify-between">
            <button
                data-testid="poolToggle"
                onClick={() => onToggle(!isRated)}
                className="from-primary via-primary/50 to-primary relative col-span-2 h-8 w-full
                    cursor-pointer rounded-sm bg-gradient-to-r"
            >
                <div
                    className={clsx(
                        "bg-secondary absolute top-1 h-6 w-10 rounded-sm shadow-2xl transition-all",
                        isRated ? "left-[calc(100%-2.75rem)]" : "left-1",
                    )}
                />
            </button>
            <span className="text-[0.9rem]">Casual (faster match)</span>
            <span className="text-[0.9rem]">Rated (longer wait)</span>
        </div>
    );
};
export default PoolToggle;
