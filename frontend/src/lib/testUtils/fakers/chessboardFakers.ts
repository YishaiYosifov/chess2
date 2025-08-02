import { GameColor, PieceType } from "@/lib/apiClient";
import { logicalPoint, pointToStr } from "@/lib/utils/pointUtils";
import { LogicalPoint } from "@/features/point/types";
import { Move } from "@/features/chessboard/lib/types";
import { LegalMoveMap } from "@/features/chessboard/lib/types";
import { PieceMap } from "@/features/chessboard/lib/types";
import { Piece } from "@/features/chessboard/lib/types";
import { faker } from "@faker-js/faker";

export function createRandomPoint(): LogicalPoint {
    return logicalPoint({
        x: faker.number.int({ min: 0, max: 99 }),
        y: faker.number.int({ min: 0, max: 99 }),
    });
}

export function createFakePiece(override?: Partial<Piece>): Piece {
    return {
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
        triggers: [createRandomPoint(), createRandomPoint()],
        captures: [createRandomPoint()],
        sideEffects: [],
        promotesTo: faker.helpers.enumValue(PieceType),
        ...override,
    };
}

export function createFakeMoveFromPieces(
    pieces: PieceMap,
    override?: Partial<Move>,
): Move {
    return createFakeMove({
        from: pieces.values().toArray()[0].position,
        ...override,
    });
}

export function createFakePieceMap(count = 5): PieceMap {
    const map: PieceMap = new Map();
    for (let i = 0; i < count; i++) {
        map.set(`${i}`, createFakePiece());
    }
    return map;
}

export function createFakePieceMapFromPieces(...pieces: Piece[]): PieceMap {
    const map: PieceMap = new Map();
    for (const [i, piece] of pieces.entries()) {
        map.set(`${i}`, piece);
    }
    return map;
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
