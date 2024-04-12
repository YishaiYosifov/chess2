import {
    HTMLAttributes,
    ReactNode,
    forwardRef,
    ForwardRefRenderFunction,
} from "react";

import styles from "./ChessSquare.module.scss";
import { Color, Point } from "./chess.types";
import { useBoardSize, useChessStore } from "@/hooks/useChess";

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
> = ({ position, children, ...divProps }, ref) => {
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

    // allow custom className
    if (divProps.className) divProps.className += " " + styles["chess-square"];
    else divProps.className = styles["chess-square"];

    divProps.style = {
        ...divProps.style,
        transform: `translate(${physicalX}%, ${physicalY}%)`,
    };

    return (
        <div {...divProps} ref={ref}>
            {children}
        </div>
    );
};
export default forwardRef(ChessSquare);
