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

        it("should set highlightedLegalMoves with unique points from 'to' and 'triggers'", () => {
            const piece = createFakePiece();
            const move1To = logicalPoint({ x: 3, y: 3 });
            const move2To = logicalPoint({ x: 4, y: 4 });
            const trigger1 = logicalPoint({ x: 5, y: 5 });
            const trigger2 = move1To; // same as move1To to test deduplication

            const move1 = createFakeMove({
                from: piece.position,
                to: move1To,
                triggers: [trigger1, trigger2],
            });

            const move2 = createFakeMove({
                from: piece.position,
                to: move2To,
                triggers: [],
            });

            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(piece.position), [move1, move2]],
            ]);

            store.setState({
                pieces: createFakePieceMapFromPieces(piece),
                moveOptions: createMoveOptions({ legalMoves }),
            });

            store.getState().showLegalMoves("0");

            const state = store.getState();
            const highlighted = state.highlightedLegalMoves;

            expect(highlighted).toHaveLength(3);
            expect(highlighted).toEqual(
                expect.arrayContaining([move1To, move2To, trigger1]),
            );
        });
    });
});
