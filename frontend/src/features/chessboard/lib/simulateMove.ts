import { pointEquals } from "@/lib/utils/pointUtils";
import { LogicalPoint, Move, PieceID, PieceMap } from "@/types/tempModels";

export function simulateMove(
    pieces: PieceMap,
    move: Move,
): { newPieces: PieceMap; movedPieceIds: Set<PieceID> } {
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

export function pointToPiece(
    pieces: PieceMap,
    position: LogicalPoint,
): PieceID | undefined {
    for (const [id, piece] of pieces) {
        if (pointEquals(piece.position, position)) return id;
    }
}
