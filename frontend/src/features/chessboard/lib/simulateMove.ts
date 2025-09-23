import { pointEquals } from "@/features/point/pointUtils";
import { LogicalPoint } from "@/features/point/types";
import { PieceID } from "./types";
import { Move } from "./types";
import { PieceMap } from "./types";

export interface MoveResult {
    newPieces: PieceMap;
    movedPieceIds: Set<PieceID>;
}

export function simulateMove(pieces: PieceMap, move: Move): MoveResult {
    const movedPieceIds = new Set<PieceID>();
    const newPieces = new Map(pieces);

    const destCaptureId = pointToPiece(pieces, move.to);
    if (destCaptureId) newPieces.delete(destCaptureId);
    for (const capture of move.captures) {
        const captureId = pointToPiece(pieces, capture);
        if (captureId) newPieces.delete(captureId);
    }

    const fromId = pointToPiece(pieces, move.from);
    if (fromId) {
        movedPieceIds.add(fromId);
        const piece = { ...newPieces.get(fromId)! };
        piece.position = move.to;
        piece.type = move.promotesTo ?? piece.type;
        newPieces.set(fromId, piece);
    }

    for (const sideEffect of move.sideEffects) {
        const sideEffectId = pointToPiece(pieces, sideEffect.from);
        if (!sideEffectId) continue;

        const piece = { ...newPieces.get(sideEffectId)! };
        piece.position = sideEffect.to;
        newPieces.set(sideEffectId, piece);
        movedPieceIds.add(sideEffectId);
    }

    return { newPieces, movedPieceIds };
}

export function simulateMoveWithIntermediates(
    pieces: PieceMap,
    move: Move,
): MoveResult[] {
    const fromId = pointToPiece(pieces, move.from);
    if (!fromId) return [];

    const results: MoveResult[] = [];
    const currentPieces = new Map(pieces);
    for (const intermediate of move.intermediates) {
        const piece = { ...currentPieces.get(fromId)! };
        piece.position = intermediate;
        currentPieces.set(fromId, piece);

        results.push({
            newPieces: new Map(currentPieces),
            movedPieceIds: new Set([fromId]),
        });
    }

    results.push(simulateMove(pieces, move));
    return results;
}

export function pointToPiece(
    pieces: PieceMap,
    position: LogicalPoint,
): PieceID | undefined {
    for (const [id, piece] of pieces) {
        if (pointEquals(piece.position, position)) return id;
    }
}
