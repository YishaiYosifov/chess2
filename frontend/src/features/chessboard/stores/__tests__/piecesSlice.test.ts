import { StoreApi } from "zustand";

import {
    createFakeLegalMoveMap,
    createFakeMove,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";
import {
    logicalPoint,
    pointToStr,
    screenPoint,
} from "@/features/point/pointUtils";

import { BoardState, IntermediateSquare, LegalMoveMap } from "../../lib/types";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";
import { createMoveOptions } from "../../lib/moveOptions";
import { LogicalPoint } from "@/features/point/types";
import { ScreenPoint } from "@/features/point/types";
import BoardPieces from "../../lib/boardPieces";
import { GameColor, PieceType, SpecialMoveType } from "@/lib/apiClient";
import { Piece } from "../../lib/types";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";

vi.mock("@/features/audio/audioPlayer");

describe("PiecesSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
        store.setState({
            boardRect: {
                left: 0,
                top: 0,
                width: 100,
                height: 100,
            } as DOMRect,
        });
        vi.useFakeTimers({ shouldAdvanceTime: true });
    });

    function expectPieces(
        ...pieces: { position: LogicalPoint; piece: Piece }[]
    ) {
        const expectedPieces = new BoardPieces();
        for (const { position, piece } of pieces) {
            expectedPieces.addAt(piece, position);
        }

        const newPieces = store.getState().pieces;
        expect(newPieces).toEqual(expectedPieces);
    }

    describe("selectPiece", () => {
        it("should return false and warn if pieceId does not exist", () => {
            const result = store.getState().selectPiece("nonexistent");
            expect(result).toBe(false);
        });

        it("should return false if the piece is already selected", () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            store.setState({
                pieces: BoardPieces.fromPieces(piece),
                selectedPieceId: piece.id,
            });

            const result = store.getState().selectPiece(piece.id);

            expect(result).toBe(false);
            expect(store.getState().selectedPieceId).toBe(piece.id);
        });

        it("should select a piece if it has legal moves", () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const showLegalMovesMock = vi.fn(() => true);

            store.setState({
                pieces: BoardPieces.fromPieces(piece),
                showLegalMoves: showLegalMovesMock,
                selectedPieceId: null,
            });

            const result = store.getState().selectPiece(piece.id);

            expect(result).toBe(true);
            expect(store.getState().selectedPieceId).toBe(piece.id);
            expect(showLegalMovesMock).toHaveBeenCalledWith(piece);
        });

        it("should not select a piece if it has no legal moves", () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const showLegalMovesMock = vi.fn(() => false);

            store.setState({
                pieces: BoardPieces.fromPieces(piece),
                showLegalMoves: showLegalMovesMock,
                selectedPieceId: null,
            });

            const result = store.getState().selectPiece(piece.id);

            expect(result).toBe(false);
            expect(store.getState().selectedPieceId).toBeNull();
            expect(showLegalMovesMock).toHaveBeenCalledWith(piece);
        });
    });

    describe("unselectPiece", () => {
        it("should hide legal moves and clear selectedPieceId", () => {
            const hideLegalMovesMock = vi.fn();
            store.setState({
                selectedPieceId: "0",
                hideLegalMoves: hideLegalMovesMock,
            });

            store.getState().unselectPiece();

            expect(hideLegalMovesMock).toHaveBeenCalled();
            expect(store.getState().selectedPieceId).toBeNull();
        });
    });

    describe("applyMoveAnimated", () => {
        it("should apply all intermediate steps and final move", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const pieces = BoardPieces.fromPieces(piece);
            store.setState({ pieces });

            const intermediates: IntermediateSquare[] = [
                { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
                { position: logicalPoint({ x: 2, y: 2 }), isCapture: false },
            ];
            const move = createFakeMove({
                from: piece.position,
                to: logicalPoint({ x: 3, y: 3 }),
                intermediates,
            });

            store.getState().applyMoveAnimated(move);

            // final position should be set first, non animated
            const expectedPieces = new BoardPieces(pieces);
            expectedPieces.playMove(move);
            expect(store.getState().pieces).toEqual(expectedPieces);

            for (const intermediate of intermediates) {
                const expectedAnimationPieces = new BoardPieces(pieces);
                expectedAnimationPieces.movePiece(
                    piece.id,
                    intermediate.position,
                );
                expect(store.getState().animatingPieces).toEqual(
                    expectedAnimationPieces,
                );

                vi.advanceTimersByTime(100);
                await flushMicrotasks();
            }
        });
    });

    describe("applyMoveImmediate", () => {
        it("should simulate the move and animate it", () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 1, y: 1 }),
            });

            store.setState({
                pieces: BoardPieces.fromPieces(piece),
            });

            const move = createFakeMove({
                from: piece.position,
                to: logicalPoint({ x: 5, y: 5 }),
            });

            store.getState().applyMoveImmediate(move);

            expectPieces({ piece, position: move.to });
            expect(store.getState().animatingPieceIds).toEqual(
                new Set([piece.id]),
            );
        });
    });

    describe("handleMousePieceDrop", () => {
        it.each([
            {
                viewingFrom: GameColor.WHITE,
                expectedPosition: logicalPoint({ x: 2, y: 7 }),
                mousePosition: screenPoint({ x: 20, y: 20 }),
            },
            {
                viewingFrom: GameColor.BLACK,
                expectedPosition: logicalPoint({ x: 7, y: 2 }),
                mousePosition: screenPoint({ x: 20, y: 20 }),
            },
        ])(
            "should move the selected piece based on drop coordinates depending on viewingFrom",
            async ({
                viewingFrom,
                expectedPosition,
                mousePosition,
            }: {
                viewingFrom: GameColor;
                expectedPosition: LogicalPoint;
                mousePosition: ScreenPoint;
            }) => {
                const piece = createFakePiece({
                    position: logicalPoint({ x: 0, y: 0 }),
                });
                const move = createFakeMove({
                    from: piece.position,
                    to: expectedPosition,
                });

                const pieces = BoardPieces.fromPieces(piece);
                const legalMoves: LegalMoveMap = new Map([
                    [pointToStr(piece.position), [move]],
                ]);

                store.setState({
                    selectedPieceId: piece.id,
                    pieces,
                    viewingFrom,
                    moveOptions: createMoveOptions({ legalMoves }),
                });

                const result = await store.getState().handleMousePieceDrop({
                    mousePoint: mousePosition,
                    isDrag: false,
                    isDoubleClick: false,
                });

                expect(result).toEqual({ success: true });
                expectPieces({ position: expectedPosition, piece });
            },
        );

        it("should return false immediately if isProcessingMove is true", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const move = createFakeMove({
                from: piece.position,
                to: logicalPoint({ x: 1, y: 1 }),
            });

            const pieces = BoardPieces.fromPieces(piece);
            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(piece.position), [move]],
            ]);

            store.setState({
                isProcessingMove: true,
                selectedPieceId: piece.id,
                pieces,
                moveOptions: createMoveOptions({ legalMoves }),
            });

            const result = await store.getState().handleMousePieceDrop({
                mousePoint: screenPoint({ x: 10, y: 80 }),
                isDrag: false,
                isDoubleClick: false,
            });

            expect(result).toEqual({ success: false });
            expect(store.getState().pieces).toEqual(pieces);
        });

        it("should set isProcessingMove to true before processing and reset to false after", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const move = createFakeMove({
                from: piece.position,
                to: logicalPoint({ x: 1, y: 1 }),
            });

            const pieces = BoardPieces.fromPieces(piece);
            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(piece.position), [move]],
            ]);

            store.setState({
                selectedPieceId: piece.id,
                pieces,
                moveOptions: createMoveOptions({ legalMoves }),
            });

            const movePromise = store.getState().handleMousePieceDrop({
                mousePoint: screenPoint({ x: 10, y: 80 }),
                isDrag: false,
                isDoubleClick: false,
            });

            expect(store.getState().isProcessingMove).toBe(true);
            await movePromise;
            expect(store.getState().isProcessingMove).toBe(false);
        });

        it("should flash legal moves if no move is found and hasForcedMoves is true and isDrag is true", async () => {
            const pieces = BoardPieces.fromPieces(createFakePiece());
            const flashLegalMovesMock = vi.fn();
            store.setState({
                pieces,
                moveOptions: createMoveOptions({
                    legalMoves: new Map(),
                    hasForcedMoves: true,
                }),
                flashLegalMoves: flashLegalMovesMock,
            });

            const result = await store.getState().handleMousePieceDrop({
                mousePoint: screenPoint({ x: 10, y: 10 }),
                isDrag: true,
                isDoubleClick: false,
            });

            expect(result).toEqual({ success: false });
            expect(flashLegalMovesMock).toHaveBeenCalled();
            expect(AudioPlayer.playAudio).toHaveBeenCalledExactlyOnceWith(
                AudioType.ILLEGAL_MOVE,
            );
        });

        it("should not flash legal moves if no move is found and isDrag is false", async () => {
            const pieces = BoardPieces.fromPieces(createFakePiece());
            const flashLegalMovesMock = vi.fn();
            store.setState({
                pieces,
                moveOptions: createMoveOptions({
                    legalMoves: new Map(),
                    hasForcedMoves: true,
                }),
                flashLegalMoves: flashLegalMovesMock,
            });

            const result = await store.getState().handleMousePieceDrop({
                mousePoint: screenPoint({ x: 10, y: 10 }),
                isDrag: false,
                isDoubleClick: false,
            });

            expect(result).toEqual({ success: false });
            expect(flashLegalMovesMock).not.toHaveBeenCalled();
        });

        it("should not flash legal moves if no move is found and hasForcedMoves is false", async () => {
            const pieces = BoardPieces.fromPieces(createFakePiece());
            const flashLegalMovesMock = vi.fn();
            store.setState({
                pieces,
                moveOptions: createMoveOptions({
                    legalMoves: new Map(),
                    hasForcedMoves: false,
                }),
                flashLegalMoves: flashLegalMovesMock,
            });

            const result = await store.getState().handleMousePieceDrop({
                mousePoint: screenPoint({ x: 10, y: 10 }),
                isDrag: true,
                isDoubleClick: false,
            });

            expect(result).toEqual({ success: false });
            expect(flashLegalMovesMock).not.toHaveBeenCalled();
        });

        it("should return needsDoubleClick if the piece has moves from its position to dest, and dest === piece position", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const pieces = BoardPieces.fromPieces(piece);
            const move = createFakeMove({
                from: piece.position,
                to: piece.position,
            });
            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(piece.position), [move]],
            ]);

            store.setState({
                selectedPieceId: piece.id,
                pieces,
                moveOptions: createMoveOptions({ legalMoves }),
            });

            const result = await store.getState().handleMousePieceDrop({
                mousePoint: screenPoint({ x: 0, y: 90 }),
                isDrag: false,
                isDoubleClick: false,
            });

            expect(result).toEqual({ success: false, needsDoubleClick: true });
        });

        it("should not return needsDouble click if the piece doesn't have moves from its position to dest, and dest === piece position", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const pieces = BoardPieces.fromPieces(piece);
            const move = createFakeMove({
                from: piece.position,
                to: logicalPoint({ x: 1, y: 1 }),
            });
            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(piece.position), [move]],
            ]);

            store.setState({
                selectedPieceId: piece.id,
                pieces,
                moveOptions: createMoveOptions({ legalMoves }),
            });

            const result = await store.getState().handleMousePieceDrop({
                mousePoint: screenPoint({ x: 0, y: 90 }),
                isDrag: false,
                isDoubleClick: false,
            });

            expect(result).toEqual({ success: false });
        });
    });

    describe("goToPosition", () => {
        it("should call applyMoveAnimated in goToPosition when animateIntermediates=true", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const move = createFakeMove({
                from: piece.position,
                to: logicalPoint({ x: 2, y: 2 }),
            });
            const legalMoves = createFakeLegalMoveMap();

            const applyMoveAnimatedMock = vi.fn();
            store.setState({
                applyMoveAnimated: applyMoveAnimatedMock,
            });

            const boardState: BoardState = {
                pieces: BoardPieces.fromPieces(piece),
                moveOptions: { legalMoves, hasForcedMoves: false },
                moveThatProducedPosition: move,
                moveFromPreviousViewedPosition: move,
            };

            await store
                .getState()
                .goToPosition(boardState, { animateIntermediates: true });

            expect(applyMoveAnimatedMock).toHaveBeenCalledExactlyOnceWith(move);
            expect(store.getState().moveOptions).toEqual(
                boardState.moveOptions,
            );
        });

        it("should directly set pieces in goToPosition without animateIntermediates", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const newPos = logicalPoint({ x: 1, y: 1 });

            const playAnimationMock = vi.fn();
            store.setState({
                playAnimation: playAnimationMock,
                pieces: BoardPieces.fromPieces(piece),
            });

            const boardState: BoardState = {
                pieces: BoardPieces.fromPieces({
                    ...piece,
                    position: newPos,
                }),
                moveOptions: { legalMoves: new Map(), hasForcedMoves: false },
            };

            await store.getState().goToPosition(boardState);

            expect(playAnimationMock).toHaveBeenCalledExactlyOnceWith({
                newPieces: boardState.pieces,
                movedPieceIds: [piece.id],
                isCapture: false,
                isPromotion: false,
            });
        });

        it("should set isCapture to true if moveFromPreviousViewedPosition is a capture", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const newPos = logicalPoint({ x: 1, y: 1 });
            const playAnimationMock = vi.fn();

            store.setState({
                playAnimation: playAnimationMock,
                pieces: BoardPieces.fromPieces(piece),
            });

            const boardState: BoardState = {
                pieces: BoardPieces.fromPieces({
                    ...piece,
                    position: newPos,
                }),
                moveOptions: { legalMoves: new Map(), hasForcedMoves: false },
                moveFromPreviousViewedPosition: createFakeMove({
                    captures: [logicalPoint({ x: 1, y: 1 })],
                }),
            };

            await store.getState().goToPosition(boardState);
            expect(playAnimationMock).toHaveBeenCalledExactlyOnceWith({
                newPieces: boardState.pieces,
                movedPieceIds: [piece.id],
                isCapture: true,
                isPromotion: false,
                specialMoveType: null,
            });
        });

        it("should pass specialMoveType to playAnimation when present", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const newPos = logicalPoint({ x: 2, y: 2 });
            const playAnimationMock = vi.fn();

            store.setState({
                playAnimation: playAnimationMock,
                pieces: BoardPieces.fromPieces(piece),
            });

            const boardState: BoardState = {
                pieces: BoardPieces.fromPieces({
                    ...piece,
                    position: newPos,
                }),
                moveOptions: { legalMoves: new Map(), hasForcedMoves: false },
                moveFromPreviousViewedPosition: createFakeMove({
                    specialMoveType: SpecialMoveType.KNOOKLEAR_FUSION,
                }),
            };

            await store.getState().goToPosition(boardState);

            expect(playAnimationMock).toHaveBeenCalledExactlyOnceWith({
                newPieces: boardState.pieces,
                movedPieceIds: [piece.id],
                isCapture: false,
                isPromotion: false,
                specialMoveType: SpecialMoveType.KNOOKLEAR_FUSION,
            });
        });

        it("should set isPromotion=true when promotesTo is defined", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const newPos = logicalPoint({ x: 0, y: 7 });
            const playAnimationMock = vi.fn();

            store.setState({
                playAnimation: playAnimationMock,
                pieces: BoardPieces.fromPieces(piece),
            });

            const boardState: BoardState = {
                pieces: BoardPieces.fromPieces({
                    ...piece,
                    position: newPos,
                }),
                moveOptions: { legalMoves: new Map(), hasForcedMoves: false },
                moveFromPreviousViewedPosition: createFakeMove({
                    promotesTo: PieceType.QUEEN,
                }),
            };

            await store.getState().goToPosition(boardState);

            expect(playAnimationMock).toHaveBeenCalledExactlyOnceWith({
                newPieces: boardState.pieces,
                movedPieceIds: [piece.id],
                isCapture: false,
                isPromotion: true,
                specialMoveType: null,
            });
        });

        it("should set moveBounds when moveThatProducedPosition is defined", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const newPos = logicalPoint({ x: 0, y: 4 });
            const playAnimationMock = vi.fn();

            store.setState({
                playAnimation: playAnimationMock,
                pieces: BoardPieces.fromPieces(piece),
            });

            const boardState: BoardState = {
                pieces: BoardPieces.fromPieces({
                    ...piece,
                    position: newPos,
                }),
                moveOptions: { legalMoves: new Map(), hasForcedMoves: false },
                moveThatProducedPosition: createFakeMove({
                    from: piece.position,
                    to: newPos,
                }),
                moveFromPreviousViewedPosition: createFakeMove(),
            };

            await store.getState().goToPosition(boardState);

            expect(playAnimationMock).toHaveBeenCalledExactlyOnceWith({
                newPieces: boardState.pieces,
                movedPieceIds: [piece.id],
                moveBounds: { from: piece.position, to: newPos },
                isCapture: false,
                isPromotion: false,
                specialMoveType: null,
            });
        });
    });
});
