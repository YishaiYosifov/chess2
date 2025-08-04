import { StoreApi } from "zustand";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import {
    createFakeLegalMoveMapFromPieces,
    createFakeMove,
    createFakePiece,
    createFakePieceMap,
    createFakePieceMapFromPieces,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { LogicalPoint } from "@/features/point/types";
import { ScreenPoint } from "@/features/point/types";
import { PieceID } from "../../lib/types";
import { ProcessedMoveOptions } from "@/features/chessboard/lib/types";
import { LegalMoveMap } from "../../lib/types";
import { PieceMap } from "../../lib/types";
import { Piece } from "../../lib/types";
import { logicalPoint, pointToStr, screenPoint } from "@/lib/utils/pointUtils";
import { GameColor } from "@/lib/apiClient";
import { createMoveOptions } from "../../lib/moveOptions";

describe("PiecesSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
        vi.useFakeTimers();
    });

    function expectPieces(
        ...pieces: { id: PieceID; position: LogicalPoint; piece: Piece }[]
    ) {
        const expectedPieces: PieceMap = new Map();
        for (const { id, position, piece } of pieces) {
            expectedPieces.set(id, { ...piece, position });
        }

        const newPieces = store.getState().pieces;
        expect(newPieces).toEqual(expectedPieces);
    }

    describe("applyMove", () => {
        it("should return if no piece at move.from", () => {
            const pieces = createFakePieceMap();
            store.setState({ pieces });

            const move = createFakeMove({
                from: logicalPoint({ x: 9, y: 9 }), // guaranteed empty point
            });

            store.getState().applyMove(move);
            const newPieces = store.getState().pieces;
            expect(newPieces).toEqual(pieces);
        });

        it("should move piece, delete captured pieces and handle sideEffects", () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 1, y: 1 }),
            });
            const capturedPiece = createFakePiece({
                position: logicalPoint({ x: 2, y: 2 }),
            });
            const sideEffectPiece = createFakePiece({
                position: logicalPoint({ x: 3, y: 3 }),
            });

            store.setState({
                pieces: createFakePieceMapFromPieces(
                    piece,
                    capturedPiece,
                    sideEffectPiece,
                ),
            });

            const sideEffectMove = createFakeMove({
                from: sideEffectPiece.position,
                to: logicalPoint({ x: 4, y: 4 }),
            });

            const move = createFakeMove({
                from: piece.position,
                to: logicalPoint({ x: 5, y: 5 }),
                captures: [capturedPiece.position],
                sideEffects: [sideEffectMove],
            });

            store.getState().applyMove(move);

            expectPieces(
                { id: "0", position: move.to, piece },
                {
                    id: "2",
                    position: sideEffectMove.to,
                    piece: sideEffectPiece,
                },
            );
        });
    });

    describe("tryApplySelectedMove", () => {
        it("should not move if no piece is selected", async () => {
            const pieces = createFakePieceMap();
            store.setState({ pieces });

            store.getState().tryApplySelectedMove(logicalPoint({ x: 6, y: 9 }));
            const newPieces = store.getState().pieces;
            expect(newPieces).toEqual(pieces);
        });

        it("should not move if no legal move is found", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 1, y: 1 }),
            });
            const pieces: PieceMap = new Map([
                [
                    "1",
                    createFakePiece({ position: logicalPoint({ x: 2, y: 2 }) }),
                ],
            ]);

            const legalMoves = createFakeLegalMoveMapFromPieces(piece);

            store.setState({
                selectedPieceId: "1",
                pieces,
                moveOptions: createMoveOptions({ legalMoves }),
            });

            await store
                .getState()
                .tryApplySelectedMove(logicalPoint({ x: 9, y: 9 }));
            expect(store.getState().pieces).toEqual(pieces);
        });

        it("should move the piece if the move is found", async () => {
            const position = logicalPoint({ x: 3, y: 3 });
            const moveTo = logicalPoint({ x: 4, y: 4 });
            const piece = createFakePiece({ position });

            const move = createFakeMove({ from: position, to: moveTo });
            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(position), [move]],
            ]);

            store.setState({
                selectedPieceId: "0",
                pieces: new Map([["0", piece]]),
                moveOptions: { legalMoves, hasForcedMoves: true },
            });

            await store.getState().tryApplySelectedMove(move.to);

            expectPieces({ id: "0", position: move.to, piece });
            const {
                moveOptions: newMoveOptions,
                highlightedLegalMoves,
                selectedPieceId,
            } = store.getState();
            expect(newMoveOptions).toEqual<ProcessedMoveOptions>(
                createMoveOptions(),
            );
            expect(highlightedLegalMoves.length).toBe(0);
            expect(selectedPieceId).toBeNull();
        });

        it("should call onPieceMovement with correct MoveKey after a valid move", async () => {
            const position = logicalPoint({ x: 3, y: 3 });
            const moveTo = logicalPoint({ x: 4, y: 4 });
            const piece = createFakePiece({ position });

            const move = createFakeMove({ from: position, to: moveTo });
            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(position), [move]],
            ]);

            const onPieceMovement = vi.fn();

            store.setState({
                selectedPieceId: "0",
                pieces: new Map([["0", piece]]),
                moveOptions: { legalMoves, hasForcedMoves: false },
                onPieceMovement,
            });

            await store.getState().tryApplySelectedMove(moveTo);

            expect(onPieceMovement).toHaveBeenCalledTimes(1);
            expect(onPieceMovement).toHaveBeenCalledWith({
                from: move.from,
                to: move.to,
                promotesTo: move.promotesTo,
            });
        });
    });

    describe("handleMousePieceDrop", () => {
        it.each([
            [
                GameColor.WHITE,
                logicalPoint({ x: 2, y: 7 }),
                screenPoint({ x: 20, y: 20 }),
            ],
            [
                GameColor.BLACK,
                logicalPoint({ x: 7, y: 2 }),
                screenPoint({ x: 20, y: 20 }),
            ],
        ])(
            "should move the selected piece based on drop coordinates depending on viewingFrom",
            async (
                viewingFrom: GameColor,
                expectedPosition: LogicalPoint,
                mousePosition: ScreenPoint,
            ) => {
                const piece = createFakePiece({
                    position: logicalPoint({ x: 0, y: 0 }),
                });
                const move = createFakeMove({
                    from: piece.position,
                    to: expectedPosition,
                });

                const pieces: PieceMap = new Map([["0", piece]]);
                const legalMoves: LegalMoveMap = new Map([
                    [pointToStr(piece.position), [move]],
                ]);

                store.setState({
                    selectedPieceId: "0",
                    pieces,
                    boardDimensions: { width: 10, height: 10 },
                    boardRect: {
                        left: 0,
                        top: 0,
                        width: 100,
                        height: 100,
                    } as DOMRect,
                    viewingFrom,
                    moveOptions: createMoveOptions({ legalMoves }),
                });

                await store.getState().handleMousePieceDrop({
                    mousePoint: mousePosition,
                    isDrag: false,
                });

                expectPieces({ id: "0", position: expectedPosition, piece });
            },
        );
    });

    describe("addAnimatingPiece", () => {
        it("should add pieceId to animatingPieces and remove it after 100ms", () => {
            const pieceId = "1";
            store.getState().addAnimatingPiece(pieceId);

            let state = store.getState();
            expect(state.animatingPieces.has(pieceId)).toBe(true);

            vi.advanceTimersByTime(100);

            state = store.getState();
            expect(state.animatingPieces.has(pieceId)).toBe(false);
        });

        it("should not add pieceId multiple times", () => {
            const pieceId = "1";
            store.getState().addAnimatingPiece(pieceId);
            store.getState().addAnimatingPiece(pieceId);

            const state = store.getState();
            expect(state.animatingPieces.size).toBe(1);
        });
    });
});
