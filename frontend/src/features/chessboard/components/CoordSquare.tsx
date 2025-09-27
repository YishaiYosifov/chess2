import { Point, ViewPoint } from "@/features/point/types";
import {
    forwardRef,
    ForwardRefRenderFunction,
    HTMLAttributes,
    memo,
    ReactNode,
    useImperativeHandle,
    useRef,
} from "react";
import { useChessboardStore } from "../hooks/useChessboard";
import { pointToStr } from "@/features/point/pointUtils";
import { twMerge } from "tailwind-merge";

export type ChessCoordProps = {
    position: ViewPoint;
    children?: ReactNode;
} & HTMLAttributes<HTMLDivElement>;

export interface ChessSquareRef {
    updateDraggingOffset: (offset: Point) => void;
    getBoundingClientRect: () => DOMRect | null;
}

/**
 * Render an element in a specific location on the chess board
 */
const ChessSquare: ForwardRefRenderFunction<ChessSquareRef, ChessCoordProps> = (
    { position, children, className, style, ...divProps },
    ref,
) => {
    const { boardWidth, boardHeight } = useChessboardStore((x) => ({
        logicalPointToViewPoint: x.logicalPointToViewPoint,
        boardWidth: x.boardDimensions.width,
        boardHeight: x.boardDimensions.height,
    }));
    const squareDivRef = useRef<HTMLDivElement>(null);

    const { x, y } = position;
    const tileWidth = 100 / boardWidth;
    const tileHeight = 100 / boardHeight;

    const physicalX = x * 100;
    const physicalY = y * 100;

    const maxX = (boardWidth - 1) * 100;
    const maxY = (boardHeight - 1) * 100;

    function calculateTransform(offset: Point): string {
        return `translate(
            clamp(0%, calc(${physicalX}% + ${offset.x}px), ${maxX}%),
            clamp(0%, calc(${physicalY}% + ${offset.y}px), ${maxY}%))`;
    }

    useImperativeHandle(ref, () => ({
        updateDraggingOffset(offset: Point) {
            if (squareDivRef.current) {
                squareDivRef.current.style.transform =
                    calculateTransform(offset);
            }
        },
        getBoundingClientRect: () =>
            squareDivRef.current?.getBoundingClientRect() ?? null,
    }));

    return (
        <div
            data-position={pointToStr(position)}
            className={twMerge(
                "absolute transform will-change-transform",
                className,
            )}
            // tailwind doesn't work well with dynamic values
            style={{
                width: `${tileWidth}%`,
                height: `${tileHeight}%`,
                transform: calculateTransform({ x: 0, y: 0 }),
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
