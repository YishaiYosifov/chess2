import { Move, Point, StrPoint } from "@/types/tempModels";
import { pointToStr } from "../utils/pointUtils";

export function decodeMovesIntoMap(encoded: string[]): Map<StrPoint, Move[]> {
    const moves = new Map<StrPoint, Move[]>();
    for (const encodedMove of encoded) {
        const decodedMove = decodeSingleMove(encodedMove);

        const stringPoint = pointToStr(decodedMove.from);
        const movesFromPoint = moves.get(stringPoint) ?? [];
        movesFromPoint.push(decodedMove);

        moves.set(pointToStr(decodedMove.from), movesFromPoint);
    }
    return moves;
}

export const decodeMoves = (encoded: string[]): Move[] =>
    encoded.map(decodeSingleMove);

export function decodeSingleMove(path: string): Move {
    const sideEffects: Move[] = [];
    const parts = path.split("-");

    const rootPath = parts[0];
    const [mainPart, ...captureParts] = rootPath.split("!");

    const captures = captureParts.map(algebraicToPoint);
    const rootMove = decodePath(mainPart);

    for (let i = 1; i < parts.length; i++) {
        const effectPath = decodeSingleMove(parts[i]);
        sideEffects.push(effectPath);
    }

    return {
        ...rootMove,
        captures,
        sideEffects,
    };
}

function decodePath(path: string): {
    from: Point;
    through: Point[];
    to: Point;
} {
    const algebraicMatch = path.match(/[a-zA-Z]+\d+/g);
    if (!algebraicMatch)
        throw new Error(`Invalid move: could not parse points (${path})`);

    const points = algebraicMatch.map(algebraicToPoint);
    if (points.length < 2)
        throw new Error(`Invalid move: not enough points (${path})`);

    return {
        from: points[0],
        through: points.slice(1, -1),
        to: points[points.length - 1],
    };
}

function algebraicToPoint(algebraic: string): Point {
    const file = algebraic.charCodeAt(0) - "a".charCodeAt(0);
    const rank = parseInt(algebraic.slice(1)) - 1;
    return { x: file, y: rank };
}
