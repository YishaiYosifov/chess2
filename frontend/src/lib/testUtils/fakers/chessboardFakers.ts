import { GameColor, PieceType } from "@/lib/apiClient";
import { logicalPoint, pointToStr } from "@/features/point/pointUtils";
import { LogicalPoint } from "@/features/point/types";
import { Move } from "@/features/chessboard/lib/types";
import { LegalMoveMap } from "@/features/chessboard/lib/types";
import { Piece } from "@/features/chessboard/lib/types";
import { faker } from "@faker-js/faker";
import BoardPieces from "@/features/chessboard/lib/boardPieces";

export function createRandomPoint(): LogicalPoint {
    return logicalPoint({
        x: faker.number.int({ min: 0, max: 99 }),
        y: faker.number.int({ min: 0, max: 99 }),
    });
}

export function createFakePiece(override?: Partial<Piece>): Piece {
    return {
        id: faker.string.uuid(),
        type: faker.helpers.enumValue(PieceType),
        color: faker.helpers.enumValue(GameColor),
        position: createRandomPoint(),
        ...override,
    };
}

export function createFakeMove(override?: Partial<Move>): Move {
    return {
        from: createRandomPoint(),
        to: createRandomPoint(),
        moveKey: faker.string.alpha(10),
        triggers: [],
        captures: [],
        intermediates: [],
        sideEffects: [],
        pieceSpawns: [],
        promotesTo: null,
        ...override,
    };
}

export function createFakeMoveFromPieces(
    pieces: BoardPieces,
    override?: Partial<Move>,
): Move {
    const firstPiece = pieces.values().next().value;
    if (!firstPiece) throw new Error("BoardPieces is empty");

    return createFakeMove({
        from: firstPiece.position,
        ...override,
    });
}

export function createFakeBoardPieces(count = 5): BoardPieces {
    const boardPieces = new BoardPieces();
    for (let i = 0; i < count; i++) {
        boardPieces.add(createFakePiece({ id: i.toString() }));
    }
    return boardPieces;
}

export function createFakeLegalMoveMap(count = 5): LegalMoveMap {
    const pieces: Piece[] = [];
    for (let i = 0; i < count; i++) pieces.push(createFakePiece());

    return createFakeLegalMoveMapFromPieces(...pieces);
}

export function createFakeLegalMoveMapFromPieces(
    ...pieces: Piece[]
): LegalMoveMap {
    const map: LegalMoveMap = new Map();
    for (const piece of pieces) {
        map.set(pointToStr(piece.position), [
            createFakeMove({ from: piece.position }),
            createFakeMove({ from: piece.position }),
        ]);
    }
    return map;
}

export function createFakeLegalMoveMapFromMoves(moves: Move[]): LegalMoveMap {
    const map: LegalMoveMap = new Map();
    for (const move of moves) {
        map.set(pointToStr(move.from), [move]);
    }
    return map;
}
