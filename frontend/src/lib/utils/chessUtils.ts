import {
    Move,
    PieceID,
    PieceMap,
    PieceType,
    Point,
    StrPoint,
} from "@/types/tempModels";
import { GameColor } from "../apiClient";

/**
 * Parse a fen into a PieceMap
 *
 * @param fen - the fen to convert to a map
 * @returns the board as a map
 */
export function parseFen(fen: string): PieceMap {
    const board: PieceMap = new Map();
    const ranks = fen.split("/");

    let pieceIdx = 0;
    for (const [y, rank] of ranks.entries()) {
        // split the rank into numbers and pieces.
        // this regex makes sure multiple digits are grouped together
        const squares = rank.match(/[a-zA-Z]|\d+/g)!;

        let x = 0;
        for (const square of squares) {
            // if the square is a digit, skip that amount of squares
            const numSquare = Number(square);
            if (numSquare) {
                x += numSquare;
                continue;
            }

            const pieceId = pieceIdx.toString() as PieceID;
            const color =
                square == square.toUpperCase()
                    ? GameColor.WHITE
                    : GameColor.BLACK;
            const pieceType = square.toLowerCase() as PieceType;

            board.set(pieceId, {
                position: [x, y],
                type: pieceType,
                color,
            });
            x++;
            pieceIdx++;
        }
    }
    return board;
}

export function decodeLegalMoves(encoded: string): Map<StrPoint, Move[]> {
    const cleanedEncoded = encoded.trim().split(/\s+/).filter(Boolean);
    const moves = new Map<StrPoint, Move[]>();
    for (const encodedMove of cleanedEncoded) {
        const decodedMove = parseMove(encodedMove);

        const stringPoint = pointToString(decodedMove.from);
        const movesFromPoint = moves.get(stringPoint) ?? [];
        movesFromPoint.push(decodedMove);

        moves.set(pointToString(decodedMove.from), movesFromPoint);
    }
    return moves;
}

function parseMove(moveStr: string): Move {
    let captures: Point[] = [];
    const [main, ...captureParts] = moveStr.split("!");
    if (captureParts.length > 0) {
        captures = captureParts.map(uciToPoint);
    }

    const sideEffects: Move[] = [];
    const mainParts = main.split("-");

    const rootPath = decodePath(mainParts[0]);

    for (let i = 1; i < mainParts.length; i++) {
        const effectPath = parseMove(mainParts[i]);
        sideEffects.push(effectPath);
    }

    return {
        ...rootPath,
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
    const rank = parseInt(uci[1], 10) - 1;
    return [file, rank];
}

export function stringToPoint(point: StrPoint): Point {
    return point.split(",").map((x) => Number(x)) as Point;
}

export function pointToString(point: Point): StrPoint {
    return point.toString() as StrPoint;
}
