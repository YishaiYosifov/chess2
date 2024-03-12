import { Color } from "@/client";
import styles from "./ChessPiece.module.scss";
import { Point, PieceInfo } from "./chess.types";

export const ChessPiece = ({
    position,
    pieceInfo,
    boardWidth,
    boardHeight,
    fixed = false,
    side = Color.White,
}: {
    position: Point;
    pieceInfo: PieceInfo;
    boardWidth: number;
    boardHeight: number;
    fixed?: boolean;
    side?: Color;
}) => {
    if (side == Color.Black) position[1] = boardHeight - position[1] - 1;

    const boardSize = boardWidth * boardHeight;

    const [x, y] = position;
    const [physicalX, physicalY] = [
        x * boardWidth * boardHeight,
        y * boardSize,
    ];

    return (
        <div
            data-testid="piece"
            className={styles.piece}
            style={{
                backgroundImage: `url("/assets/pieces/${pieceInfo.pieceType}-${pieceInfo.color}.png")`,
                transform: `translate(${physicalX}%, ${physicalY}%)`,
            }}
        />
    );
};

export default ChessPiece;
