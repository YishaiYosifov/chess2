import {
    Color,
    PieceID,
    PieceMap,
    PieceType,
    Point,
    StrPoint,
} from "@/types/tempModels";

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
    for (const [yCoord, rank] of ranks.entries()) {
        // split the rank into numbers and pieces.
        // this regex makes sure multiple digits are grouped together
        const squares = rank.match(/[a-zA-Z]|\d+/g)!;

        let xCoord = 0;
        for (const square of squares) {
            // if the square is a digit, skip that amount of squares
            const numSquare = Number(square);
            if (numSquare) {
                xCoord += numSquare;
                continue;
            }

            const pieceId = pieceIdx.toString() as PieceID;
            const color =
                square == square.toUpperCase() ? Color.White : Color.Black;
            const pieceType = square.toLowerCase() as PieceType;

            board.set(pieceId, {
                position: [xCoord, yCoord],
                pieceType,
                color,
            });
            xCoord += 1;
            pieceIdx += 1;
        }
    }
    return board;
}

export function stringToPoint(point: StrPoint): Point {
    return point.split(",").map((x) => Number(x)) as Point;
}

export function pointToString(point: Point): StrPoint {
    return point.toString() as StrPoint;
}
