import { StateCreator } from "zustand";
import type { ChessboardStore } from "./chessboardStore";
import { AnimationStep, MoveAnimation, PieceID, PieceMap } from "../lib/types";
import { LogicalPoint } from "@/features/point/types";

export interface AnimationSlice {
    animatingPieceMap: PieceMap | null;
    animatingPieces: Set<PieceID>;
    removingPieces: Set<PieceID>;

    playAnimationBatch(animation: MoveAnimation): Promise<void>;
    playAnimation(animation: AnimationStep): Promise<void>;
    animatePiece(
        pieceId: PieceID,
        newPosition: LogicalPoint,
        pieceMap: PieceMap,
    ): Promise<void>;
    clearAnimation(): void;
}
export const createAnimationSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    AnimationSlice
> = (set, get) => {
    let currentAnimationCancelToken: { canceled: boolean } | null = null;

    async function processMoveAnimation(
        animation: MoveAnimation,
        persistent: boolean = false,
    ) {
        const { playAudioForAnimationStep } = get();

        if (currentAnimationCancelToken) {
            currentAnimationCancelToken.canceled = true;
        }

        const cancelToken = { canceled: false };
        currentAnimationCancelToken = cancelToken;

        set((state) => {
            state.removingPieces = new Set(animation.removedPieceIds);
        });

        for (const step of animation.steps) {
            if (cancelToken.canceled) break;

            playAudioForAnimationStep(step);
            if (step.initialSpawnPositions)
                await spawnPieces(step.initialSpawnPositions);
            set((state) => {
                state.animatingPieceMap = step.newPieces;
            });
            await markPiecesAsAnimating(step.movedPieceIds);
        }

        if (!cancelToken.canceled && !persistent) {
            set((state) => {
                state.animatingPieceMap = null;
                state.removingPieces = new Set();
            });
        }

        if (currentAnimationCancelToken === cancelToken) {
            currentAnimationCancelToken = null;
        }
    }

    async function spawnPieces(initialSpawnPositions: PieceMap): Promise<void> {
        set((state) => {
            state.animatingPieceMap = initialSpawnPositions;
        });
        await new Promise<void>((resolve) => setTimeout(resolve));
    }

    async function markPiecesAsAnimating(pieceIds: Iterable<PieceID>) {
        set((state) => {
            for (const pieceId of pieceIds) state.animatingPieces.add(pieceId);
        });

        await new Promise<void>((resolve) =>
            setTimeout(() => {
                set((state) => {
                    for (const pieceId of pieceIds)
                        state.animatingPieces.delete(pieceId);
                });
                resolve();
            }, 100),
        );
    }

    return {
        animatingPieceMap: null,
        animatingPieces: new Set(),
        removingPieces: new Set(),

        async playAnimationBatch(animation) {
            await processMoveAnimation(animation);
        },

        async playAnimation(animation) {
            await processMoveAnimation({
                steps: [animation],
                removedPieceIds: [],
            });
        },

        async animatePiece(pieceId, newPosition, pieceMap) {
            const newPieces = new Map(pieceMap);
            const piece = newPieces.get(pieceId);
            if (!piece) return;

            newPieces.set(pieceId, { ...piece, position: newPosition });
            await processMoveAnimation(
                {
                    steps: [
                        {
                            newPieces,
                            movedPieceIds: [pieceId],
                            isCapture: false,
                        },
                    ],
                    removedPieceIds: [],
                },
                true,
            );
        },

        clearAnimation() {
            set((state) => {
                state.animatingPieceMap = null;
            });
        },
    };
};
