import { StateCreator } from "zustand";

import type { ChessboardStore } from "./chessboardStore";
import { LogicalPoint } from "@/features/point/types";
import { Piece } from "../lib/types";
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
    ChessboardStore,
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
