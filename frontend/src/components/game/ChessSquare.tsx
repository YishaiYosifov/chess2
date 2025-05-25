import {
    HTMLAttributes,
    ReactNode,
    forwardRef,
    ForwardRefRenderFunction,
} from "react";

import { useBoardSize, useChessStore } from "@/hooks/useChess";
import { Color, Point } from "@/types/tempModels";
import clsx from "clsx";

type ChessSquareProps = {
    position: Point;
    children?: ReactNode;
} & HTMLAttributes<HTMLDivElement>;

/**
 * Render an element in a specific location on the chess board
 */
const ChessSquare: ForwardRefRenderFunction<
    HTMLDivElement,
    ChessSquareProps
> = ({ position, children, className, style, ...divProps }, ref) => {
    const [boardWidth, boardHeight] = useBoardSize();
    const viewingFrom = useChessStore((state) => state.viewingFrom);

    const boardSize = boardWidth * boardHeight;
    let [x, y] = position;

    // flip the board if we are viewing from the black prespective
    if (viewingFrom == Color.Black) {
        x = boardWidth - x - 1;
        y = boardHeight - y - 1;
    }

    const physicalX = x * boardWidth * boardHeight;
    const physicalY = y * boardSize;

    // tailwind doesn't work well with dynamic values
    style ??= {};
    style.transform = `translate(${physicalX}%, ${physicalY}%)`;

    return (
        <div
            className={clsx(className, "absolute h-[10%] w-[10%] transform")}
            style={style}
            {...divProps}
            ref={ref}
        >
            {children}
        </div>
    );
};
export default forwardRef(ChessSquare);
