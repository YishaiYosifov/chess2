import { pointToStr } from "@/features/point/pointUtils";
import { AnimationStep, MoveAnimation, PieceID } from "./types";
import { Move } from "./types";
import BoardPieces from "./boardPieces";

export function simulateMove(pieces: BoardPieces, move: Move): AnimationStep {
    return simulateMoveDetails(pieces, move).step;
}

export function simulateMoveWithIntermediates(
    pieces: BoardPieces,
    move: Move,
): MoveAnimation {
    const fromPiece = pieces.getByPosition(move.from);
    if (!fromPiece) return { steps: [], removedPieceIds: [] };

    const steps: AnimationStep[] = [];
    const currentPieces = new BoardPieces(pieces);
    for (const intermediate of move.intermediates) {
        currentPieces.move(fromPiece.id, intermediate.position);

        steps.push({
            newPieces: new BoardPieces(currentPieces),
            movedPieceIds: [fromPiece.id],
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
    basePieces: BoardPieces,
    move: Move,
): { step: AnimationStep; removedPieceIds: PieceID[] } {
    const movedPieceIds = new Set<PieceID>();
    const newPieces = new BoardPieces(basePieces);

    const movingPiece = basePieces.getByPosition(move.from);
    if (movingPiece) {
        newPieces.move(movingPiece.id, move.to, move.promotesTo);
        movedPieceIds.add(movingPiece.id);
    } else {
        console.warn("Could not find piece to move at", pointToStr(move.from));
    }

    for (const sideEffect of move.sideEffects) {
        const sideEffectPiece = basePieces.getByPosition(sideEffect.from);
        if (!sideEffectPiece) {
            console.warn(
                "Could not find side effect piece at",
                pointToStr(sideEffect.from),
            );
            continue;
        }

        newPieces.move(sideEffectPiece.id, sideEffect.to);
        movedPieceIds.add(sideEffectPiece.id);
    }

    const removedPieceIds: PieceID[] = [];
    const destCapturePiece = basePieces.getByPosition(move.to);
    if (destCapturePiece && !movedPieceIds.has(destCapturePiece.id)) {
        removedPieceIds.push(destCapturePiece.id);
        newPieces.delete(destCapturePiece.id);
    }
    for (const capture of move.captures) {
        const capturedPiece = basePieces.getByPosition(capture);
        if (capturedPiece) {
            newPieces.delete(capturedPiece.id);
            removedPieceIds.push(capturedPiece.id);
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
                removedPieceIds.length > 0 &&
                move.intermediates.filter((x) => x.isCapture).length <
                    removedPieceIds.length,
        },
        removedPieceIds,
    };
}

function applySpawns(
    newPieces: BoardPieces,
    basePieces: BoardPieces,
    move: Move,
    movedPieceIds: Set<PieceID>,
): BoardPieces | undefined {
    if (move.pieceSpawns.length === 0) return;

    const initialSpawnPositions = new BoardPieces(basePieces);
    for (const piece of move.pieceSpawns) {
        newPieces.add(piece);
        initialSpawnPositions.addAt(piece, move.from);

        movedPieceIds.add(piece.id);
    }
    return initialSpawnPositions;
}
