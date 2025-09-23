import { StoreApi } from "zustand";
import { MoveAnimation } from "../../lib/types";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import { logicalPoint } from "@/features/point/pointUtils";

describe("AnimationSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
        vi.useFakeTimers({ shouldAdvanceTime: true });
    });

    describe("playAnimationBatch", () => {
        it("should set animatingPieceMap and animatingPieces during animation and clear after", async () => {
            const pieceId = "1";
            const animation: MoveAnimation[] = [
                {
                    newPieces: new Map([[pieceId, createFakePiece()]]),
                    movedPieceIds: [pieceId],
                },
            ];

            const promise = store.getState().playAnimationBatch(animation);

            expect(store.getState().animatingPieceMap).toEqual(
                animation[0].newPieces,
            );
            expect(store.getState().animatingPieces).toEqual(new Set(pieceId));

            await promise;

            expect(store.getState().animatingPieceMap).toBeNull();
            expect(store.getState().animatingPieces.size).toBe(0);
        });

        it("should cancel a previous animation when a new one starts", async () => {
            const firstAnimation: MoveAnimation[] = [
                {
                    newPieces: new Map([["1", createFakePiece()]]),
                    movedPieceIds: ["1"],
                },
            ];
            const secondAnimation: MoveAnimation[] = [
                {
                    newPieces: new Map([["2", createFakePiece()]]),
                    movedPieceIds: ["2"],
                },
            ];

            const firstPromise = store
                .getState()
                .playAnimationBatch(firstAnimation);

            vi.advanceTimersByTime(50);

            const secondPromise = store
                .getState()
                .playAnimationBatch(secondAnimation);

            expect(store.getState().animatingPieceMap).toEqual(
                secondAnimation[0].newPieces,
            );

            vi.advanceTimersByTime(100);
            await Promise.all([firstPromise, secondPromise]);

            expect(store.getState().animatingPieceMap).toBeNull();
            expect(store.getState().animatingPieces.size).toBe(0);
        });
    });

    describe("animatePiece", () => {
        it("should animate a single piece and persist animatingPieceMap", async () => {
            const piece = createFakePiece();
            const pieceId = "1";
            const newPosition = logicalPoint({ x: 2, y: 2 });

            await store.getState().animatePiece(pieceId, piece, newPosition);

            const animatingMap = store.getState().animatingPieceMap!;
            expect(animatingMap.get(pieceId)?.position).toEqual(newPosition);

            expect(Array.from(store.getState().animatingPieces)).toEqual([]);
        });
    });

    describe("clearAnimation", () => {
        it("should clear animatingPieceMap", () => {
            const piece = createFakePiece();
            const pieceId = "1";

            store.setState({
                animatingPieceMap: new Map([[pieceId, piece]]),
            });

            store.getState().clearAnimation();

            expect(store.getState().animatingPieceMap).toBeNull();
        });
    });
});
