import { GameColor } from "@/lib/apiClient";
import { logicalPoint } from "@/features/point/pointUtils";
import constants from "@/lib/constants";
import { createPieceId } from "./pieceUtils";
import BoardPieces from "./boardPieces";

/**
 * Parse a fen into BoardPieces
 *
 * @param fen - the fen to convert to a map
 * @returns the board as a map
 */
export function decodeFen(fen: string): BoardPieces {
    const board = new BoardPieces();
    const ranks = fen.split("/").reverse();

    for (const [y, rank] of ranks.entries()) {
        // split the rank into numbers and pieces.
        // this regex makes sure multiple digits are grouped together
        const squares = rank.match(/\d+|[^0-9]/g);
        if (!squares) continue;

        let x = 0;
        for (const square of squares) {
            // if the square is a digit, skip that amount of squares
            const numSquare = Number(square);
            if (numSquare) {
                x += numSquare;
                continue;
            }

            const pieceId = createPieceId();
            const color = getColorFromLetter(square);
            const pieceLetter = square.toLowerCase();
            const pieceType = constants.LETTER_TO_PIECE[pieceLetter];

            board.add({
                id: pieceId,
                position: logicalPoint({ x, y }),
                type: pieceType,
                color,
            });
            x++;
        }
    }
    return board;
}

function getColorFromLetter(letter: string): GameColor | null {
    if (!isLetter(letter)) return null;
    return letter === letter.toUpperCase() ? GameColor.WHITE : GameColor.BLACK;
}

function isLetter(char: string): boolean {
    const code = char.charCodeAt(0);
    return (code >= 65 && code <= 90) || (code >= 97 && code <= 122);
}
