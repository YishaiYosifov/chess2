import { StoreApi } from "zustand";
import { AnimationStep, MoveAnimation } from "../../lib/types";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import { logicalPoint } from "@/features/point/pointUtils";
import BoardPieces from "../../lib/boardPieces";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";

describe("AnimationSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
        vi.useFakeTimers({ shouldAdvanceTime: true });
    });

    describe("playAnimationBatch", () => {
        it("should set animatingPieces and animatingPieces during animation and clear after", async () => {
            const piece = createFakePiece();
            const animation: MoveAnimation = {
                steps: [
                    {
                        newPieces: BoardPieces.fromPieces(piece),
                        movedPieceIds: [piece.id],
                        isCapture: false,
                    },
                ],
                removedPieceIds: [],
            };

            const promise = store.getState().playAnimationBatch(animation);

            expect(store.getState().animatingPieces).toEqual(
                animation.steps[0].newPieces,
            );
            expect(store.getState().animatingPieceIds).toEqual(
                new Set([piece.id]),
            );

            vi.advanceTimersByTime(100);
            await promise;

            expect(store.getState().animatingPieces).toBeNull();
            expect(store.getState().animatingPieceIds.size).toBe(0);
        });

        it("should cancel a previous animation when a new one starts", async () => {
            const piece1 = createFakePiece();
            const piece2 = createFakePiece();
            const firstAnimation: MoveAnimation = {
                steps: [
                    {
                        newPieces: BoardPieces.fromPieces(piece1),
                        movedPieceIds: [piece1.id],
                        isCapture: false,
                    },
                ],
                removedPieceIds: [],
            };
            const secondAnimation: MoveAnimation = {
                steps: [
                    {
                        newPieces: BoardPieces.fromPieces(piece2),
                        movedPieceIds: [piece2.id],
                        isCapture: false,
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

            expect(store.getState().animatingPieces).toEqual(
                secondAnimation.steps[0].newPieces,
            );

            vi.advanceTimersByTime(100);
            await Promise.all([firstPromise, secondPromise]);

            expect(store.getState().animatingPieces).toBeNull();
            expect(store.getState().animatingPieceIds.size).toBe(0);
        });

        it("should handle removedPieceIds correctly", async () => {
            const movingPiece = createFakePiece();
            const removedPiece = createFakePiece();
            const animation: MoveAnimation = {
                steps: [
                    {
                        newPieces: BoardPieces.fromPieces(movingPiece),
                        movedPieceIds: [movingPiece.id],
                        isCapture: true,
                    },
                ],
                removedPieceIds: [removedPiece.id],
            };

            const promise = store.getState().playAnimationBatch(animation);

            expect(store.getState().removingPieceIds).toEqual(
                new Set([removedPiece.id]),
            );

            vi.advanceTimersByTime(100);
            await promise;

            expect(store.getState().removingPieceIds).toEqual(new Set());
        });

        it("should display initialSpawnPositions before showing newPieces", async () => {
            const movingPieceId = "moving";
            const spawnedPieceId = "spawn1";

            const initialSpawnPositions = BoardPieces.fromPieces(
                createFakePiece({ id: movingPieceId }),
                createFakePiece({ id: spawnedPieceId }),
            );

            const newPieces = BoardPieces.fromPieces(
                createFakePiece({ id: movingPieceId }),
                createFakePiece({ id: spawnedPieceId }),
            );

            const animation: MoveAnimation = {
                steps: [
                    {
                        newPieces,
                        movedPieceIds: [movingPieceId, spawnedPieceId],
                        initialSpawnPositions,
                        isCapture: false,
                    },
                ],
                removedPieceIds: [],
            };

            const promise = store.getState().playAnimationBatch(animation);

            expect(store.getState().animatingPieces).toEqual(
                initialSpawnPositions,
            );

            await vi.runAllTimersAsync();
            await promise;

            expect(store.getState().animatingPieces).toBeNull();
        });

        it("should play audio for each animation step", async () => {
            const playAudioForAnimationStepMock = vi.fn();
            store.setState({
                playAudioForAnimationStep: playAudioForAnimationStepMock,
            });
            const piece = createFakePiece();
            const animation: MoveAnimation = {
                steps: [
                    {
                        newPieces: BoardPieces.fromPieces(piece),
                        movedPieceIds: [piece.id],
                        isCapture: false,
                    },
                    {
                        newPieces: BoardPieces.fromPieces(piece),
                        movedPieceIds: [piece.id],
                        isCapture: true,
                    },
                ],
                removedPieceIds: [],
            };

            const promise = store.getState().playAnimationBatch(animation);

            expect(
                playAudioForAnimationStepMock,
            ).toHaveBeenCalledExactlyOnceWith(animation.steps[0]);

            vi.advanceTimersByTime(100);
            await flushMicrotasks();
            expect(playAudioForAnimationStepMock).toHaveBeenCalledTimes(2);
            expect(playAudioForAnimationStepMock).toHaveBeenCalledWith(
                animation.steps[1],
            );

            await promise;
        });
    });

    describe("playAnimation", () => {
        it("should set animatingPieces and animatingPieceIds for a single-step animation and clear after", async () => {
            const piece = createFakePiece();
            const animationStep: AnimationStep = {
                newPieces: BoardPieces.fromPieces(piece),
                movedPieceIds: [piece.id],
                isCapture: false,
            };

            const promise = store.getState().playAnimation(animationStep);

            expect(store.getState().animatingPieces).toEqual(
                animationStep.newPieces,
            );
            expect(store.getState().animatingPieceIds).toEqual(
                new Set([piece.id]),
            );

            vi.advanceTimersByTime(100);
            await promise;

            expect(store.getState().animatingPieces).toBeNull();
            expect(store.getState().animatingPieceIds.size).toBe(0);
        });
    });

    describe("animatePiece", () => {
        it("should animate a single piece and persist animatingPieces", async () => {
            const piece = createFakePiece();
            const newPosition = logicalPoint({ x: 2, y: 2 });
            const pieces = BoardPieces.fromPieces(piece);

            await store
                .getState()
                .animatePiece(piece.id, newPosition, new BoardPieces(pieces));

            pieces.addAt(piece, newPosition);
            const animatingMap = store.getState().animatingPieces;
            expect(animatingMap).toEqual(pieces);

            expect(store.getState().animatingPieceIds.size).toBe(0);
        });
    });

    describe("clearAnimation", () => {
        it("should clear animatingPieces", () => {
            const piece = createFakePiece();

            store.setState({
                animatingPieces: BoardPieces.fromPieces(piece),
            });

            store.getState().clearAnimation();

            expect(store.getState().animatingPieces).toBeNull();
        });
    });
});
