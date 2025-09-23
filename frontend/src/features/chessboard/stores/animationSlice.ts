import { StateCreator } from "zustand";
import type { ChessboardStore } from "./chessboardStore";
import { Piece, PieceID, PieceMap } from "../lib/types";
import { LogicalPoint } from "@/features/point/types";
import { MoveResult } from "../lib/simulateMove";
import LastOneWinsAsyncLock from "@/lib/lastOneWinsAsyncLock";
import { Draft } from "immer";

export interface AnimationSlice {
    animatingPieceMap: PieceMap | null;
    animatingPieces: Set<PieceID>;

    cycleAnimatingPieceMap(positions: MoveResult[]): Promise<void>;
    animatePiece(
        pieceId: PieceID,
        piece: Piece,
        newPosition: LogicalPoint,
    ): Promise<void>;
    clearAnimation(): void;
    addAnimatingPieces(...pieceIds: PieceID[]): Promise<void>;
}

export const createAnimationSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    AnimationSlice
> = (set, get) => {
    const animationLock = new LastOneWinsAsyncLock();

    return {
        animatingPieceMap: null,
        animatingPieces: new Set(),

        async cycleAnimatingPieceMap(positions) {
            const { clearAnimation } = get();

            await animationLock.acquire(async () => {
                for (const { movedPieceIds, newPieces } of positions) {
                    set((state) => {
                        state.animatingPieceMap = newPieces;
                    });

                    await animateOnePiece(set, ...movedPieceIds);
                }
                clearAnimation();
            });
        },

        async animatePiece(pieceId, piece, newPosition) {
            const { addAnimatingPieces: addAnimatingPiece } = get();

            set((state) => {
                state.animatingPieceMap ??= new Map();
                state.animatingPieceMap.set(pieceId, {
                    ...piece,
                    position: newPosition,
                });
            });
            await addAnimatingPiece(pieceId);
        },

        clearAnimation() {
            set((state) => {
                state.animatingPieceMap = null;
            });
        },

        addAnimatingPieces(...pieceIds) {
            ensureAnimatingPieces(set, ...pieceIds);
            return animationLock.acquire(() =>
                animateOnePiece(set, ...pieceIds),
            );
        },
    };
};

async function animateOnePiece(
    set: (fn: (state: Draft<ChessboardStore>) => void) => void,
    ...pieceIds: PieceID[]
) {
    ensureAnimatingPieces(set, ...pieceIds);

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

function ensureAnimatingPieces(
    set: (fn: (state: Draft<ChessboardStore>) => void) => void,
    ...pieceIds: PieceID[]
) {
    set((state) => {
        for (const pieceId of pieceIds) state.animatingPieces.add(pieceId);
    });
}
