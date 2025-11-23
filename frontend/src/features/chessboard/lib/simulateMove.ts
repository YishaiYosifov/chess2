import { AnimationStep, MoveAnimation, PieceID } from "./types";
import BoardPieces from "./boardPieces";
import { Move } from "./types";

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
        currentPieces.movePiece(fromPiece.id, intermediate.position);

        steps.push({
            newPieces: new BoardPieces(currentPieces),
            movedPieceIds: [fromPiece.id],
            isCapture: intermediate.isCapture,
            specialMoveType: move.specialMoveType,
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
    const newPieces = new BoardPieces(basePieces);
    const { movedPieceIds, removedPieceIds } = newPieces.playMove(move);

    const initialSpawnPositions = createInitialSpawns(basePieces, move);

    return {
        step: {
            newPieces,
            movedPieceIds: [...movedPieceIds],
            initialSpawnPositions,
            isCapture:
                removedPieceIds.length > 0 &&
                move.intermediates.filter((x) => x.isCapture).length <
                    removedPieceIds.length,
            isPromotion: move.promotesTo !== null,
            specialMoveType: move.specialMoveType,
        },
        removedPieceIds,
    };
}

function createInitialSpawns(
    basePieces: BoardPieces,
    move: Move,
): BoardPieces | undefined {
    if (move.pieceSpawns.length === 0) return;

    const initialSpawnPositions = new BoardPieces(basePieces);
    for (const piece of move.pieceSpawns) {
        initialSpawnPositions.addAt(piece, move.from);
    }
    return initialSpawnPositions;
}
