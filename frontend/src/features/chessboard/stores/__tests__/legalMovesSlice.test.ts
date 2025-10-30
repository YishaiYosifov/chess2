import { StoreApi } from "zustand";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import {
    createFakeLegalMoveMap,
    createFakeMove,
    createFakePiece,
    createRandomPoint,
    createSequentialPieceMapFromPieces,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { LegalMoveMap, Piece } from "../../lib/types";
import { logicalPoint, pointToStr } from "@/features/point/pointUtils";
import { createMoveOptions } from "../../lib/moveOptions";
import { PieceType } from "@/lib/apiClient";
import { waitFor } from "@testing-library/react";

describe("LegalMovesSlice", () => {
    let store: StoreApi<ChessboardStore>;
    let piece: Piece;

    beforeEach(() => {
        store = createChessboardStore();
        piece = createFakePiece();
    });

    describe("getLegalMove", () => {
        it("should return null if no legal moves exist for the origin", async () => {
            const origin = logicalPoint({ x: 1, y: 2 });
            const dest = logicalPoint({ x: 3, y: 3 });
            const pieceMap = createSequentialPieceMapFromPieces(
                createFakePiece({ position: origin }),
            );

            store.setState({
                moveOptions: createMoveOptions({ legalMoves: new Map() }),
            });

            const result = await store
                .getState()
                .getLegalMove(dest, "0", pieceMap);
            expect(result).toBeNull();
        });

        it("should return null if no move matches the destination", async () => {
            const origin = logicalPoint({ x: 1, y: 1 });
            const dest = logicalPoint({ x: 5, y: 5 });
            const pieceMap = createSequentialPieceMapFromPieces(
                createFakePiece({ position: origin }),
            );

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
                .getLegalMove(dest, "0", pieceMap);
            expect(result).toBeNull();
        });

        it("should return the single matching move if only one matches", async () => {
            const origin = logicalPoint({ x: 2, y: 2 });
            const dest = logicalPoint({ x: 3, y: 3 });
            const pieceMap = createSequentialPieceMapFromPieces(
                createFakePiece({ position: origin }),
            );

            const move = createFakeMove({ from: origin, to: dest });
            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(origin), [move]],
            ]);

            store.setState({
                moveOptions: createMoveOptions({ legalMoves }),
            });

            const result = await store
                .getState()
                .getLegalMove(dest, "0", pieceMap);
            expect(result).toEqual(move);
        });

        it("should return a move that matches via trigger", async () => {
            const origin = logicalPoint({ x: 4, y: 4 });
            const trigger = logicalPoint({ x: 6, y: 6 });
            const dest = logicalPoint({ x: 9, y: 9 });
            const pieceMap = createSequentialPieceMapFromPieces(
                createFakePiece({ position: origin }),
            );

            const triggerMove = createFakeMove({
                from: origin,
                to: dest,
                triggers: [trigger],
            });
            const regularMove = createFakeMove({ from: origin, to: dest });

            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(origin), [triggerMove, regularMove]],
            ]);

            store.setState({
                moveOptions: createMoveOptions({ legalMoves }),
            });

            const result = await store
                .getState()
                .getLegalMove(trigger, "0", pieceMap);
            expect(result).toEqual(triggerMove);
        });

        it("should return a move that matches via trigger even if trigger wasn't used", async () => {
            const origin = logicalPoint({ x: 4, y: 4 });
            const trigger = logicalPoint({ x: 6, y: 6 });
            const dest = logicalPoint({ x: 9, y: 9 });
            const pieceMap = createSequentialPieceMapFromPieces(
                createFakePiece({ position: origin }),
            );

            const triggerMove = createFakeMove({
                from: origin,
                to: dest,
                triggers: [trigger],
            });
            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(origin), [triggerMove]],
            ]);
            store.setState({
                moveOptions: createMoveOptions({ legalMoves }),
            });

            const result = await store
                .getState()
                .getLegalMove(dest, "0", pieceMap);
            expect(result).toEqual(triggerMove);
        });

        it("should return null if multiple moves match but promotion is cancelled", async () => {
            const origin = logicalPoint({ x: 0, y: 1 });
            const dest = logicalPoint({ x: 0, y: 7 });
            const pieceMap = createSequentialPieceMapFromPieces(
                createFakePiece({ position: origin }),
            );

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
                pendingPromotion: null,
                resolvePromotion: null,
            });

            const promise = store.getState().getLegalMove(dest, "0", pieceMap);

            await waitFor(() => {
                const state = store.getState();
                expect(state.resolvePromotion).not.toBeNull();
            });
            store.getState().resolvePromotion?.(null);

            const result = await promise;
            expect(result).toBeNull();
        });
    });

    describe("showLegalMoves", () => {
        it("should highlight unique points from 'to' and 'triggers'", () => {
            const piece = createFakePiece();
            const move1To = logicalPoint({ x: 3, y: 3 });
            const move2To = logicalPoint({ x: 4, y: 4 });
            const trigger1 = logicalPoint({ x: 5, y: 5 });
            const trigger2 = move1To; // same as move1To

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
                pieceMap: createSequentialPieceMapFromPieces(piece),
                moveOptions: createMoveOptions({ legalMoves }),
            });

            store.getState().showLegalMoves(piece);

            const highlighted = store.getState().highlightedLegalMoves;
            expect(highlighted).toHaveLength(3);
            expect(highlighted).toEqual(
                expect.arrayContaining([move1To, move2To, trigger1]),
            );
        });

        it("should highlight the first intermediate instead of 'to'", () => {
            const intermediate = logicalPoint({ x: 1, y: 1 });
            const destination = logicalPoint({ x: 2, y: 2 });
            const move = createFakeMove({
                from: piece.position,
                to: destination,
                intermediates: [intermediate],
            });

            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(piece.position), [move]],
            ]);

            store.setState({
                pieceMap: createSequentialPieceMapFromPieces(piece),
                moveOptions: createMoveOptions({ legalMoves }),
            });

            store.getState().showLegalMoves(piece);

            const highlighted = store.getState().highlightedLegalMoves;
            expect(highlighted).toHaveLength(1);
            expect(highlighted[0]).toEqual(intermediate);
            expect(highlighted).not.toContainEqual(destination);
        });
    });

    describe("hideLegalMoves", () => {
        it("should remove any highlighted legal moves", () => {
            store.setState({
                highlightedLegalMoves: [
                    createRandomPoint(),
                    createRandomPoint(),
                    createRandomPoint(),
                ],
            });

            store.getState().hideLegalMoves();
            expect(store.getState().highlightedLegalMoves).toHaveLength(0);
        });
    });

    describe("hasMovesFromTo", () => {
        it("should return false if there are no moves from the given 'from' position", () => {
            const move = createFakeMove({
                from: logicalPoint({ x: 0, y: 0 }),
                to: logicalPoint({ x: 2, y: 2 }),
            });

            store.setState({
                moveOptions: {
                    legalMoves: new Map([[pointToStr(move.from), [move]]]),
                    hasForcedMoves: false,
                },
            });

            const result = store
                .getState()
                .hasMovesFromTo(logicalPoint({ x: 1, y: 1 }), move.to);
            expect(result).toBe(false);
        });

        it("should return false if there are moves from 'from' but none to the 'to' position", () => {
            const move = createFakeMove({
                from: logicalPoint({ x: 0, y: 0 }),
                to: logicalPoint({ x: 2, y: 2 }),
            });

            store.setState({
                moveOptions: {
                    legalMoves: new Map([[pointToStr(move.from), [move]]]),
                    hasForcedMoves: false,
                },
            });

            const result = store
                .getState()
                .hasMovesFromTo(move.from, logicalPoint({ x: 1, y: 1 }));
            expect(result).toBe(false);
        });

        it("should return true if there is at least one move from 'from' to 'to'", () => {
            const move1 = createFakeMove({
                from: logicalPoint({ x: 0, y: 0 }),
                to: logicalPoint({ x: 1, y: 1 }),
            });
            const move2 = createFakeMove({
                from: logicalPoint({ x: 0, y: 0 }),
                to: logicalPoint({ x: 2, y: 2 }),
            });

            store.setState({
                moveOptions: {
                    legalMoves: new Map([
                        [pointToStr(move1.from), [move1, move2]],
                    ]),
                    hasForcedMoves: false,
                },
            });

            const result = store
                .getState()
                .hasMovesFromTo(
                    logicalPoint({ x: 0, y: 0 }),
                    logicalPoint({ x: 1, y: 1 }),
                );
            expect(result).toBe(true);
        });
    });

    describe("setLegalMoves", () => {
        it("should update moveOptions in the store", () => {
            const legalMoves = createFakeLegalMoveMap();
            const newMoveOptions = createMoveOptions({ legalMoves });
            store.getState().setLegalMoves(newMoveOptions);

            expect(store.getState().moveOptions).toBe(newMoveOptions);
        });

        it("should remove highlight legal moves", () => {
            store.setState({
                highlightedLegalMoves: [
                    logicalPoint({ x: 1, y: 2 }),
                    logicalPoint({ x: 3, y: 4 }),
                ],
            });

            store.getState().setLegalMoves(createMoveOptions());

            expect(store.getState().highlightedLegalMoves.length).toBe(0);
        });
    });
});
