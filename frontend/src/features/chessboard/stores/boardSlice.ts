import { StateCreator } from "zustand";

import type { ChessboardState } from "./chessboardStore";
import { GameColor } from "@/lib/apiClient";

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
    return (set) => ({
        ...initState,

        setBoardRect: (rect) =>
            set((state) => {
                state.boardRect = rect;
            }),
    });
}
