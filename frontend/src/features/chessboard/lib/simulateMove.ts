import { pointEquals } from "@/features/point/pointUtils";
import { LogicalPoint } from "@/features/point/types";
import { AnimationStep, MoveAnimation, PieceID } from "./types";
import { Move } from "./types";
import { PieceMap } from "./types";
import { createPieceId } from "./pieceMapUtils";

export function simulateMove(pieces: PieceMap, move: Move): AnimationStep {
    return simulateMoveDetails(pieces, move).step;
}

export function simulateMoveWithIntermediates(
    pieces: PieceMap,
    move: Move,
): MoveAnimation {
    const fromId = pointToPiece(pieces, move.from);
    if (!fromId) return { steps: [], removedPieceIds: [] };

    const steps: AnimationStep[] = [];
    const currentPieces = new Map(pieces);
    for (const intermediate of move.intermediates) {
        const piece = { ...currentPieces.get(fromId)! };
        piece.position = intermediate.position;
        currentPieces.set(fromId, piece);

        steps.push({
            newPieces: new Map(currentPieces),
            movedPieceIds: [fromId],
            isCapture: intermediate.isCapture,
        });
    }

    const mainMoveAnimation = simulateMoveDetails(pieces, move);
    steps.push(mainMoveAnimation.step);
    return {
        steps,
        removedPieceIds: mainMoveAnimation.removedPieceIds,
    };
}

function simulateMoveDetails(
    basePieces: PieceMap,
    move: Move,
): { step: AnimationStep; removedPieceIds: Set<PieceID> } {
    const movedPieceIds = new Set<PieceID>();
    const newPieces = new Map(basePieces);

    const fromId = pointToPiece(basePieces, move.from);
    if (fromId) {
        const piece = { ...basePieces.get(fromId)! };
        piece.position = move.to;
        piece.type = move.promotesTo ?? piece.type;

        newPieces.set(fromId, piece);
        movedPieceIds.add(fromId);
    }

    for (const sideEffect of move.sideEffects) {
        const sideEffectId = pointToPiece(basePieces, sideEffect.from);
        if (!sideEffectId) continue;

        const piece = { ...basePieces.get(sideEffectId)! };
        piece.position = sideEffect.to;
        newPieces.set(sideEffectId, piece);
        movedPieceIds.add(sideEffectId);
    }

    const removedPieceIds = new Set<PieceID>();
    const destCaptureId = pointToPiece(basePieces, move.to);
    if (destCaptureId && !movedPieceIds.has(destCaptureId)) {
        removedPieceIds.add(destCaptureId);
        newPieces.delete(destCaptureId);
    }
    for (const capture of move.captures) {
        const captureId = pointToPiece(basePieces, capture);
        if (captureId) {
            newPieces.delete(captureId);
            removedPieceIds.add(captureId);
        }
    }

    const initialSpawnPositions = applySpawns(
        newPieces,
        basePieces,
        move,
        movedPieceIds,
    );

    return {
        step: {
            newPieces,
            movedPieceIds: [...movedPieceIds],
            initialSpawnPositions,
            isCapture:
                removedPieceIds.size > 0 &&
                move.intermediates.filter((x) => x.isCapture).length <
                    removedPieceIds.size,
        },
        removedPieceIds,
    };
}

function applySpawns(
    newPieces: PieceMap,
    basePieces: PieceMap,
    move: Move,
    movedPieceIds: Set<PieceID>,
): PieceMap | undefined {
    if (move.pieceSpawns.length === 0) return;

    const initialSpawnPositions = new Map(basePieces);
    for (const piece of move.pieceSpawns) {
        const id = createPieceId();
        newPieces.set(id, piece);
        initialSpawnPositions.set(id, { ...piece, position: move.from });

        movedPieceIds.add(id);
    }
    return initialSpawnPositions;
}

export function pointToPiece(
    pieces: PieceMap,
    position: LogicalPoint,
): PieceID | undefined {
    for (const [id, piece] of pieces) {
        if (pointEquals(piece.position, position)) return id;
    }
}
