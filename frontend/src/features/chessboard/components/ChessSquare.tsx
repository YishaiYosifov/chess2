import {
    HTMLAttributes,
    ReactNode,
    forwardRef,
    ForwardRefRenderFunction,
    memo,
} from "react";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { LogicalPoint } from "@/features/point/types";
import CoordSquare, { ChessSquareRef } from "./CoordSquare";
import { pointToStr } from "@/features/point/pointUtils";

type ChessSquareProps = {
    position: LogicalPoint;
    children?: ReactNode;
} & HTMLAttributes<HTMLDivElement>;

/**
 * Render an element in a specific location on the chess board, respecting view position
 */
const ChessSquare: ForwardRefRenderFunction<
    ChessSquareRef,
    ChessSquareProps
> = ({ position, ...props }, ref) => {
    const { logicalPointToViewPoint } = useChessboardStore((x) => ({
        logicalPointToViewPoint: x.logicalPointToViewPoint,
        boardWidth: x.boardDimensions.width,
        boardHeight: x.boardDimensions.height,
    }));
    const viewPosition = logicalPointToViewPoint(position);

    return (
        <CoordSquare
            data-position={pointToStr(position)}
            data-testid="chessSquare"
            position={viewPosition}
            {...props}
            ref={ref}
        />
    );
};
export default memo(forwardRef(ChessSquare));
