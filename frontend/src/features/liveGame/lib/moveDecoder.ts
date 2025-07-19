import { LegalMoveMap, Move, MoveSideEffect } from "@/types/tempModels";
import { MovePath, MoveSideEffectPath } from "@/lib/apiClient";
import { idxToPoint, pointToStr } from "@/lib/utils/pointUtils";
import { gunzipSync } from "zlib";

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
    const from = idxToPoint(path.fromIdx, boardWidth);
    const to = idxToPoint(path.toIdx, boardWidth);
    const triggers =
        path.triggerIdxs?.map((idx) => idxToPoint(idx, boardWidth)) ?? [];
    const captures =
        path.capturedIdxs?.map((idx) => idxToPoint(idx, boardWidth)) ?? [];
    const sideEffects =
        path.sideEffects?.map((m) => sideEffectToMove(m, boardWidth)) ?? [];

    return {
        from,
        to,
        triggers,
        captures,
        sideEffects,
    };
}

function sideEffectToMove(
    path: MoveSideEffectPath,
    boardWidth: number,
): MoveSideEffect {
    const from = idxToPoint(path.fromIdx, boardWidth);
    const to = idxToPoint(path.toIdx, boardWidth);
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
    const decoded = gunzipSync(buffer).toString();
    const moves = decodePathIntoMap(JSON.parse(decoded), boardWidth);
    return moves;
}
