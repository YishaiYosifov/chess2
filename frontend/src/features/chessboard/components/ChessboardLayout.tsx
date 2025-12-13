import { useEffect, useLayoutEffect, useMemo, useRef, useState } from "react";

import { twMerge } from "tailwind-merge";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import LastMoveHighlight from "./LastMoveHighlight";
import OverlayRenderer from "./OverlayRenderer";
import PieceRenderer from "./PieceRenderer";
import Coords from "./Coords";

export interface PaddingOffset {
    width: number;
    height: number;
    maxSize?: number;
}

export interface ChessboardBreakpoint {
    maxScreenSize: number;
    paddingOffset: PaddingOffset;
}

export interface ChessboardLayoutProps {
    breakpoints?: ChessboardBreakpoint[];
    defaultOffset?: PaddingOffset;
    className?: string;
}

const ChessboardLayout = ({
    breakpoints = [],
    defaultOffset,
    className,
}: ChessboardLayoutProps) => {
    const [boardSize, setBoardSize] = useState<number>(0);
    const { setBoardRect, onPointerDown, onPointerUp } = useChessboardStore(
        (x) => ({
            setBoardRect: x.setBoardRect,
            onPointerDown: x.onPointerDown,
            onPointerUp: x.onPointerUp,
        }),
    );
    const boardDimensions = useChessboardStore((x) => x.boardDimensions);

    const ref = useRef<HTMLDivElement>(null);

    // Sort the offset breakpoints in ascending order
    const sortedBreakpoints = useMemo(
        () => breakpoints.sort((a, b) => a.maxScreenSize - b.maxScreenSize),
        [breakpoints],
    );

    useEffect(() => {
        /**
         * Calculate the width and height offset based on the offsetBreakpoints param and window width
         */
        function calculateOffset(): PaddingOffset {
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
            const {
                width: offsetWidth,
                height: offsetHeight,
                maxSize,
            } = calculateOffset();

            const width = window.innerWidth - offsetWidth;
            const height = window.innerHeight - offsetHeight;

            let minSize = Math.max(264, Math.min(width, height));
            if (maxSize !== undefined) minSize = Math.min(minSize, maxSize);
            setBoardSize(minSize);
        }

        window.addEventListener("resize", resizeBoard);
        resizeBoard();

        return () => window.removeEventListener("resize", resizeBoard);
    }, [defaultOffset, sortedBreakpoints]);

    useLayoutEffect(() => {
        let timeoutId: NodeJS.Timeout;
        function updateRect() {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(() => {
                if (ref.current)
                    setBoardRect(ref.current.getBoundingClientRect());
            }, 100);
        }
        if (ref.current) setBoardRect(ref.current.getBoundingClientRect());

        window.addEventListener("scroll", updateRect);
        window.addEventListener("resize", updateRect);

        return () => {
            window.removeEventListener("scroll", updateRect);
            window.removeEventListener("resize", updateRect);
        };
    }, [boardSize, ref, setBoardRect]);

    return (
        <div
            data-testid="chessboard"
            className={twMerge(
                "relative cursor-pointer touch-none select-none",
                className,
            )}
            style={{ width: `${boardSize}px`, height: `${boardSize}px` }}
            ref={ref}
            onPointerDown={onPointerDown}
            onPointerUp={onPointerUp}
            onContextMenu={(e) => e.preventDefault()}
        >
            <svg
                viewBox={`0 0 ${boardDimensions.width} ${boardDimensions.height}`}
                preserveAspectRatio="none"
                className="absolute inset-0 h-full w-full rounded-md"
            >
                <image
                    href="/assets/board.svg"
                    width={boardDimensions.width}
                    height={boardDimensions.height}
                    preserveAspectRatio="none"
                />
            </svg>

            <LastMoveHighlight />
            <OverlayRenderer />
            <PieceRenderer />
            <Coords />
        </div>
    );
};
export default ChessboardLayout;
