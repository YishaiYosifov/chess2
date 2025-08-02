import { StoreApi } from "zustand";
import { ChessboardState, createChessboardStore } from "../chessboardStore";
import {
    createFakeMove,
    createFakePiece,
    createFakePieceMapFromPieces,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { LegalMoveMap, Piece } from "../../lib/types";
import { logicalPoint, pointToStr } from "@/lib/utils/pointUtils";
import { createMoveOptions } from "../../lib/moveOptions";
import { PieceType } from "@/lib/apiClient";

describe("LegalMoveSlice", () => {
    let store: StoreApi<ChessboardState>;
    let piece: Piece;

    beforeEach(() => {
        store = createChessboardStore();
        piece = createFakePiece();
    });

    describe("getLegalMove", () => {
        it("should return undefined if no legal moves exist for the origin", async () => {
            const origin = logicalPoint({ x: 1, y: 2 });
            const dest = logicalPoint({ x: 3, y: 3 });

            store.setState({
                moveOptions: createMoveOptions({ legalMoves: new Map() }),
            });

            const result = await store
                .getState()
                .getLegalMove(origin, dest, piece);
            expect(result).toBeUndefined();
        });

        it("should return undefined if no move matches the destination", async () => {
            const origin = logicalPoint({ x: 1, y: 1 });
            const dest = logicalPoint({ x: 5, y: 5 });

            const move = createFakeMove({
                from: origin,
                to: logicalPoint({ x: 2, y: 2 }),
            });
            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(origin), [move]],
            ]);
            store.setState({
                moveOptions: createMoveOptions({ legalMoves }),
            });

            const result = await store
                .getState()
                .getLegalMove(origin, dest, piece);
            expect(result).toBeUndefined();
        });

        it("should return the single matching move if only one matches", async () => {
            const origin = logicalPoint({ x: 2, y: 2 });
            const dest = logicalPoint({ x: 3, y: 3 });

            const move = createFakeMove({ from: origin, to: dest });

            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(origin), [move]],
            ]);

            store.setState({
                moveOptions: createMoveOptions({ legalMoves }),
            });

            const result = await store
                .getState()
                .getLegalMove(origin, dest, piece);
            expect(result).toEqual(move);
        });

        it("should return a move that matches via trigger", async () => {
            const origin = logicalPoint({ x: 4, y: 4 });
            const trigger = logicalPoint({ x: 6, y: 6 });
            const dest = logicalPoint({ x: 9, y: 9 });

            const triggerMove = createFakeMove({
                from: origin,
                to: dest,
                triggers: [trigger],
            });
            const regularMove = createFakeMove({
                from: origin,
                to: dest,
            });

            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(origin), [triggerMove, regularMove]],
            ]);

            store.setState({
                moveOptions: createMoveOptions({ legalMoves }),
            });

            const result = await store
                .getState()
                .getLegalMove(origin, trigger, piece);
            expect(result).toEqual(triggerMove);
        });

        it("should prompt for promotion and return the selected move when multiple moves match", async () => {
            const origin = logicalPoint({ x: 0, y: 1 });
            const dest = logicalPoint({ x: 0, y: 7 });

            const queenMove = createFakeMove({
                from: origin,
                to: dest,
                promotesTo: PieceType.QUEEN,
            });

            const rookMove = createFakeMove({
                from: origin,
                to: dest,
                promotesTo: PieceType.ROOK,
            });

            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(origin), [queenMove, rookMove]],
            ]);

            store.setState({
                moveOptions: createMoveOptions({ legalMoves }),
            });

            const getLegalMovePromise = store
                .getState()
                .getLegalMove(origin, dest, piece);

            store.getState().resolvePromotion?.(PieceType.ROOK);

            const result = await getLegalMovePromise;
            expect(result).toEqual(rookMove);
        });

        it("should return undefined if promotion is cancelled", async () => {
            const origin = logicalPoint({ x: 0, y: 1 });
            const dest = logicalPoint({ x: 0, y: 7 });

            const queenMove = createFakeMove({
                from: origin,
                to: dest,
                promotesTo: PieceType.QUEEN,
            });

            const horseyMove = createFakeMove({
                from: origin,
                to: dest,
                promotesTo: PieceType.HORSEY,
            });

            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(origin), [queenMove, horseyMove]],
            ]);

            store.setState({
                moveOptions: createMoveOptions({ legalMoves }),
                pendingPromotion: null,
                resolvePromotion: null,
            });

            const getLegalMovePromise = store
                .getState()
                .getLegalMove(origin, dest, piece);

            store.getState().resolvePromotion?.(null);

            const result = await getLegalMovePromise;
            expect(result).toBeUndefined();
        });
    });

    describe("showLegalMoves", () => {
        it("should do nothing if no piece found by ID", () => {
            store.setState({ pieces: new Map([["123", createFakePiece()]]) });

            store.getState().showLegalMoves("1");

            expect(store.getState().highlightedLegalMoves.length).toBe(0);
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
