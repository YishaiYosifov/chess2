import clsx from "clsx";
import { useState } from "react";

const PoolToggle = () => {
    const [enabled, setIsRated] = useState(false);

    return (
        <div className="grid w-full grid-rows-2 justify-between">
            <button
                onClick={() => setIsRated((prev) => !prev)}
                className="from-primary to-secondary/60 relative col-span-2 h-8 w-full cursor-pointer
                    rounded-sm bg-gradient-to-r p-1"
            >
                <div
                    className={clsx(
                        "bg-secondary absolute top-1 h-6 w-10 rounded-sm transition-all",
                        enabled ? "left-[calc(100%-2.75rem)]" : "left-1",
                    )}
                ></div>
            </button>
            <span className="text-[0.9rem]">Casual (faster match)</span>
            <span className="text-[0.9rem]">Rated (longer wait)</span>
        </div>
    );
};
export default PoolToggle;
