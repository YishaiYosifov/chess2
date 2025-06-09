import {
    HTMLAttributes,
    ReactNode,
    forwardRef,
    ForwardRefRenderFunction,
} from "react";
import clsx from "clsx";

import { useBoardSize, useChessStore } from "@/hooks/useChess";
import { Point } from "@/types/tempModels";
import { GameColor } from "@/lib/apiClient";

type ChessSquareProps = {
    position: Point;
    draggingOffset?: Point;
    children?: ReactNode;
} & HTMLAttributes<HTMLDivElement>;

/**
 * Render an element in a specific location on the chess board
 */
const ChessSquare: ForwardRefRenderFunction<
    HTMLDivElement,
    ChessSquareProps
> = (
    {
        position,
        draggingOffset = [0, 0],
        children,
        className,
        style,
        ...divProps
    },
    ref,
) => {
    const [boardWidth, boardHeight] = useBoardSize();
    const viewingFrom = useChessStore((state) => state.viewingFrom);

    const boardSize = boardWidth * boardHeight;
    let [x, y] = position;

    // flip the coordinates because white is starts at y 0,
    // but we want to the playing side on the bottom
    if (viewingFrom == GameColor.WHITE) {
        x = boardWidth - x - 1;
        y = boardHeight - y - 1;
    }

    const physicalX = x * boardWidth * boardHeight;
    const physicalY = y * boardSize;

    // tailwind doesn't work well with dynamic values
    style ??= {};
    style.transform = `translate(
        calc(${physicalX}% + ${draggingOffset[0]}px),
        calc(${physicalY}% + ${draggingOffset[1]}px))`;
    style.height = `${100 / boardHeight}%`;
    style.width = `${100 / boardWidth}%`;

    return (
        <div
            className={clsx(className, "absolute transform")}
            style={style}
            {...divProps}
            ref={ref}
        >
            {children}
        </div>
    );
};
export default forwardRef(ChessSquare);
