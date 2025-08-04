import { logicalPoint } from "@/lib/utils/pointUtils";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { StoreApi } from "zustand";
import { PieceType } from "@/lib/apiClient";
import { PromotionRequest } from "../promotionSlice";
import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";

describe("PromotionSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    describe("promptPromotion", () => {
        it("should set pendingPromotion and resolvePromotion, and resolve the promotion", async () => {
            const promotionRequest: PromotionRequest = {
                at: logicalPoint({ x: 0, y: 6 }),
                pieces: [
                    PieceType.BISHOP,
                    PieceType.HORSEY,
                    PieceType.UNDERAGE_PAWN,
                    PieceType.ROOK,
                ],
                piece: createFakePiece({ type: PieceType.PAWN }),
            };

            const promotionPromise = store
                .getState()
                .promptPromotion(promotionRequest);

            const midPromotionState = store.getState();
            expect(midPromotionState.pendingPromotion).toEqual(
                promotionRequest,
            );
            expect(typeof midPromotionState.resolvePromotion).toBe("function");
            midPromotionState.resolvePromotion?.(PieceType.ROOK);

            const result = await promotionPromise;

            expect(result).toBe(PieceType.ROOK);

            // make sure the state was cleaned
            const finalState = store.getState();
            expect(finalState.pendingPromotion).toBeNull();
            expect(finalState.resolvePromotion).toBeNull();
        });

        it("should resolve null if resolvePromotion is called with null", async () => {
            const promotionRequest: PromotionRequest = {
                at: logicalPoint({ x: 2, y: 1 }),
                pieces: [PieceType.QUEEN, PieceType.HORSEY],
                piece: createFakePiece({ type: PieceType.PAWN }),
            };

            const promotionPromise = store
                .getState()
                .promptPromotion(promotionRequest);
            store.getState().resolvePromotion?.(null);

            const result = await promotionPromise;

            expect(result).toBeNull();
        });
    });
});
