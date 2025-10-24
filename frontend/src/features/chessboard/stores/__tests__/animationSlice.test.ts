import { StoreApi } from "zustand";
import { AnimationStep, MoveAnimation, PieceMap } from "../../lib/types";
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
            const animation: MoveAnimation = {
                steps: [
                    {
                        newPieces: new Map([[pieceId, createFakePiece()]]),
                        movedPieceIds: [pieceId],
                    },
                ],
                removedPieceIds: [],
            };

            const promise = store.getState().playAnimationBatch(animation);

            expect(store.getState().animatingPieceMap).toEqual(
                animation.steps[0].newPieces,
            );
            expect(store.getState().animatingPieces).toEqual(new Set(pieceId));

            vi.advanceTimersByTime(100);
            await promise;

            expect(store.getState().animatingPieceMap).toBeNull();
            expect(store.getState().animatingPieces.size).toBe(0);
        });

        it("should cancel a previous animation when a new one starts", async () => {
            const firstAnimation: MoveAnimation = {
                steps: [
                    {
                        newPieces: new Map([["1", createFakePiece()]]),
                        movedPieceIds: ["1"],
                    },
                ],
                removedPieceIds: [],
            };
            const secondAnimation: MoveAnimation = {
                steps: [
                    {
                        newPieces: new Map([["2", createFakePiece()]]),
                        movedPieceIds: ["2"],
                    },
                ],
                removedPieceIds: [],
            };

            const firstPromise = store
                .getState()
                .playAnimationBatch(firstAnimation);

            vi.advanceTimersByTime(50);

            const secondPromise = store
                .getState()
                .playAnimationBatch(secondAnimation);

            expect(store.getState().animatingPieceMap).toEqual(
                secondAnimation.steps[0].newPieces,
            );

            vi.advanceTimersByTime(100);
            await Promise.all([firstPromise, secondPromise]);

            expect(store.getState().animatingPieceMap).toBeNull();
            expect(store.getState().animatingPieces.size).toBe(0);
        });

        it("should handle removedPieceIds correctly", async () => {
            const pieceId = "1";
            const removedPieceId = "2";
            const animation: MoveAnimation = {
                steps: [
                    {
                        newPieces: new Map([[pieceId, createFakePiece()]]),
                        movedPieceIds: [pieceId],
                    },
                ],
                removedPieceIds: [removedPieceId],
            };

            const promise = store.getState().playAnimationBatch(animation);

            expect(store.getState().removingPieces).toEqual(new Set("2"));

            vi.advanceTimersByTime(100);
            await promise;

            expect(store.getState().removingPieces).toEqual(new Set());
        });

        it("should display initialSpawnPositions before showing newPieces", async () => {
            const pieceId = "1";
            const spawnedPieceId = "spawn1";

            const initialSpawnPositions = new Map([
                [pieceId, createFakePiece()],
                [spawnedPieceId, createFakePiece()],
            ]);

            const newPieces = new Map([
                [pieceId, createFakePiece()],
                [spawnedPieceId, createFakePiece()],
            ]);

            const animation: MoveAnimation = {
                steps: [
                    {
                        newPieces,
                        movedPieceIds: [pieceId, spawnedPieceId],
                        initialSpawnPositions,
                    },
                ],
                removedPieceIds: [],
            };

            const promise = store.getState().playAnimationBatch(animation);

            expect(store.getState().animatingPieceMap).toEqual(
                initialSpawnPositions,
            );

            await vi.runAllTimersAsync();
            await promise;

            expect(store.getState().animatingPieceMap).toBeNull();
        });
    });

    describe("playAnimation", () => {
        it("should set animatingPieceMap and animatingPieces for a single-step animation and clear after", async () => {
            const pieceId = "1";
            const animationStep: AnimationStep = {
                newPieces: new Map([[pieceId, createFakePiece()]]),
                movedPieceIds: [pieceId],
            };

            const promise = store.getState().playAnimation(animationStep);

            expect(store.getState().animatingPieceMap).toEqual(
                animationStep.newPieces,
            );
            expect(store.getState().animatingPieces).toEqual(
                new Set([pieceId]),
            );

            vi.advanceTimersByTime(100);
            await promise;

            expect(store.getState().animatingPieceMap).toBeNull();
            expect(store.getState().animatingPieces.size).toBe(0);
        });
    });

    describe("animatePiece", () => {
        it("should animate a single piece and persist animatingPieceMap", async () => {
            const piece = createFakePiece();
            const pieceId = "1";
            const newPosition = logicalPoint({ x: 2, y: 2 });
            const pieceMap: PieceMap = new Map([[pieceId, piece]]);

            await store
                .getState()
                .animatePiece(pieceId, newPosition, new Map(pieceMap));

            pieceMap.set(pieceId, { ...piece, position: newPosition });
            const animatingMap = store.getState().animatingPieceMap!;
            expect(animatingMap).toEqual(pieceMap);

            expect(store.getState().animatingPieces.size).toBe(0);
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
