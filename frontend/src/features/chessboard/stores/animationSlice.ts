import { StateCreator } from "zustand";
import type { ChessboardStore } from "./chessboardStore";
import { MoveAnimation, Piece, PieceID, PieceMap } from "../lib/types";
import { LogicalPoint } from "@/features/point/types";

export interface AnimationSlice {
    animatingPieceMap: PieceMap | null;
    animatingPieces: Set<PieceID>;

    playAnimationBatch(positions: MoveAnimation[]): Promise<void>;
    animatePiece(
        pieceId: PieceID,
        piece: Piece,
        newPosition: LogicalPoint,
    ): Promise<void>;
    clearAnimation(): void;
}
export const createAnimationSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    AnimationSlice
> = (set) => {
    let currentAnimationCancelToken: { canceled: boolean } | null = null;

    async function processMoveAnimation(
        animation: MoveAnimation[],
        persistent: boolean = false,
    ) {
        if (currentAnimationCancelToken) {
            currentAnimationCancelToken.canceled = true;
        }

        const cancelToken = { canceled: false };
        currentAnimationCancelToken = cancelToken;

        for (const { movedPieceIds, newPieces } of animation) {
            if (cancelToken.canceled) break;

            set((state) => {
                state.animatingPieceMap = newPieces;
            });
            await markPiecesAsAnimating(movedPieceIds);
        }

        if (!cancelToken.canceled && !persistent) {
            set((state) => {
                state.animatingPieceMap = null;
            });
        }

        if (currentAnimationCancelToken === cancelToken) {
            currentAnimationCancelToken = null;
        }
    }

    async function markPiecesAsAnimating(pieceIds: PieceID[]) {
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

        async playAnimationBatch(positions) {
            await processMoveAnimation(positions);
        },

        async animatePiece(pieceId, piece, newPosition) {
            await processMoveAnimation(
                [
                    {
                        newPieces: new Map([
                            [
                                pieceId,
                                {
                                    ...piece,
                                    position: newPosition,
                                },
                            ],
                        ]),
                        movedPieceIds: [pieceId],
                    },
                ],
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
