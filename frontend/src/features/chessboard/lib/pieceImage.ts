import { GameColor, PieceType } from "@/lib/apiClient";

export default function getPieceImage(
    type: PieceType,
    color: GameColor,
): string {
    const pieceName = PieceType[type].toLowerCase();
    const pieceColor = GameColor[color].toLowerCase();
    return `/assets/pieces/${pieceName}_${pieceColor}.png`;
}
