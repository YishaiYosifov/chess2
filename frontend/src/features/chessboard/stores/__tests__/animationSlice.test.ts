import { StoreApi } from "zustand";
import { PieceID, PieceMap } from "../../lib/types";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import { logicalPoint } from "@/features/point/pointUtils";

describe("AnimationSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
        vi.useFakeTimers({ shouldAdvanceTime: true });
    });

    function expectAnimatingPieces(...ids: PieceID[]) {
        const animatingSet = store.getState().animatingPieces;
        expect(Array.from(animatingSet).sort()).toEqual(ids.sort());
    }

    describe("addAnimatingPiece", () => {
        it("should add pieceId to animatingPieces and remove it after 100ms", async () => {
            const pieceId = "1";

            store.getState().addAnimatingPiece(pieceId);
            expectAnimatingPieces(pieceId);

            vi.advanceTimersByTime(100);
            expectAnimatingPieces();
        });

        it("should not add the same pieceId multiple times", async () => {
            const pieceId = "1";

            store.getState().addAnimatingPiece(pieceId);
            store.getState().addAnimatingPiece(pieceId);

            expect(store.getState().animatingPieces.size).toBe(1);
        });
    });

    describe("setAnimatingPieceMap", () => {
        it("should set animatingPieceMap and call addAnimatingPiece for each moved piece", async () => {
            const pieceA = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const pieceB = createFakePiece({
                position: logicalPoint({ x: 1, y: 1 }),
            });

            const map: PieceMap = new Map([
                ["0", pieceA],
                ["1", pieceB],
            ]);

            await store
                .getState()
                .setAnimatingPieceMap(map, new Set(["0", "1"]));

            expect(store.getState().animatingPieceMap).toEqual(map);
            vi.advanceTimersByTime(100);
            expect(store.getState().animatingPieces.size).toBe(0);
        });
    });

    describe("animatePiece", () => {
        it("should set animatingPieceMap for a single piece and add it to animatingPieces", async () => {
            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const newPosition = logicalPoint({ x: 2, y: 2 });

            await store.getState().animatePiece("0", piece, newPosition);

            expect(
                store.getState().animatingPieceMap!.get("0")!.position,
            ).toEqual(newPosition);
            vi.advanceTimersByTime(100);
            expectAnimatingPieces();
        });

        it("should initialize animatingPieceMap if it was null", async () => {
            expect(store.getState().animatingPieceMap).toBeNull();

            const piece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            await store
                .getState()
                .animatePiece("0", piece, logicalPoint({ x: 1, y: 1 }));

            expect(store.getState().animatingPieceMap).not.toBeNull();
            expect(
                store.getState().animatingPieceMap!.get("0")!.position,
            ).toEqual(logicalPoint({ x: 1, y: 1 }));
        });
    });

    describe("clearAnimation", () => {
        it("should clear animatingPieceMap", () => {
            const pieceMap: PieceMap = new Map([
                [
                    "0",
                    createFakePiece({ position: logicalPoint({ x: 0, y: 0 }) }),
                ],
            ]);
            store.setState({ animatingPieceMap: pieceMap });

            store.getState().clearAnimation();
            expect(store.getState().animatingPieceMap).toBeNull();
        });
    });
});
