import { useContext } from "react";
import { useStore } from "zustand";

import { ChessStoreContext } from "@/contexts/chessStoreContext";
import { type ChessboardStore } from "@/stores/chessboardStore";
import { PieceID } from "@/lib/apiClient/models";

export function useChessStore<T>(selector: (store: ChessboardStore) => T): T {
    const chessStoreContext = useContext(ChessStoreContext);

    if (!chessStoreContext)
        throw new Error("useChessStore must be use within ChessProvider");

    return useStore(chessStoreContext, selector);
}

export const usePieces = () => useChessStore((state) => state.pieces);
export const useHighlightedLegalMoves = () =>
    useChessStore((state) => state.highlightedLegalMoves);

export const usePiece = (pieceId: PieceID) =>
    useChessStore((state) => state.pieces.get(pieceId));
