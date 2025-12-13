import { useRef, useState } from "react";
import clsx from "clsx";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { ArrowsUpDownIcon, ShareIcon } from "@heroicons/react/24/solid";

const GameActions = () => {
    const flipBoard = useChessboardStore((x) => x.flipBoard);
    const [showCopiedTooltip, setShowCopiedTooltip] = useState(false);
    const shareTooltipTimeout = useRef<NodeJS.Timeout>(null);

    async function share() {
        await navigator.clipboard.writeText(window.location.href);

        setShowCopiedTooltip(true);

        if (shareTooltipTimeout.current)
            clearTimeout(shareTooltipTimeout.current);
        shareTooltipTimeout.current = setTimeout(
            () => setShowCopiedTooltip(false),
            1500,
        );
    }

    return (
        <div
            className="absolute right-0 bottom-0 flex w-fit gap-3 p-3"
            data-testid="gameActions"
        >
            <div className="relative">
                <ShareIcon
                    className="text-secondary h-6 w-6 cursor-pointer"
                    title="Share"
                    onClick={share}
                />

                <span
                    className={clsx(
                        `absolute bottom-full left-1/2 mb-2 -translate-x-1/2
                        rounded bg-black p-1 text-xs whitespace-nowrap
                        transition-opacity duration-100`,
                        showCopiedTooltip ? "opacity-100" : "opacity-0",
                    )}
                >
                    Copied to Clipboard
                </span>
            </div>

            <ArrowsUpDownIcon
                className="text-secondary h-6 w-6 cursor-pointer"
                title="Flip Board"
                onClick={flipBoard}
            />
        </div>
    );
};
export default GameActions;
