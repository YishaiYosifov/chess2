import { StoreApi } from "zustand";
import { ChessboardState, createChessboardStore } from "../chessboardStore";
import {
    createFakeLegalMoveMap,
    createFakeMove,
    createFakePiece,
    createFakePieceMap,
    createFakePieceMapFromPieces,
    createUniquePoint,
} from "@/lib/testUtils/fakers/chessboardFakers";
import {
    LegalMoveMap,
    Piece,
    PieceID,
    PieceMap,
    Point,
} from "@/types/tempModels";
import { pointToStr } from "@/lib/utils/pointUtils";
import { GameColor } from "@/lib/apiClient";

describe("PiecesSlice", () => {
    let store: StoreApi<ChessboardState>;

    beforeEach(() => {
        store = createChessboardStore();
        vi.useFakeTimers();
    });

    function expectPieces(
        ...pieces: { id: PieceID; position: Point; piece: Piece }[]
    ) {
        const expectedPieces: PieceMap = new Map();
        for (const { id, position, piece } of pieces) {
            expectedPieces.set(id, { ...piece, position });
        }

        const newPieces = store.getState().pieces;
        expect(newPieces).toEqual(expectedPieces);
    }

    describe("playMove", () => {
        it("should return if no piece at move.from", () => {
            const pieces = createFakePieceMap();
            store.setState({ pieces });

            const move = createFakeMove({ from: { x: 69, y: 420 } });
            store.getState().applyMove(move);

            const newPieces = store.getState().pieces;
            expect(newPieces).toEqual(pieces);
        });

        it("should move piece, delete captured pieces and handle sideEffects", () => {
            const piece = createFakePiece();
            const capturedPiece = createFakePiece();
            const sideEffectPiece = createFakePiece();

            store.setState({
                pieces: createFakePieceMapFromPieces(
                    piece,
                    capturedPiece,
                    sideEffectPiece,
                ),
            });

            const sideEffectMove = createFakeMove({
                from: sideEffectPiece.position,
            });

            const move = createFakeMove({
                from: piece.position,
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

            store.getState().tryApplySelectedMove({ x: 6, y: 9 });

            const newPieces = store.getState().pieces;
            expect(newPieces).toEqual(pieces);
        });

        it("should not move if no legal move is found", async () => {
            const piece = createFakePiece();
            const pieces: PieceMap = new Map([["1", createFakePiece()]]);
            const legalMoves = createFakeLegalMoveMap(piece);

            store.setState({
                selectedPieceId: "1",
                pieces,
                legalMoves,
            });

            await store.getState().tryApplySelectedMove(createUniquePoint());

            const newPieces = store.getState().pieces;
            expect(newPieces).toEqual(pieces);
        });

        it("should move the piece if the move is found", async () => {
            const piece = createFakePiece();
            const move = createFakeMove({ from: piece.position });
            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(piece.position), [move]],
            ]);

            store.setState({
                selectedPieceId: "0",
                pieces: new Map([["0", piece]]),
                legalMoves,
            });

            await store.getState().tryApplySelectedMove(move.to);

            expectPieces({ id: "0", position: move.to, piece });
            const {
                legalMoves: newLegalMoves,
                highlightedLegalMoves,
                selectedPieceId,
            } = store.getState();
            expect(newLegalMoves).toEqual(new Map());
            expect(highlightedLegalMoves.length).toBe(0);
            expect(selectedPieceId).toBeNull();
        });
    });

    describe("handleMousePieceDrop", () => {
        it.each([
            [GameColor.WHITE, { x: 2, y: 7 }, { x: 20, y: 20 }],
            [GameColor.BLACK, { x: 7, y: 2 }, { x: 20, y: 20 }],
        ])(
            "should move the selected piece based on drop coordinates depending on viewingFrom",
            async (
                viewingFrom: GameColor,
                expectedPosition: Point,
                mousePosition: Point,
            ) => {
                const piece = createFakePiece();
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
                    legalMoves: legalMoves,
                });

                await store.getState().handleMousePieceDrop({
                    mousePoint: {
                        x: mousePosition.x,
                        y: mousePosition.y,
                    },
                    isDrag: false,
                });

                expectPieces({ id: "0", position: expectedPosition, piece });
            },
        );
    });

    describe("pointToPiece", () => {
        it("should return piece ID if piece found at position", () => {
            const piece = createFakePiece();
            store.setState({
                pieces: new Map([["0", piece]]),
            });

            const id = store.getState().pointToPiece(piece.position);
            expect(id).toBe("0");
        });

        it("should return undefined if no piece at position", () => {
            store.setState({
                pieces: new Map([["0", createFakePiece()]]),
            });
            const id = store.getState().pointToPiece({ x: 69, y: 420 });
            expect(id).toBeUndefined();
        });
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
