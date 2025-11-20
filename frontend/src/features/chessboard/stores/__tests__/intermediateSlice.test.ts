import { logicalPoint, sortPoints } from "@/features/point/pointUtils";
import {
    createFakeMove,
    createFakeBoardPieces,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { LogicalPoint } from "@/features/point/types";
import { StoreApi } from "zustand";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { PieceID } from "../../lib/types";
import { PieceType } from "@/lib/apiClient";

describe("IntermediateSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
        vi.useFakeTimers({ shouldAdvanceTime: true });
    });

    function expectNextIntermediates(...expected: LogicalPoint[]) {
        const actual = store.getState().nextIntermediates;
        expect(sortPoints(actual)).toEqual(sortPoints(expected));
    }

    function expectVisitedAnimating(
        pieceId: PieceID,
        expectedPosition: LogicalPoint,
    ) {
        const animatingPiece = store
            .getState()
            .animatingPieces?.getById(pieceId);
        expect(animatingPiece?.position).toEqual(expectedPosition);
    }

    it("should return null if no moves", async () => {
        const result = await store
            .getState()
            .disambiguateDestination(
                logicalPoint({ x: 0, y: 0 }),
                [],
                "0",
                createFakeBoardPieces(1),
            );
        expect(result).toBeNull();
    });

    it("should return single move immediately if only one move with no intermediates", async () => {
        const move = createFakeMove({
            to: logicalPoint({ x: 1, y: 1 }),
        });

        const result = await store
            .getState()
            .disambiguateDestination(
                move.to,
                [move],
                "0",
                createFakeBoardPieces(1),
            );
        expect(result).toEqual([move]);
        expect(store.getState().nextIntermediates).toEqual([]);
    });

    it("should return all moves if moves are indistinguishable", async () => {
        const moves = [
            createFakeMove({
                intermediates: [
                    {
                        position: logicalPoint({ x: 1, y: 1 }),
                        isCapture: false,
                    },
                ],
                to: logicalPoint({ x: 3, y: 3 }),
            }),
            createFakeMove({
                intermediates: [
                    {
                        position: logicalPoint({ x: 1, y: 1 }),
                        isCapture: false,
                    },
                ],
                to: logicalPoint({ x: 3, y: 3 }),
                promotesTo: PieceType.QUEEN,
            }),
        ];

        const result = await store
            .getState()
            .disambiguateDestination(
                logicalPoint({ x: 1, y: 1 }),
                moves,
                "0",
                createFakeBoardPieces(1),
            );
        expect(result).toEqual(moves);
    });

    it("should compute nextIntermediates and resolve choice correctly", async () => {
        const move1 = createFakeMove({
            intermediates: [
                { position: logicalPoint({ x: 1, y: 0 }), isCapture: false },
                { position: logicalPoint({ x: 2, y: 0 }), isCapture: false },
            ],
            to: logicalPoint({ x: 3, y: 3 }),
        });
        const move2 = createFakeMove({
            intermediates: [
                { position: logicalPoint({ x: 1, y: 0 }), isCapture: false },
                { position: logicalPoint({ x: 2, y: 1 }), isCapture: false },
            ],
            to: logicalPoint({ x: 3, y: 4 }),
        });

        const promise = store
            .getState()
            .disambiguateDestination(
                logicalPoint({ x: 1, y: 0 }),
                [move1, move2],
                "0",
                createFakeBoardPieces(1),
            );

        // nextIntermediates should include all unique next points
        expectNextIntermediates(
            logicalPoint({ x: 2, y: 0 }),
            logicalPoint({ x: 2, y: 1 }),
        );

        const resolve = store.getState().resolveNextIntermediate!;
        resolve(logicalPoint({ x: 2, y: 0 }));

        const result = await promise;
        // moves filtered by visited path
        expect(result).toEqual([move1]);
        expect(store.getState().nextIntermediates).toEqual([]);
    });

    it("should cancel correctly when user resolves null", async () => {
        const move1 = createFakeMove({
            intermediates: [
                { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
                { position: logicalPoint({ x: 2, y: 1 }), isCapture: false },
            ],
            to: logicalPoint({ x: 3, y: 1 }),
        });
        const move2 = createFakeMove({
            intermediates: [
                { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
                { position: logicalPoint({ x: 2, y: 2 }), isCapture: false },
            ],
            to: logicalPoint({ x: 3, y: 2 }),
        });

        const promise = store
            .getState()
            .disambiguateDestination(
                logicalPoint({ x: 1, y: 1 }),
                [move1, move2],
                "0",
                createFakeBoardPieces(1),
            );

        const resolve = store.getState().resolveNextIntermediate!;
        resolve(null);

        const result = await promise;
        expect(result).toBeNull();
        expect(store.getState().nextIntermediates).toEqual([]);
        expect(store.getState().resolveNextIntermediate).toBeNull();
    });

    it("should return moves ending at dest when user clicks dest", async () => {
        const move1 = createFakeMove({
            to: logicalPoint({ x: 1, y: 1 }),
        });
        const move2 = createFakeMove({
            intermediates: [
                { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
            ],
            to: logicalPoint({ x: 2, y: 2 }),
        });

        const promise = store
            .getState()
            .disambiguateDestination(
                logicalPoint({ x: 1, y: 1 }),
                [move1, move2],
                "0",
                createFakeBoardPieces(1),
            );

        const resolve = store.getState().resolveNextIntermediate!;
        resolve(logicalPoint({ x: 1, y: 1 }));

        const result = await promise;
        expect(result).toEqual([move1]);
    });

    it("should update animatingPieces during selection", async () => {
        const move1 = createFakeMove({
            intermediates: [
                { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
                { position: logicalPoint({ x: 2, y: 0 }), isCapture: false },
            ],
            to: logicalPoint({ x: 3, y: 0 }),
        });
        const move2 = createFakeMove({
            intermediates: [
                { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
                { position: logicalPoint({ x: 2, y: 1 }), isCapture: false },
            ],
            to: logicalPoint({ x: 3, y: 1 }),
        });

        const promise = store
            .getState()
            .disambiguateDestination(
                logicalPoint({ x: 1, y: 1 }),
                [move1, move2],
                "0",
                createFakeBoardPieces(1),
            );

        expectVisitedAnimating("0", logicalPoint({ x: 1, y: 1 }));

        const resolve = store.getState().resolveNextIntermediate!;
        resolve(logicalPoint({ x: 2, y: 0 }));

        await promise;

        expect(store.getState().resolveNextIntermediate).toBeNull();
    });

    it("should respect trigger points as valid moves", async () => {
        const move = createFakeMove({
            triggers: [logicalPoint({ x: 1, y: 1 })],
            to: logicalPoint({ x: 2, y: 2 }),
        });

        const result = await store
            .getState()
            .disambiguateDestination(
                logicalPoint({ x: 1, y: 1 }),
                [move],
                "0",
                createFakeBoardPieces(1),
            );
        expect(result).toEqual([move]);
    });
});
