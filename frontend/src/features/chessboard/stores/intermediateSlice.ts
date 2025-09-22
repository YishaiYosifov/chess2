import type { ChessboardStore } from "./chessboardStore";
import { LogicalPoint, StrPoint } from "@/features/point/types";
import { Move, Piece, PieceID } from "../lib/types";
import { StateCreator } from "zustand";
import {
    pointArraysEqual,
    pointArrayStartsWith,
    pointEquals,
    pointToStr,
} from "@/features/point/pointUtils";

export interface IntermediateSlice {
    nextIntermediates: LogicalPoint[];
    intermediateVisited: LogicalPoint[];

    resolveNextIntermediate: ((move: LogicalPoint | null) => void) | null;
    disambiguateDestination(
        dest: LogicalPoint,
        moves: Move[],
        pieceId: PieceID,
        piece: Piece,
    ): Promise<Move[] | null>;
}

export const createIntermediateSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    IntermediateSlice
> = (set, get) => ({
    nextIntermediates: [],
    intermediateVisited: [],
    resolveNextIntermediate: null,

    async disambiguateDestination(dest, moves, pieceId, piece) {
        if (moves.length == 0 || get().nextIntermediates.length > 0)
            return null;

        const { animatePiece, clearAnimation } = get();
        const visited: LogicalPoint[] = [dest];
        try {
            while (true) {
                moves = filterMovesByVisited(moves, visited, dest);
                if (moves.length === 0) return null;

                const cantDisambiguateFurther =
                    movesAreIndistinguishable(moves);
                if (cantDisambiguateFurther) return moves;

                const nextIntermediates = computeNextIntermediates(moves, dest);
                if (nextIntermediates.length === 0) return moves;

                animatePiece(pieceId, piece, dest);
                const choice = await new Promise<LogicalPoint | null>(
                    (resolve) => {
                        set((state) => {
                            state.nextIntermediates = nextIntermediates;
                            state.resolveNextIntermediate = resolve;
                        });
                    },
                );

                // if move cancelled
                if (!choice) return null;

                // if we click on the same square
                // finish by returning all moves whose intermediates match what we've visited
                // and whose destination is the square we clicked on
                if (pointEquals(choice, dest)) {
                    const movesThatEndHere = moves.filter((move) =>
                        pointEquals(move.to, dest),
                    );
                    return movesThatEndHere;
                }

                dest = choice;
                visited.push(choice);
            }
        } finally {
            set((state) => {
                state.nextIntermediates = [];
                state.resolveNextIntermediate = null;
            });
            clearAnimation();
        }
    },
});

function filterMovesByVisited(
    moves: Move[],
    visited: LogicalPoint[],
    dest: LogicalPoint,
): Move[] {
    return moves.filter((move) => {
        if (
            (move.intermediates.length === 0 && pointEquals(move.to, dest)) ||
            move.triggers.some((p) => pointEquals(p, dest))
        )
            return true;
        return pointArrayStartsWith([...move.intermediates, move.to], visited);
    });
}

function movesAreIndistinguishable(moves: Move[]): boolean {
    return (
        moves.length === 1 ||
        moves.every(
            (move) =>
                pointArraysEqual(move.intermediates, moves[0].intermediates) &&
                pointEquals(move.to, moves[0].to),
        )
    );
}

function computeNextIntermediates(
    moves: Move[],
    dest: LogicalPoint,
): LogicalPoint[] {
    const seen = new Set<StrPoint>();
    const nextIntermediates: LogicalPoint[] = [];
    for (const move of moves) {
        const index = move.intermediates.findIndex((intermediate) =>
            pointEquals(intermediate, dest),
        );
        const next =
            index !== -1 && index + 1 < move.intermediates.length
                ? move.intermediates[index + 1]
                : move.to;

        const strPoint = pointToStr(next);
        if (!seen.has(strPoint)) {
            seen.add(strPoint);
            nextIntermediates.push(next);
        }
    }

    return nextIntermediates;
}
