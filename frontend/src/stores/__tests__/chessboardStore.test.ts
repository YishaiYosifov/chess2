import { GameColor } from "@/lib/apiClient";
import {
    type PieceID,
    type Move,
    LegalMoveMap,
    PieceMap,
    Point,
    Piece,
} from "@/types/tempModels"; // adjust paths
import {
    createChessboardStore,
    defaultChessboardState,
} from "../chessboardStore";
import {
    createLegalMoveMap,
    createMove,
    createPiece,
    createPieceMap,
    createPieceMapFromPieces,
    createUniquePoint,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { pointToStr } from "@/lib/utils/pointUtils";

describe("ChessboardStore", () => {
    let store: ReturnType<typeof createChessboardStore>;

    beforeEach(() => {
        vi.useFakeTimers();
        store = createChessboardStore();
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

    it("should initialize with default values", () => {
        const state = store.getState();
        expect(state).toMatchObject(defaultChessboardState);
    });

    describe("playTurn", () => {
        it("should apply move if provided and update legal moves and sideToMove", () => {
            const piece = createPiece();
            const move = createMove({ from: piece.position });
            const pieces: PieceMap = new Map([["0", piece]]);
            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(piece.position), [move]],
            ]);
            store.setState({
                pieces,
                legalMoves,
                highlightedLegalMoves: [createUniquePoint()],
                selectedPieceId: "0",
            });

            const newLegalMoves = new Map();
            const sideToMove = GameColor.BLACK;
            store.getState().playTurn(newLegalMoves, sideToMove, move);

            expectPieces({ id: "0", position: move.to, piece });
            const state = store.getState();
            expect(state.legalMoves).toBe(newLegalMoves);
            expect(state.highlightedLegalMoves).toEqual([]);
            expect(state.selectedPieceId).toBeUndefined();
            expect(state.sideToMove).toBe(sideToMove);
        });

        it("should update legal moves and sideToMove without applying move if none provided", () => {
            const newLegalMoves = new Map();
            const sideToMove = GameColor.BLACK;

            store.getState().playTurn(newLegalMoves, sideToMove);

            const state = store.getState();
            expect(state.legalMoves).toBe(newLegalMoves);
            expect(state.sideToMove).toBe(sideToMove);
        });
    });

    describe("playMove", () => {
        it("should warn and return if no piece at move.from", () => {
            const pieces = createPieceMap();
            store.setState({ pieces });

            const move: Move = createMove({ from: { x: 69, y: 420 } });
            store.getState().playMove(move);

            const newPieces = store.getState().pieces;
            expect(newPieces).toEqual(pieces);
        });

        it("should move piece, delete captured pieces and handle sideEffects", () => {
            const piece = createPiece();
            const capturedPiece = createPiece();
            const sideEffectPiece = createPiece();
            const sideEffectCapturedPiece = createPiece();

            store.setState({
                pieces: createPieceMapFromPieces(
                    piece,
                    capturedPiece,
                    sideEffectPiece,
                    sideEffectCapturedPiece,
                ),
            });

            const sideEffectMove = createMove({
                from: sideEffectPiece.position,
                captures: [sideEffectCapturedPiece.position],
            });

            const move = createMove({
                from: piece.position,
                captures: [capturedPiece.position],
                sideEffects: [sideEffectMove],
            });

            store.getState().playMove(move);

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

    describe("moveSelectedPiece", () => {
        it("should not move if no piece is selected", async () => {
            const pieces = createPieceMap();
            store.setState({ pieces });

            store.getState().moveSelectedPiece({ x: 6, y: 9 });

            const newPieces = store.getState().pieces;
            expect(newPieces).toEqual(pieces);
        });

        it("should not move if no legal move is found", async () => {
            const piece = createPiece();
            const pieces: PieceMap = new Map([["1", createPiece()]]);
            const legalMoves = createLegalMoveMap(piece);

            store.setState({
                selectedPieceId: "1",
                pieces,
                legalMoves,
            });

            await store.getState().moveSelectedPiece(createUniquePoint());

            const newPieces = store.getState().pieces;
            expect(newPieces).toEqual(pieces);
        });

        it("should move the piece if the move is found", async () => {
            const piece = createPiece();
            const move = createMove({ from: piece.position });
            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(piece.position), [move]],
            ]);

            store.setState({
                selectedPieceId: "0",
                pieces: new Map([["0", piece]]),
                legalMoves,
            });

            await store.getState().moveSelectedPiece(move.to);

            expectPieces({ id: "0", position: move.to, piece });
        });
    });

    describe("handlePieceDrop", () => {
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
                const piece = createPiece();
                const move = createMove({
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

                await store
                    .getState()
                    .handlePieceDrop(mousePosition.x, mousePosition.y);

                expectPieces({ id: "0", position: expectedPosition, piece });
            },
        );
    });

    describe("position2Id", () => {
        it("should return piece ID if piece found at position", () => {
            const piece = createPiece();
            store.setState({
                pieces: new Map([["0", piece]]),
            });

            const id = store.getState().position2Id(piece.position);
            expect(id).toBe("0");
        });

        it("should return undefined if no piece at position", () => {
            store.setState({
                pieces: new Map([["0", createPiece()]]),
            });
            const id = store.getState().position2Id({ x: 69, y: 420 });
            expect(id).toBeUndefined();
        });
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

        it("should set highlightedLegalMoves and selectedPieceId correctly", () => {
            const piece1 = createPiece();
            const piece1Move1 = createMove({
                from: piece1.position,
            });
            const piece1Move2 = createMove({
                from: piece1.position,
            });

            const piece2 = createPiece();
            const piece2Move = createMove({ from: piece2.position });

            const legalMoves: LegalMoveMap = new Map([
                [pointToStr(piece1.position), [piece1Move1, piece1Move2]],
                [pointToStr(piece2.position), [piece2Move]],
            ]);
            store.setState({
                pieces: createPieceMapFromPieces(piece1, piece2),
                legalMoves,
            });

            store.getState().showLegalMoves("0");

            const state = store.getState();
            expect(state.selectedPieceId).toBe("0");
            expect(state.highlightedLegalMoves).toEqual([
                piece1Move1.to,
                piece1Move2.to,
                ...piece1Move1.through,
                ...piece1Move2.through,
            ]);
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
        it("should reset state and set new pieces, legalMoves, and sideToMove", () => {
            const piece = createPiece();
            const pieces: PieceMap = new Map([["0", piece]]);
            const legalMoves = createLegalMoveMap(piece);
            const sideToMove = GameColor.BLACK;

            store.getState().resetState(pieces, legalMoves, sideToMove);

            const state = store.getState();
            expect(state).toMatchObject({
                ...defaultChessboardState,
                pieces,
                legalMoves,
                sideToMove,
            });
        });
    });
});
