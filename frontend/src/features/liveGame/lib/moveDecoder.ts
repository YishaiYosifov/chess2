import brotliDecompress from "brotli/decompress";

import { MoveSideEffect } from "@/features/chessboard/lib/types";
import { Move } from "@/features/chessboard/lib/types";
import { LegalMoveMap } from "@/features/chessboard/lib/types";
import { MovePath, MoveSideEffectPath } from "@/lib/apiClient";
import { idxToLogicalPoint, pointToStr } from "@/features/point/pointUtils";

export function decodePathIntoMap(
    paths: MovePath[],
    boardWidth: number,
): LegalMoveMap {
    const moves: LegalMoveMap = new Map();
    for (const path of paths) {
        const move = decodePath(path, boardWidth);
        const fromString = pointToStr(move.from);
        const movesFromPoint = moves.get(fromString) ?? [];
        movesFromPoint.push(move);

        moves.set(pointToStr(move.from), movesFromPoint);
    }
    return moves;
}

export function decodePath(path: MovePath, boardWidth: number): Move {
    const from = idxToLogicalPoint(path.fromIdx, boardWidth);
    const to = idxToLogicalPoint(path.toIdx, boardWidth);
    const triggers =
        path.triggerIdxs?.map((idx) => idxToLogicalPoint(idx, boardWidth)) ??
        [];
    const captures =
        path.capturedIdxs?.map((idx) => idxToLogicalPoint(idx, boardWidth)) ??
        [];
    const intermediates =
        path.intermediateIdxs?.map((idx) =>
            idxToLogicalPoint(idx, boardWidth),
        ) ?? [];
    const sideEffects =
        path.sideEffects?.map((m) => sideEffectToMove(m, boardWidth)) ?? [];

    return {
        from,
        to,
        triggers,
        captures,
        intermediates,
        sideEffects,
        promotesTo: path.promotesTo ?? null,
    };
}

function sideEffectToMove(
    path: MoveSideEffectPath,
    boardWidth: number,
): MoveSideEffect {
    const from = idxToLogicalPoint(path.fromIdx, boardWidth);
    const to = idxToLogicalPoint(path.toIdx, boardWidth);
    return {
        from,
        to,
    };
}

export function decodeEncodedMovesIntoMap(
    encoded: string,
    boardWidth: number,
): LegalMoveMap {
    const buffer = Buffer.from(encoded, "base64");
    const decompressed = brotliDecompress(buffer);
    const decoded = new TextDecoder().decode(decompressed);
    const moves = decodePathIntoMap(JSON.parse(decoded), boardWidth);
    return moves;
}
