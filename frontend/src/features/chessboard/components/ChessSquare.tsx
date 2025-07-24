import {
    HTMLAttributes,
    ReactNode,
    forwardRef,
    ForwardRefRenderFunction,
    memo,
    useImperativeHandle,
    useRef,
} from "react";
import { twMerge } from "tailwind-merge";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { LogicalPoint } from "@/types/tempModels";

type ChessSquareProps = {
    position: LogicalPoint;
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
    const { width: boardWidth, height: boardHeight } = useChessboardStore(
        (store) => store.boardDimensions,
    );

    const logicalPointToViewPoint = useChessboardStore(
        (state) => state.logicalPointToViewPoint,
    );
    const squareDivRef = useRef<HTMLDivElement>(null);

    const { x, y } = logicalPointToViewPoint(position);

    const tileWidth = 100 / boardWidth;
    const tileHeight = 100 / boardHeight;

    const physicalX = x * 100;
    const physicalY = y * 100;

    const maxX = (boardWidth - 1) * 100;
    const maxY = (boardHeight - 1) * 100;

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
            className={twMerge(
                "absolute transform will-change-transform",
                className,
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
