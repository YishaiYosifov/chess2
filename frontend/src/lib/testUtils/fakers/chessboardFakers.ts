import { GameColor } from "@/lib/apiClient";
import { pointToStr } from "@/lib/utils/pointUtils";
import {
    LegalMoveMap,
    Move,
    Piece,
    PieceMap,
    PieceType,
    Point,
} from "@/types/tempModels";
import { faker } from "@faker-js/faker";

const allPoints = Array.from({ length: 100 }, (_, i) => ({
    x: i % 10,
    y: Math.floor(i / 10),
}));

export function createUniquePoint(): Point {
    if (allPoints.length === 0)
        throw new Error("No more unique points available");

    const index = faker.number.int({ min: 0, max: allPoints.length - 1 });
    return allPoints.splice(index, 1)[0];
}

export function createPiece(override?: Partial<Piece>): Piece {
    return {
        type: faker.helpers.enumValue(PieceType),
        color: faker.helpers.enumValue(GameColor),
        position: createUniquePoint(),
        ...override,
    };
}

export function createMove(override?: Partial<Move>): Move {
    return {
        from: createUniquePoint(),
        to: createUniquePoint(),
        through: [createUniquePoint(), createUniquePoint()],
        captures: [createUniquePoint()],
        sideEffects: [],
        ...override,
    };
}

export function createMoveFromPieces(
    pieces: PieceMap,
    override?: Partial<Move>,
): Move {
    return createMove({
        from: pieces.values().toArray()[0].position,
        ...override,
    });
}

export function createPieceMap(count = 5): PieceMap {
    const map: PieceMap = new Map();
    for (let i = 0; i < count; i++) {
        map.set(`${i}`, createPiece());
    }
    return map;
}

export function createPieceMapFromPieces(...pieces: Piece[]): PieceMap {
    const map: PieceMap = new Map();
    for (const [i, piece] of pieces.entries()) {
        map.set(`${i}`, piece);
    }
    return map;
}

export function createLegalMoveMap(...pieces: Piece[]): LegalMoveMap {
    const map: LegalMoveMap = new Map();
    for (const piece of pieces) {
        map.set(pointToStr(piece.position), [
            createMove({ from: piece.position }),
            createMove({ from: piece.position }),
        ]);
    }
    return map;
}
