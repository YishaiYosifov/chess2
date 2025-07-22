import { StateCreator } from "zustand";

import type { ChessboardState } from "./chessboardStore";
import { GameColor } from "@/lib/apiClient";
import { Point } from "@/types/tempModels";

interface BoardDimensions {
    width: number;
    height: number;
}

export interface BoardSliceProps {
    viewingFrom: GameColor;
    boardDimensions: BoardDimensions;
}

export interface BoardSlice extends BoardSliceProps {
    viewingFrom: GameColor;
    boardDimensions: BoardDimensions;
    boardRect?: DOMRect;

    screenToLogicalPoint(screenPoint: Point): Point | undefined;
    screenToViewPoint(screenPoint: Point): Point | undefined;
    logicalPointToScreenPoint(logicalPoint: Point): Point | undefined;

    viewPointToLogicalPoint(viewPoint: Point): Point;
    logicalPointToViewPoint(logicalPoint: Point): Point;

    setBoardRect: (rect: DOMRect) => void;
}

export function createBoardSlice(
    initState: BoardSliceProps,
): StateCreator<
    ChessboardState,
    [["zustand/immer", never], never],
    [],
    BoardSlice
> {
    return (set, get) => ({
        ...initState,

        screenToLogicalPoint(screenPoint: Point): Point | undefined {
            const { screenToViewPoint, viewPointToLogicalPoint } = get();

            const viewPoint = screenToViewPoint(screenPoint);
            if (!viewPoint) return;

            return viewPointToLogicalPoint(viewPoint);
        },
        screenToViewPoint(screenPoint: Point): Point | undefined {
            const { boardDimensions, boardRect } = get();
            if (!boardRect) return;

            const relX = Math.max(screenPoint.x - boardRect.left, 0);
            const relY = Math.max(screenPoint.y - boardRect.top, 0);

            const x = Math.floor(
                (relX / boardRect.width) * boardDimensions.width,
            );
            const y = Math.floor(
                (relY / boardRect.height) * boardDimensions.height,
            );

            return { x, y };
        },
        logicalPointToScreenPoint(logicalPoint: Point): Point | undefined {
            const { logicalPointToViewPoint, boardDimensions, boardRect } =
                get();
            if (!boardRect) return;

            const viewPoint = logicalPointToViewPoint(logicalPoint);
            const screenX =
                boardRect.left +
                ((viewPoint.x + 0.5) / boardDimensions.width) * boardRect.width;
            const screenY =
                boardRect.top +
                ((viewPoint.y + 0.5) / boardDimensions.height) *
                    boardRect.height;

            return { x: screenX, y: screenY };
        },

        // both perform the same coordinate transformation
        // we have both for clarity
        viewPointToLogicalPoint(viewPoint: Point): Point {
            const { viewingFrom, boardDimensions } = get();
            return flipPointForPerspective(
                viewPoint,
                viewingFrom,
                boardDimensions,
            );
        },
        logicalPointToViewPoint(logicalPoint: Point): Point {
            const { viewingFrom, boardDimensions } = get();
            return flipPointForPerspective(
                logicalPoint,
                viewingFrom,
                boardDimensions,
            );
        },

        setBoardRect: (rect) =>
            set((state) => {
                state.boardRect = rect;
            }),
    });
}

function flipPointForPerspective(
    point: Point,
    viewingFrom: GameColor,
    boardDimensions: BoardDimensions,
) {
    let { x, y } = point;
    if (viewingFrom == GameColor.WHITE) {
        y = boardDimensions.height - y - 1;
    } else {
        x = boardDimensions.width - x - 1;
    }
    return { x, y };
}
