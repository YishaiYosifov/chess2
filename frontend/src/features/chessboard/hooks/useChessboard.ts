import { useContext } from "react";
import { useStore } from "zustand";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { type ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { useShallow } from "zustand/shallow";

export function useChessboardStore<T>(
    selector: (store: ChessboardStore) => T,
): T {
    const chessStoreContext = useContext(ChessboardStoreContext);

    if (!chessStoreContext)
        throw new Error("useChessStore must be use within ChessboardProvider");

    return useStore(chessStoreContext, useShallow(selector));
}
