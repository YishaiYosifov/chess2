import {
    HTMLAttributes,
    ReactNode,
    forwardRef,
    ForwardRefRenderFunction,
    memo,
    useImperativeHandle,
    useRef,
} from "react";
import clsx from "clsx";

import { useBoardSize, useChessStore } from "@/hooks/useChess";
import { Point } from "@/types/tempModels";
import { GameColor } from "@/lib/apiClient";

type ChessSquareProps = {
    position: Point;
    children?: ReactNode;
} & HTMLAttributes<HTMLDivElement>;

export interface ChessSquareRef {
    updateDraggingOffset: (x: number, y: number) => void;
    getBoundingClientRect: () => DOMRect | null;
}

/**
 * Render an element in a specific location on the chess board
 */
const ChessSquare: ForwardRefRenderFunction<
    ChessSquareRef,
    ChessSquareProps
> = ({ position, children, className, style, ...divProps }, ref) => {
    const [boardWidth, boardHeight] = useBoardSize();
    const viewingFrom = useChessStore((state) => state.viewingFrom);
    const squareDivRef = useRef<HTMLDivElement>(null);

    let { x, y } = position;

    // flip the coordinates because white is starts at y 0,
    // but we want to the playing side on the bottom
    if (viewingFrom == GameColor.WHITE) {
        y = boardHeight - y - 1;
    } else {
        x = boardWidth - x - 1;
    }

    const tileWidthStepPercent = boardWidth * boardWidth;
    const tileHeightStepPercent = boardHeight * boardHeight;

    const tileWidth = 100 / boardWidth;
    const tileHeight = 100 / boardHeight;

    const physicalX = x * tileWidthStepPercent;
    const physicalY = y * tileHeightStepPercent;

    const maxX = (boardWidth - 1) * tileWidthStepPercent;
    const maxY = (boardHeight - 1) * tileHeightStepPercent;

    const calculateTransform = (offsetX: number, offsetY: number): string =>
        `translate(
            clamp(0%, calc(${physicalX}% + ${offsetX}px), ${maxX}%),
            clamp(0%, calc(${physicalY}% + ${offsetY}px), ${maxY}%))`;

    useImperativeHandle(ref, () => ({
        updateDraggingOffset(offsetX: number, offsetY: number) {
            if (squareDivRef.current)
                squareDivRef.current.style.transform = calculateTransform(
                    offsetX,
                    offsetY,
                );
        },
        getBoundingClientRect: () =>
            squareDivRef.current?.getBoundingClientRect() ?? null,
    }));

    return (
        <div
            className={clsx(
                className,
                "absolute transform will-change-transform",
            )}
            // tailwind doesn't work well with dynamic values
            style={{
                width: `${tileWidth}%`,
                height: `${tileHeight}%`,
                transform: calculateTransform(0, 0),
                ...style,
            }}
            ref={squareDivRef}
            {...divProps}
        >
            {children}
        </div>
    );
};
export default memo(forwardRef(ChessSquare));
