import { Move, Point, StrPoint } from "@/types/tempModels";
import { pointToString } from "../utils/pointUtils";

export function decodeLegalMoves(encoded: string[]): Map<StrPoint, Move[]> {
    const moves = new Map<StrPoint, Move[]>();
    for (const encodedMove of encoded) {
        const decodedMove = parseMove(encodedMove);

        const stringPoint = pointToString(decodedMove.from);
        const movesFromPoint = moves.get(stringPoint) ?? [];
        movesFromPoint.push(decodedMove);

        moves.set(pointToString(decodedMove.from), movesFromPoint);
    }
    return moves;
}

function parseMove(path: string): Move {
    const sideEffects: Move[] = [];
    const parts = path.split("-");

    const rootPath = parts[0];
    const [mainPart, ...captureParts] = rootPath.split("!");

    const captures = captureParts.map(uciToPoint);
    const rootMove = decodePath(mainPart);

    for (let i = 1; i < parts.length; i++) {
        const effectPath = parseMove(parts[i]);
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
    const uciMatch = path.match(/[a-zA-Z]+\d+/g);
    if (!uciMatch)
        throw new Error(`Invalid move: could not parse points  (${path})`);

    const points = uciMatch.map(uciToPoint);
    if (points.length < 2)
        throw new Error(`Invalid move: not enough points (${path})`);

    return {
        from: points[0],
        through: points.slice(1, -1),
        to: points[points.length - 1],
    };
}

function uciToPoint(uci: string): Point {
    const file = uci.charCodeAt(0) - "a".charCodeAt(0);
    const rank = parseInt(uci[1]) - 1;
    return [file, rank];
}
