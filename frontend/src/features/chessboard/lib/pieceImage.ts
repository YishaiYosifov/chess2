import { GameColor, PieceType } from "@/lib/apiClient";

export default function getPieceImage(
    type: PieceType,
    color: GameColor | null,
): string {
    const pieceName = PieceType[type].toLowerCase();
    const pieceColor =
        color === null ? "neutral" : GameColor[color].toLowerCase();
    return `/assets/pieces/${pieceName}_${pieceColor}.png`;
}
