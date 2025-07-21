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

    screenPointToBoardPoint(screenPoint: Point): Point | null;
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

        screenPointToBoardPoint(screenPoint: Point): Point | null {
            const { boardDimensions, boardRect } = get();
            if (!boardRect) {
                console.warn("Cannot move piece, board rect not set yet");
                return null;
            }

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

        setBoardRect: (rect) =>
            set((state) => {
                state.boardRect = rect;
            }),
    });
}
