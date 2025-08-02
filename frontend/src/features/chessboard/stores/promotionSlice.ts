import { StateCreator } from "zustand";

import type { ChessboardState } from "./chessboardStore";
import { LogicalPoint, Piece } from "@/types/tempModels";
import { PieceType } from "@/lib/apiClient";

export interface PromotionRequest {
    at: LogicalPoint;
    pieces: (PieceType | null)[];
    piece: Piece;
}

export interface PromotionSlice {
    pendingPromotion: PromotionRequest | null;
    resolvePromotion: ((piece: PieceType | null) => void) | null;

    promptPromotion(promotion: PromotionRequest): Promise<PieceType | null>;
}

export const createPromotionSlice: StateCreator<
    ChessboardState,
    [["zustand/immer", never], never],
    [],
    PromotionSlice
> = (set) => ({
    pendingPromotion: null,
    resolvePromotion: null,

    async promptPromotion(promotion) {
        let resolvePromotion: (piece: PieceType | null) => void;
        const promotionPromise = new Promise<PieceType | null>((r) => {
            resolvePromotion = r;
        });

        set((state) => {
            state.pendingPromotion = promotion;
            state.resolvePromotion = resolvePromotion;
        });

        const promoteTo = await promotionPromise;
        set((state) => {
            state.pendingPromotion = null;
            state.resolvePromotion = null;
        });

        return promoteTo;
    },
});
