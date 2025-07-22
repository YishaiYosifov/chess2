import { StoreApi } from "zustand";
import { ChessboardState, createChessboardStore } from "../chessboardStore";
import {
    createFakeLegalMoveMap,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { PieceMap } from "@/types/tempModels";

describe("CoreSlice", () => {
    let store: StoreApi<ChessboardState>;

    beforeEach(() => {
        vi.useFakeTimers();
        store = createChessboardStore();
    });

    describe("setBoardRect", () => {
        it("should set boardRect state", () => {
            const rect = {
                left: 10,
                top: 20,
                width: 100,
                height: 200,
            } as DOMRect;
            store.getState().setBoardRect(rect);
            expect(store.getState().boardRect).toBe(rect);
        });
    });

    describe("resetState", () => {
        it("should reset the state to a playable position", () => {
            const piece = createFakePiece();
            const pieces: PieceMap = new Map([["0", piece]]);
            const legalMoves = createFakeLegalMoveMap(piece);

            store.getState().resetState(pieces, legalMoves);

            const state = store.getState();
            expect(state).toMatchObject({
                pieces,
                legalMoves,
                selectedPieceId: undefined,
                highlightedLegalMoves: [],
            });
        });
    });
});
