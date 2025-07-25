import { StoreApi } from "zustand";
import { ChessboardState, createChessboardStore } from "../chessboardStore";
import {
    createFakeMove,
    createFakePiece,
    createFakePieceMapFromPieces,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { LegalMoveMap, PieceID } from "@/types/tempModels";
import { pointToStr } from "@/lib/utils/pointUtils";
import { createMoveOptions } from "../../lib/moveOptions";

describe("LegalMoveSlice", () => {
    let store: StoreApi<ChessboardState>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    describe("showLegalMoves", () => {
        it("should warn and return if no piece found by ID", () => {
            const warnSpy = vi
                .spyOn(console, "warn")
                .mockImplementation(() => {});
            store.setState({ pieces: new Map() });

            store.getState().showLegalMoves("1" as PieceID);

            expect(warnSpy).toHaveBeenCalled();
            warnSpy.mockRestore();
        });

        it("should set highlightedLegalMoves correctly", () => {
            const piece1 = createFakePiece();
            const piece1Move1 = createFakeMove({
                from: piece1.position,
            });
            const piece1Move2 = createFakeMove({
                from: piece1.position,
            });

            const piece2 = createFakePiece();
            const piece2Move = createFakeMove({ from: piece2.position });

            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(piece1.position), [piece1Move1, piece1Move2]],
                [pointToStr(piece2.position), [piece2Move]],
            ]);
            store.setState({
                pieces: createFakePieceMapFromPieces(piece1, piece2),
                moveOptions: createMoveOptions({ legalMoves }),
            });

            store.getState().showLegalMoves("0");

            const state = store.getState();
            expect(state.highlightedLegalMoves).toEqual([
                piece1Move1.to,
                piece1Move2.to,
                ...piece1Move1.triggers,
                ...piece1Move2.triggers,
            ]);
        });
    });
});
