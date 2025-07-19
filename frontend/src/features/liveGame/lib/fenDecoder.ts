import { PieceID, PieceMap, PieceType } from "@/types/tempModels";
import { GameColor } from "@/lib/apiClient";

/**
 * Parse a fen into a PieceMap
 *
 * @param fen - the fen to convert to a map
 * @returns the board as a map
 */
export function decodeFen(fen: string): PieceMap {
    const board: PieceMap = new Map();
    const ranks = fen.split("/").reverse();

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
                position: { x, y },
                type: pieceType,
                color,
            });
            x++;
            pieceIdx++;
        }
    }
    return board;
}
