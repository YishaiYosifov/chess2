import React, {
    useEffect,
    useLayoutEffect,
    useMemo,
    useRef,
    useState,
} from "react";
import PieceRenderer from "./PieceRenderer";
import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { twMerge } from "tailwind-merge";
import constants from "@/lib/constants";
import OverlayPainter from "./OverlayPainter";

export interface PaddingOffset {
    width: number;
    height: number;
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
    const setBoardRect = useChessboardStore((state) => state.setBoardRect);
    const onPointerDown = useChessboardStore((state) => state.onPointerDown);
    const onPointerUp = useChessboardStore((state) => state.onPointerUp);

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

            const minSize = Math.max(
                constants.MIN_BOARD_SIZE_PX,
                Math.min(width, height),
            );
            setBoardSize(minSize);
        }

        window.addEventListener("resize", resizeBoard);
        resizeBoard();

        return () => window.removeEventListener("resize", resizeBoard);
    }, [defaultOffset, sortedBreakpoints]);

    useLayoutEffect(() => {
        if (ref.current) {
            setBoardRect(ref.current.getBoundingClientRect());
        }
    }, [boardSize, setBoardRect]);

    return (
        <div
            data-testid="chessboard"
            className={twMerge(
                `grid-template-rows-10 relative grid cursor-pointer touch-none grid-cols-10
                rounded-md border-2 border-blue-400 bg-[url(/assets/board.svg)] bg-[length:100%]
                bg-no-repeat`,
                className,
            )}
            style={{
                width: `${boardSize}px`,
                height: `${boardSize}px`,
            }}
            ref={ref}
            onPointerDown={onPointerDown}
            onPointerUp={onPointerUp}
            onContextMenu={(e) => e.preventDefault()}
        >
            {/* <OverlayPainter /> */}
            <PieceRenderer />
        </div>
    );
};
export default ChessboardLayout;
