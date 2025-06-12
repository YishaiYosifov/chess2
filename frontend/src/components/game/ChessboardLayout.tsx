import { useEffect, useMemo, useState } from "react";
import PieceRenderer from "./PieceRenderer";
import clsx from "clsx";

export interface PaddingOffset {
    width: number;
    height: number;
}

export interface ChessboardBreakpoint {
    maxScreenSize: number;
    paddingOffset: PaddingOffset;
}

const ChessboardLayout = ({
    breakpoints = [],
    defaultOffset,
    className,
}: {
    breakpoints?: ChessboardBreakpoint[];
    defaultOffset?: PaddingOffset;
    className?: string;
}) => {
    const [boardSize, setBoardSize] = useState<number>(0);

    // Sort the offset breakpoints in ascending order
    const sortedBreakpoints = useMemo(
        () => breakpoints.sort((a, b) => a.maxScreenSize - b.maxScreenSize),
        [breakpoints],
    );

    useEffect(() => {
        /**
         * Calculate the width and height offset based on the offsetBreakpoints param and window width
         */
        function calculateOffset(): { width: number; height: number } {
            const width = window.innerWidth;
            for (const { maxScreenSize, paddingOffset } of sortedBreakpoints) {
                if (maxScreenSize > width) return paddingOffset;
            }

            return (
                defaultOffset ?? {
                    width: 0,
                    height: 0,
                }
            );
        }

        /**
         * Set the board size based on the viewport size and the offset
         */
        function resizeBoard(): void {
            const { width: offsetWidth, height: offsetHeight } =
                calculateOffset();

            const width = window.innerWidth - offsetWidth;
            const height = window.innerHeight - offsetHeight;

            const minSize = Math.min(width, height);
            setBoardSize(minSize);
        }

        window.addEventListener("resize", resizeBoard);
        resizeBoard();

        return () => window.removeEventListener("resize", resizeBoard);
    }, [defaultOffset, sortedBreakpoints]);

    return (
        <div
            data-testid="chessboard"
            className={clsx(
                `grid-template-rows-10 relative grid min-h-[300px] min-w-[300px] cursor-pointer
                grid-cols-10 rounded-md border-2 border-blue-400 bg-[url(/assets/board.svg)]
                bg-[length:100%] bg-no-repeat`,
                className,
            )}
            style={{
                width: `${boardSize}px`,
                height: `${boardSize}px`,
            }}
        >
            <PieceRenderer />
        </div>
    );
};
export default ChessboardLayout;
