import { useContext } from "react";
import { useStore } from "zustand";

import { ChessboardStoreContext } from "@/features/chessboard/contexts/chessboardStoreContext";
import { type ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { PieceID } from "@/types/tempModels";

export function useChessboardStore<T>(
    selector: (store: ChessboardStore) => T,
): T {
    const chessStoreContext = useContext(ChessboardStoreContext);

    if (!chessStoreContext)
        throw new Error("useChessStore must be use within ChessboardProvider");

    return useStore(chessStoreContext, selector);
}

export const usePieces = () => useChessboardStore((state) => state.pieces);
export const useHighlightedLegalMoves = () =>
    useChessboardStore((state) => state.highlightedLegalMoves);

export const usePiece = (pieceId: PieceID) =>
    useChessboardStore((state) => state.pieces.get(pieceId));
