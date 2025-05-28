import clsx from "clsx";
import { useState } from "react";

const PoolToggle = () => {
    const [enabled, setEnabled] = useState(false);

    return (
        <div className="grid w-full grid-rows-2 justify-between">
            <button
                onClick={() => setEnabled((prev) => !prev)}
                className="from-primary via-primary/60 to-primary relative col-span-2 h-8 w-full rounded-sm
                    bg-gradient-to-r p-1"
            >
                <div
                    className={clsx(
                        "bg-secondary absolute top-1 h-6 w-6 rounded-sm shadow-2xl transition-all",
                        enabled ? "left-[calc(100%-1.75rem)]" : "left-1",
                    )}
                ></div>
            </button>
            <span className="text-sm">Rated (longer wait)</span>
            <span className="text-sm">Casual (faster match)</span>
        </div>
    );
};
export default PoolToggle;
