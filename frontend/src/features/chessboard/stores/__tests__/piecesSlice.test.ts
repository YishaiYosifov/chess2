import { StoreApi } from "zustand";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import {
    createFakeLegalMoveMap,
    createFakeMove,
    createFakePiece,
    createFakePieceMap,
    createFakePieceMapFromPieces,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { LogicalPoint } from "@/features/point/types";
import { ScreenPoint } from "@/features/point/types";
import { PieceID } from "../../lib/types";
import { LegalMoveMap } from "../../lib/types";
import { PieceMap } from "../../lib/types";
import { Piece } from "../../lib/types";
import {
    logicalPoint,
    pointToStr,
    screenPoint,
} from "@/features/point/pointUtils";
import { GameColor } from "@/lib/apiClient";
import { createMoveOptions } from "../../lib/moveOptions";

describe("PiecesSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
        vi.useFakeTimers({ shouldAdvanceTime: true });
    });

    function expectPieces(
        ...pieces: { id: PieceID; position: LogicalPoint; piece: Piece }[]
    ) {
        const expectedPieces: PieceMap = new Map();
        for (const { id, position, piece } of pieces) {
            expectedPieces.set(id, { ...piece, position });
        }

        const newPieces = store.getState().pieceMap;
        expect(newPieces).toEqual(expectedPieces);
    }

    describe("applyMoveWithIntermediates", () => {
        it("should apply all intermediate steps and final move", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const otherPiece = createFakePiece({
                position: logicalPoint({ x: 5, y: 5 }),
            });
            store.setState({
                pieceMap: createFakePieceMapFromPieces(piece, otherPiece),
            });

            const intermediates = [
                logicalPoint({ x: 1, y: 1 }),
                logicalPoint({ x: 2, y: 2 }),
            ];
            const move = createFakeMove({
                from: piece.position,
                to: logicalPoint({ x: 3, y: 3 }),
                intermediates,
            });

            await store.getState().applyMoveWithIntermediates(move);

            expectPieces(
                { id: "0", position: move.to, piece },
                { id: "1", position: otherPiece.position, piece: otherPiece },
            );
        });

        it("should set pieces to final and animatingPieceMap to first step before awaiting animations", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const otherPiece = createFakePiece({
                position: logicalPoint({ x: 5, y: 5 }),
            });
            store.setState({
                pieceMap: createFakePieceMapFromPieces(piece, otherPiece),
            });

            const intermediates = [
                logicalPoint({ x: 1, y: 1 }),
                logicalPoint({ x: 2, y: 2 }),
            ];
            const move = createFakeMove({
                from: piece.position,
                to: logicalPoint({ x: 3, y: 3 }),
                intermediates,
            });

            // don't await
            store.getState().applyMoveWithIntermediates(move);

            const state = store.getState();
            expectPieces(
                { id: "0", position: move.to, piece }, // final pieces already set
                { id: "1", position: otherPiece.position, piece: otherPiece },
            );
            expect(state.animatingPieceMap!.get("0")!.position).toEqual(
                intermediates[0],
            );
        });
    });

    describe("applyMove", () => {
        it("should return if no piece at move.from", () => {
            const pieceMap = createFakePieceMap();
            store.setState({ pieceMap });

            const move = createFakeMove({
                from: logicalPoint({ x: 11, y: 11 }), // guaranteed empty point
            });

            store.getState().applyMove(move);
            const newPieces = store.getState().pieceMap;
            expect(newPieces).toEqual(pieceMap);
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
                pieceMap: createFakePieceMapFromPieces(
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

    describe("getMoveForSelection", () => {
        it("should return null if no piece selected", async () => {
            store.setState({ selectedPieceId: null });

            const move = await store
                .getState()
                .getMoveForSelection(logicalPoint({ x: 1, y: 1 }));
            expect(move).toBeNull();
        });

        it("should return a valid move for the selected piece", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const move = createFakeMove({
                from: piece.position,
                to: logicalPoint({ x: 1, y: 1 }),
            });

            store.setState({
                selectedPieceId: "0",
                pieceMap: new Map([["0", piece]]),
                moveOptions: {
                    legalMoves: new Map([[pointToStr(piece.position), [move]]]),
                    hasForcedMoves: false,
                },
            });

            const result = await store.getState().getMoveForSelection(move.to);
            expect(result).toEqual(move);
        });
    });

    describe("applyMoveTurn", () => {
        it("should apply the move and call onPieceMovement", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const move = createFakeMove({
                from: piece.position,
                to: logicalPoint({ x: 1, y: 1 }),
            });

            const onPieceMovementMock = vi.fn();
            store.setState({
                pieceMap: new Map([["0", piece]]),
                onPieceMovement: onPieceMovementMock,
            });

            await store.getState().applyMoveTurn(move);

            expectPieces({ id: "0", position: move.to, piece });
            expect(onPieceMovementMock).toHaveBeenCalledWith(move);
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

                const pieceMap: PieceMap = new Map([["0", piece]]);
                const legalMoves: LegalMoveMap = new Map([
                    [pointToStr(piece.position), [move]],
                ]);

                store.setState({
                    selectedPieceId: "0",
                    pieceMap,
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

    describe("goToPosition", () => {
        it("should call applyMoveWithIntermediates in goToPosition when animateIntermediates=true", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const move = createFakeMove({
                from: piece.position,
                to: logicalPoint({ x: 2, y: 2 }),
            });
            const legalMoves = createFakeLegalMoveMap();

            const applyMoveWithIntermediatesMock = vi.fn();
            store.setState({
                applyMoveWithIntermediates: applyMoveWithIntermediatesMock,
            });

            const boardState = {
                pieces: createFakePieceMapFromPieces(piece),
                moveOptions: { legalMoves, hasForcedMoves: false },
                casuedByMove: move,
            };

            await store
                .getState()
                .goToPosition(boardState, { animateIntermediates: true });

            expect(applyMoveWithIntermediatesMock).toHaveBeenCalledWith(move);
            expect(store.getState().moveOptions).toEqual(
                boardState.moveOptions,
            );
        });

        it("should directly set pieces in goToPosition without animateIntermediates", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const newPos = logicalPoint({ x: 1, y: 1 });

            const boardState = {
                pieces: createFakePieceMapFromPieces({
                    ...piece,
                    position: newPos,
                }),
                moveOptions: { legalMoves: new Map(), hasForcedMoves: false },
            };

            await store.getState().goToPosition(boardState);

            expect(store.getState().pieceMap).toEqual(boardState.pieces);
            expect(store.getState().moveOptions).toEqual(
                boardState.moveOptions,
            );
        });
    });
});
