import { pointEquals } from "@/features/point/pointUtils";
import { LogicalPoint } from "@/features/point/types";
import { MoveAnimation, PieceID } from "./types";
import { Move } from "./types";
import { PieceMap } from "./types";

export function simulateMove(pieces: PieceMap, move: Move): MoveAnimation {
    const movedPieceIds = new Set<PieceID>();
    const newPieces = new Map(pieces);

    const fromId = pointToPiece(pieces, move.from);
    if (fromId) {
        const piece = { ...pieces.get(fromId)! };
        piece.position = move.to;
        piece.type = move.promotesTo ?? piece.type;

        newPieces.set(fromId, piece);
        movedPieceIds.add(fromId);
    }

    for (const sideEffect of move.sideEffects) {
        const sideEffectId = pointToPiece(pieces, sideEffect.from);
        if (!sideEffectId) continue;

        const piece = { ...pieces.get(sideEffectId)! };
        piece.position = sideEffect.to;
        newPieces.set(sideEffectId, piece);
        movedPieceIds.add(sideEffectId);
    }

    const destCaptureId = pointToPiece(pieces, move.to);
    if (destCaptureId && !movedPieceIds.has(destCaptureId))
        newPieces.delete(destCaptureId);
    for (const capture of move.captures) {
        const captureId = pointToPiece(pieces, capture);
        if (captureId) newPieces.delete(captureId);
    }

    return { newPieces, movedPieceIds: [...movedPieceIds] };
}

export function simulateMoveWithIntermediates(
    pieces: PieceMap,
    move: Move,
): MoveAnimation[] {
    const fromId = pointToPiece(pieces, move.from);
    if (!fromId) return [];

    const results: MoveAnimation[] = [];
    const currentPieces = new Map(pieces);
    for (const intermediate of move.intermediates) {
        const piece = { ...currentPieces.get(fromId)! };
        piece.position = intermediate;
        currentPieces.set(fromId, piece);

        results.push({
            newPieces: new Map(currentPieces),
            movedPieceIds: [fromId],
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
