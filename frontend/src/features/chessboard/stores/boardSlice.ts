import { StateCreator } from "zustand";

import type { ChessboardState } from "./chessboardStore";
import { GameColor } from "@/lib/apiClient";
import { LogicalPoint } from "@/features/point/types";
import { ViewPoint } from "@/features/point/types";
import { ScreenPoint } from "@/features/point/types";
import { Point } from "@/features/point/types";
import { logicalPoint, viewPoint } from "@/lib/utils/pointUtils";

export interface BoardDimensions {
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

    screenToLogicalPoint(screenPoint: ScreenPoint): LogicalPoint | undefined;
    screenToViewPoint(screenPoint: ScreenPoint): ViewPoint | undefined;
    logicalPointToScreenPoint(
        logicalPoint: LogicalPoint,
    ): ScreenPoint | undefined;

    viewPointToLogicalPoint(viewPoint: ViewPoint): LogicalPoint;
    logicalPointToViewPoint(logicalPoint: LogicalPoint): ViewPoint;

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

        screenToLogicalPoint(screenPoint) {
            const { screenToViewPoint, viewPointToLogicalPoint } = get();

            const viewPoint = screenToViewPoint(screenPoint);
            if (!viewPoint) return;

            return viewPointToLogicalPoint(viewPoint);
        },
        screenToViewPoint(screenPoint) {
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

            return viewPoint({ x, y });
        },
        logicalPointToScreenPoint(logicalPoint) {
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

            return { x: screenX, y: screenY } as ScreenPoint;
        },

        // both perform the same coordinate transformation
        // we have both for clarity
        viewPointToLogicalPoint(viewPoint) {
            const { viewingFrom, boardDimensions } = get();
            return logicalPoint(
                flipPointForPerspective(
                    viewPoint,
                    viewingFrom,
                    boardDimensions,
                ),
            );
        },
        logicalPointToViewPoint(logicalPoint) {
            const { viewingFrom, boardDimensions } = get();
            return viewPoint(
                flipPointForPerspective(
                    logicalPoint,
                    viewingFrom,
                    boardDimensions,
                ),
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
