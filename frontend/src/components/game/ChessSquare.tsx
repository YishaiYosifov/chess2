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
        draggingOffset = { x: 0, y: 0 },
        children,
        className,
        style,
        ...divProps
    },
    ref,
) => {
    const [boardWidth, boardHeight] = useBoardSize();
    const viewingFrom = useChessStore((state) => state.viewingFrom);

    let { x, y } = position;

    // flip the coordinates because white is starts at y 0,
    // but we want to the playing side on the bottom
    if (viewingFrom == GameColor.WHITE) {
        x = boardWidth - x - 1;
        y = boardHeight - y - 1;
    }

    const tileWidthStepPercent = boardWidth * boardWidth;
    const tileHeightStepPercent = boardHeight * boardHeight;

    const tileWidth = 100 / boardWidth;
    const tileHeight = 100 / boardHeight;

    const maxX = (boardWidth - 1) * tileWidthStepPercent;
    const maxY = (boardHeight - 1) * tileHeightStepPercent;

    const physicalX = x * tileWidthStepPercent;
    const physicalY = y * tileHeightStepPercent;

    // tailwind doesn't work well with dynamic values
    style ??= {};
    style.transform = `translate(
        clamp(0%, calc(${physicalX}% + ${draggingOffset.x}px), ${maxX}%),
        clamp(0%, calc(${physicalY}% + ${draggingOffset.y}px), ${maxY}%))`;
    style.width = `${tileWidth}%`;
    style.height = `${tileHeight}%`;

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
