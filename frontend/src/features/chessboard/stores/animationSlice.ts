import { StateCreator } from "zustand";
import type { ChessboardStore } from "./chessboardStore";
import { AnimationStep, MoveAnimation, PieceID } from "../lib/types";
import { LogicalPoint } from "@/features/point/types";
import BoardPieces from "../lib/boardPieces";

export interface AnimationSlice {
    animatingPieces: BoardPieces | null;
    animatingPieceIds: Set<PieceID>;
    removingPieceIds: Set<PieceID>;

    playAnimationBatch(animation: MoveAnimation): Promise<void>;
    playAnimation(animation: AnimationStep): Promise<void>;
    animatePiece(
        pieceId: PieceID,
        newPosition: LogicalPoint,
        pieces: BoardPieces,
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
            state.removingPieceIds = new Set(animation.removedPieceIds);
        });

        for (const step of animation.steps) {
            if (cancelToken.canceled) break;

            playAudioForAnimationStep(step);
            if (step.initialSpawnPositions)
                await spawnPieces(step.initialSpawnPositions);
            set((state) => {
                state.animatingPieces = step.newPieces;
            });
            await markPiecesAsAnimating(step.movedPieceIds);
        }

        if (!cancelToken.canceled && !persistent) {
            set((state) => {
                state.animatingPieces = null;
                state.removingPieceIds = new Set();
            });
        }

        if (currentAnimationCancelToken === cancelToken) {
            currentAnimationCancelToken = null;
        }
    }

    async function spawnPieces(
        initialSpawnPositions: BoardPieces,
    ): Promise<void> {
        set((state) => {
            state.animatingPieces = initialSpawnPositions;
        });
        await new Promise<void>((resolve) => setTimeout(resolve));
    }

    async function markPiecesAsAnimating(pieceIds: Iterable<PieceID>) {
        set((state) => {
            for (const pieceId of pieceIds)
                state.animatingPieceIds.add(pieceId);
        });

        await new Promise<void>((resolve) =>
            setTimeout(() => {
                set((state) => {
                    for (const pieceId of pieceIds)
                        state.animatingPieceIds.delete(pieceId);
                });
                resolve();
            }, 100),
        );
    }

    return {
        animatingPieces: null,
        animatingPieceIds: new Set(),
        removingPieceIds: new Set(),

        async playAnimationBatch(animation) {
            await processMoveAnimation(animation);
        },

        async playAnimation(animation) {
            await processMoveAnimation({
                steps: [animation],
                removedPieceIds: [],
            });
        },

        async animatePiece(pieceId, newPosition, pieces) {
            const newPieces = new BoardPieces(pieces);
            const piece = newPieces.getById(pieceId);
            if (!piece) return;

            newPieces.addAt(piece, newPosition);
            await processMoveAnimation(
                {
                    steps: [
                        {
                            newPieces,
                            movedPieceIds: [pieceId],
                        },
                    ],
                    removedPieceIds: [],
                },
                true,
            );
        },

        clearAnimation() {
            set((state) => {
                state.animatingPieces = null;
            });
        },
    };
};
