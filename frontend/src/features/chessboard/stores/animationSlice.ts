import { StateCreator } from "zustand";
import type { ChessboardStore } from "./chessboardStore";
import { Piece, PieceID, PieceMap } from "../lib/types";
import { LogicalPoint } from "@/features/point/types";

export interface AnimationSlice {
    animatingPieceMap: PieceMap | null;
    animatingPieces: Set<PieceID>;

    setAnimatingPieceMap(
        map: PieceMap | null,
        movedPieceIds: Set<PieceID>,
    ): Promise<void>;
    animatePiece(
        pieceId: PieceID,
        piece: Piece,
        newPosition: LogicalPoint,
    ): Promise<void>;
    clearAnimation(): void;
    addAnimatingPiece(pieceId: PieceID): Promise<void>;
}

export const createAnimationSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    AnimationSlice
> = (set, get) => ({
    animatingPieceMap: null,
    animatingPieces: new Set(),

    async setAnimatingPieceMap(map, movedPieceIds) {
        const { addAnimatingPiece } = get();

        const animatingPromises = movedPieceIds.values().map(addAnimatingPiece);
        set((state) => {
            state.animatingPieceMap = map;
        });
        await Promise.all(animatingPromises);
    },

    async animatePiece(pieceId, piece, newPosition) {
        const { addAnimatingPiece } = get();

        set((state) => {
            state.animatingPieceMap ??= new Map();
            state.animatingPieceMap.set(pieceId, {
                ...piece,
                position: newPosition,
            });
        });
        await addAnimatingPiece(pieceId);
    },

    clearAnimation() {
        set((state) => {
            state.animatingPieceMap = null;
        });
    },

    addAnimatingPiece(pieceId) {
        set((state) => {
            if (!state.animatingPieces.has(pieceId))
                state.animatingPieces.add(pieceId);
        });

        return new Promise((resolve) =>
            setTimeout(() => {
                set((state) => {
                    state.animatingPieces.delete(pieceId);
                });
                resolve();
            }, 100),
        );
    },
});
